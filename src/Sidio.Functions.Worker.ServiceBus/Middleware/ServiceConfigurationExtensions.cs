using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Sidio.Functions.Worker.ServiceBus.Middleware.Abstractions;

namespace Sidio.Functions.Worker.ServiceBus.Middleware;

/// <summary>
/// The extension methods for the <see cref="IServiceCollection"/> interface.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ServiceConfigurationExtensions
{
    /// <summary>
    /// Adds the service bus context service. Registering a <see cref="IServiceBusContextService"/> is optional.
    /// When no implementation is found, the default will be used.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddServiceBusContextService<T>(this IServiceCollection services)
        where T : class, IServiceBusContextService
    {
        services.AddScoped<IServiceBusContextService, T>();
        return services;
    }

    /// <summary>
    /// Adds the service bus client for the <see cref="ScheduledRetryMiddleware"/>.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="connectionString">The service bus connection string.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddServiceBusClientForScheduledRetryMiddleware(this IServiceCollection services, string connectionString)
    {
        services.AddAzureClients(
            clientsBuilder =>
            {
                clientsBuilder.AddServiceBusClient(connectionString)
                    .WithName(ScheduledRetryMiddleware.ServiceBusClientName);
            });
        return services;
    }
}