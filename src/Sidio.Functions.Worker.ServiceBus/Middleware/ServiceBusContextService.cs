using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Sidio.Functions.Worker.ServiceBus.Middleware.Abstractions;

namespace Sidio.Functions.Worker.ServiceBus.Middleware;

internal sealed class ServiceBusContextService : IServiceBusContextService
{
    public async Task<ServiceBusMessageActions?> GetServiceBusMessageActionsAsync(
        FunctionContext functionContext,
        CancellationToken cancellationToken = default)
    {
        var actions = await functionContext
            .BindInputAsync<ServiceBusMessageActions>(new MessageActionsBindingMetadata()).ConfigureAwait(false);
        return actions.Value;
    }

    public async Task<ServiceBusReceivedMessage?> GetServiceBusReceivedMessageAsync(
        FunctionContext functionContext,
        CancellationToken cancellationToken = default)
    {
        var serviceBusTriggerBinding = functionContext.FunctionDefinition.InputBindings.GetServiceBusTriggerBinding();
        if (serviceBusTriggerBinding == null)
        {
            return null;
        }

        var result = await functionContext.BindInputAsync<ServiceBusReceivedMessage>(serviceBusTriggerBinding)
            .ConfigureAwait(false);
        return result.Value;
    }

    private sealed class MessageActionsBindingMetadata : BindingMetadata
    {
        public override string Name => "messageActions";

        public override string Type => nameof(ServiceBusMessageActions);

        public override BindingDirection Direction => BindingDirection.In;
    }
}