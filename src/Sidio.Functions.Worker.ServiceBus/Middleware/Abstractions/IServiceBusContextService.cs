using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;

namespace Sidio.Functions.Worker.ServiceBus.Middleware.Abstractions;

/// <summary>
/// The service bus context service.
/// </summary>
public interface IServiceBusContextService
{
    /// <summary>
    /// Gets the service bus message actions.
    /// </summary>
    /// <param name="functionContext">The function context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <see cref="ServiceBusMessageActions"/>.</returns>
    Task<ServiceBusMessageActions?> GetServiceBusMessageActionsAsync(FunctionContext functionContext, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the service bus received message.
    /// </summary>
    /// <param name="functionContext">The function context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <see cref="ServiceBusReceivedMessage"/>.</returns>
    Task<ServiceBusReceivedMessage?> GetServiceBusReceivedMessageAsync(FunctionContext functionContext, CancellationToken cancellationToken = default);
}