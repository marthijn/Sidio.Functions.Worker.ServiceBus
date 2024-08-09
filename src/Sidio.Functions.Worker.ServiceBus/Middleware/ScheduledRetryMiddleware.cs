using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sidio.Functions.Worker.ServiceBus.Middleware.Abstractions;

namespace Sidio.Functions.Worker.ServiceBus.Middleware;

/// <summary>
/// The scheduled retry middleware.
/// </summary>
public class ScheduledRetryMiddleware : ExceptionInsightMiddleware
{
    internal const string ServiceBusClientName = $"{nameof(ScheduledRetryMiddleware)}.ServiceBusClient";

    internal const string DeliveryAttempts = $"{nameof(ScheduledRetryMiddleware)}.DeliveryAttempts";

    private const string OriginalMessageId = $"{nameof(ScheduledRetryMiddleware)}.OriginalMessageId";

    private readonly IOptions<ScheduledRetryMiddlewareOptions> _options;

    private readonly ILogger<ScheduledRetryMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledRetryMiddleware"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="logger">The logger.</param>
    public ScheduledRetryMiddleware(
        IOptions<ScheduledRetryMiddlewareOptions> options,
        ILogger<ScheduledRetryMiddleware> logger)
        : base(options, logger)
    {
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task<bool> OnExceptionAsync(
        FunctionContext context,
        IServiceBusContextService serviceBusContextService,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        var receivedMessage = await serviceBusContextService
            .GetServiceBusReceivedMessageAsync(context, cancellationToken).ConfigureAwait(false);
        if (receivedMessage == null)
        {
            _logger.LogWarning(
                "{FunctionsWorkerMiddleware} did not receive a message",
                nameof(ScheduledRetryMiddleware));
            return false;
        }

        var deliveryAttempts = GetDeliveryAttempts(receivedMessage);
        if (deliveryAttempts < _options.Value.MaxDeliveryCount)
        {
            var serviceBusTrigger = context.GetServiceBusTrigger() ??
                                    throw new InvalidOperationException("ServiceBusTrigger is null");

            var message = CloneMessage(receivedMessage, deliveryAttempts);
            var enqueueTime = GetScheduledEnqueueTimeUtc(deliveryAttempts);

            var client = CreateServiceBusClient(context, serviceBusTrigger);
            var sender = client.CreateSender(serviceBusTrigger.TopicName ?? serviceBusTrigger.QueueName);
            var scheduleResult = await sender.ScheduleMessageAsync(message, enqueueTime, cancellationToken).ConfigureAwait(false);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Message {MessageId} scheduled for delivery at {EnqueueTime} (attempt {Attempt}) with sequenceNumber {SequenceNumber}",
                    message.MessageId,
                    enqueueTime,
                    deliveryAttempts + 1,
                    scheduleResult);
            }

            await sender.CloseAsync(cancellationToken);
            return true;
        }

        var messageActions = await serviceBusContextService.GetServiceBusMessageActionsAsync(context, cancellationToken)
                                 .ConfigureAwait(false)
                             ?? throw new InvalidOperationException("ServiceBusMessageActions is null");

        await DeadLetterMessageAsync(
            messageActions,
            receivedMessage,
            $"Message has been retried {deliveryAttempts} times",
            exception,
            cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static ServiceBusMessage CloneMessage(ServiceBusReceivedMessage serviceBusReceivedMessage, int currentDeliveryAttempts)
    {
        var clonedMessage = new ServiceBusMessage(serviceBusReceivedMessage)
        {
            ApplicationProperties =
            {
                [DeliveryAttempts] = currentDeliveryAttempts + 1,
                [OriginalMessageId] = GetMessageId(serviceBusReceivedMessage)
            }
        };

        return clonedMessage;
    }

    private static int GetDeliveryAttempts(ServiceBusReceivedMessage serviceBusReceivedMessage)
    {
        if (serviceBusReceivedMessage.ApplicationProperties.TryGetValue(DeliveryAttempts, out var deliveryAttempts))
        {
            return (int)deliveryAttempts;
        }

        return 1;
    }

    private static string GetMessageId(ServiceBusReceivedMessage serviceBusReceivedMessage)
    {
        if (serviceBusReceivedMessage.ApplicationProperties.TryGetValue(OriginalMessageId, out var messageId))
        {
            return (string)messageId;
        }

        return serviceBusReceivedMessage.MessageId;
    }

    private static ServiceBusClient CreateServiceBusClient(FunctionContext context, ServiceBusTriggerAttribute serviceBusTrigger)
    {
        var clientProvider = context.InstanceServices.GetService<IServiceBusClientProvider>() ??
                             new DefaultServiceBusClientProvider(context);
        return clientProvider.CreateClient(
            serviceBusTrigger.Connection ?? throw new InvalidOperationException("Service Bus Connection cannot be null or empty"));
    }

    private DateTimeOffset GetScheduledEnqueueTimeUtc(int deliveryAttempts)
    {
        var attempts = deliveryAttempts <= 0 ? 1 : deliveryAttempts;

        return _options.Value.BackoffMode switch
        {
            ScheduledRetryBackoffMode.Linear => DateTimeOffset.UtcNow.AddSeconds(_options.Value.BackoffInSeconds * attempts),
            ScheduledRetryBackoffMode.Exponential => DateTimeOffset.UtcNow.AddSeconds(
                Math.Pow(_options.Value.BackoffInSeconds, attempts)),
            ScheduledRetryBackoffMode.Constant => DateTimeOffset.UtcNow.AddSeconds(_options.Value.BackoffInSeconds),
            _ => throw new NotSupportedException($"BackoffMode {_options.Value.BackoffMode} is not supported")
        };
    }
}