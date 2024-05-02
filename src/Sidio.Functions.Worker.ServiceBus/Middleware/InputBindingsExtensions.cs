using System.Collections.Immutable;
using Microsoft.Azure.Functions.Worker;

namespace Sidio.Functions.Worker.ServiceBus.Middleware;

internal static class InputBindingsExtensions
{
    public static BindingMetadata? GetServiceBusTriggerBinding(this IImmutableDictionary<string, BindingMetadata> bindings) =>
        bindings.FirstOrDefault(x => x.Value.Type == Constants.ServiceBusTrigger).Value;
}