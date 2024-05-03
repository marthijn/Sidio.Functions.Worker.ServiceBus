using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sidio.Functions.Worker.ServiceBus.Middleware.Abstractions;

namespace Sidio.Functions.Worker.ServiceBus.Middleware;

internal sealed class DefaultServiceBusClientProvider : IServiceBusClientProvider
{
    private readonly FunctionContext _functionContext;

    public DefaultServiceBusClientProvider(FunctionContext functionContext)
    {
        _functionContext = functionContext;
    }

    public ServiceBusClient CreateClient(string connectionName)
    {
        var configuration = _functionContext.InstanceServices.GetRequiredService<IConfiguration>();

        var fullyQualifiedNamespace = configuration[$"{connectionName}__fullyQualifiedNamespace"];
        if (!string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
        {
            return new ServiceBusClient(fullyQualifiedNamespace, new DefaultAzureCredential());
        }

        var connectionString = configuration.GetConnectionString(connectionName);
        return new ServiceBusClient(connectionString);
    }
}