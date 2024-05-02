using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;

namespace Sidio.Functions.Worker.ServiceBus.Tests;

internal sealed class ServiceBusMessageActionsMock : ServiceBusMessageActions
{
    public bool IsDeadLettered { get; private set; }

    public override Task DeadLetterMessageAsync(
        ServiceBusReceivedMessage message,
        Dictionary<string, object>? propertiesToModify = null,
        string? deadLetterReason = null,
        string? deadLetterErrorDescription = null,
        CancellationToken cancellationToken = default)
    {
        IsDeadLettered = true;
        return Task.CompletedTask;
    }
}