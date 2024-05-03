using Azure.Messaging.ServiceBus;

namespace Sidio.Functions.Worker.ServiceBus.Middleware.Abstractions;

/// <summary>
/// The service bus client provider.
/// </summary>
public interface IServiceBusClientProvider
{
    /// <summary>
    /// Creates a new service bus client.
    /// </summary>
    /// <param name="connectionName">The app setting name that contains the Service Bus connection string.</param>
    /// <returns>A <see cref="ServiceBusClient"/>.</returns>
    ServiceBusClient CreateClient(string connectionName);
}