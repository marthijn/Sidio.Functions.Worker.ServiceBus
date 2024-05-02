using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;

namespace Sidio.Functions.Worker.ServiceBus.Examples;

public sealed class ExceptionHandlingFunction
{
    [Function(nameof(ExceptionHandlingFunction))]
    public Task RunAsync(
        [ServiceBusTrigger("examplequeue", Connection = "ServiceBus-Example")] ServiceBusReceivedMessage message,
        FunctionContext context,
        CancellationToken cancellationToken = default)
    {
        throw new Exception("An error occurred while processing the message.");
    }
}