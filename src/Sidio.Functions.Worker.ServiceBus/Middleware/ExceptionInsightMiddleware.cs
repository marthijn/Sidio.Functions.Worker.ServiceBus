using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sidio.Functions.Worker.ServiceBus.Middleware.Abstractions;

namespace Sidio.Functions.Worker.ServiceBus.Middleware;

/// <summary>
/// The ExceptionInsightMiddleware class is responsible for logging the details of exceptions that occur during the processing of a message.
/// When the maximum delivery count is reached, the message is dead-lettered.
/// The following information is included in the dead-lettered message:
/// - Dead letter reason
/// - Dead letter error description
/// </summary>
public class ExceptionInsightMiddleware : ServiceBusMiddlewareBase
{
    private readonly IOptions<ExceptionInsightMiddlewareOptions> _options;

    private readonly ILogger<ExceptionInsightMiddleware> _logger;

    /// <summary>
    /// Creates a new instance of the ExceptionLoggingMiddleware class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="logger">The logger.</param>
    public ExceptionInsightMiddleware(
        IOptions<ExceptionInsightMiddlewareOptions> options,
        ILogger<ExceptionInsightMiddleware> logger)
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
        var receivedMessage = await serviceBusContextService.GetServiceBusReceivedMessageAsync(context, cancellationToken).ConfigureAwait(false);
        if (receivedMessage == null)
        {
            _logger.LogWarning("{FunctionsWorkerMiddleware} did not receive a message", nameof(ExceptionInsightMiddleware));
            return false;
        }

        var messageActions = await serviceBusContextService.GetServiceBusMessageActionsAsync(context, cancellationToken).ConfigureAwait(false)
                             ?? throw new InvalidOperationException("ServiceBusMessageActions is null");

        if (receivedMessage.DeliveryCount >= _options.Value.MaxDeliveryCount)
        {
            await DeadLetterMessageAsync(
                messageActions,
                receivedMessage,
                $"Message has been retried {receivedMessage.DeliveryCount} times",
                exception,
                cancellationToken).ConfigureAwait(false);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Dead-letters the message.
    /// </summary>
    /// <param name="serviceBusMessageActions">The service bus message actions.</param>
    /// <param name="receivedMessage">The received message.</param>
    /// <param name="deadLetterReason">The dead-letter reason.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    protected virtual Task DeadLetterMessageAsync(
        ServiceBusMessageActions serviceBusMessageActions,
        ServiceBusReceivedMessage receivedMessage,
        string deadLetterReason,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        var errorDescription = exception.Message;
        return serviceBusMessageActions.DeadLetterMessageAsync(
            receivedMessage,
            deadLetterReason: deadLetterReason,
            deadLetterErrorDescription: errorDescription,
            cancellationToken: cancellationToken);
    }
}