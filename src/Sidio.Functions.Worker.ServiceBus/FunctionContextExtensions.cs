using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;

namespace Sidio.Functions.Worker.ServiceBus;

/// <summary>
/// The extension methods for the <see cref="FunctionContext"/> class.
/// </summary>
public static class FunctionContextExtensions
{
    private const string TriggerBindingSuffix = "Trigger";

    private const string BindingAttribute = "bindingAttribute";

    /// <summary>
    /// Returns <c>true</c> when the trigger is a service bus trigger.
    /// </summary>
    /// <param name="context">The function context.</param>
    /// <returns>A <see cref="bool"/>.</returns>
    public static bool IsServiceBusTrigger(this FunctionContext context)
    {
        var binding = context.FunctionDefinition.InputBindings.Values
            .FirstOrDefault(a => a.Type.EndsWith(TriggerBindingSuffix));
        return binding is {Type: Constants.ServiceBusTrigger};
    }

    /// <summary>
    /// Returns the <see cref="ServiceBusTriggerAttribute"/> when the trigger is a service bus trigger.
    /// </summary>
    /// <param name="context">The function context.</param>
    /// <returns>The <see cref="ServiceBusTriggerAttribute"/>.</returns>
    public static ServiceBusTriggerAttribute? GetServiceBusTrigger(this FunctionContext context)
    {
        var receivedMessage = context.GetServiceBusReceivedMessageFunctionParameter();
        var bindingAttribute = receivedMessage?.GetBindingAttribute();
        return bindingAttribute?.Value as ServiceBusTriggerAttribute;
    }

    private static FunctionParameter? GetServiceBusReceivedMessageFunctionParameter(this FunctionContext context) =>
        context.FunctionDefinition.Parameters.FirstOrDefault(
            x => x.Type == typeof(ServiceBusReceivedMessage));

    private static KeyValuePair<string, object>? GetBindingAttribute(this FunctionParameter functionParameter) =>
        functionParameter.Properties.FirstOrDefault(
            x => x.Key.Equals(BindingAttribute, StringComparison.OrdinalIgnoreCase));
}