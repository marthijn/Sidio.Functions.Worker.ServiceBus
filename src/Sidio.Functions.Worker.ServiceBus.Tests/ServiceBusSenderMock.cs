using Azure.Messaging.ServiceBus;

namespace Sidio.Functions.Worker.ServiceBus.Tests;

internal sealed class ServiceBusSenderMock : ServiceBusSender
{
    private readonly HashSet<ScheduledMessage> _messages = new ();

    public IReadOnlyCollection<ScheduledMessage> Messages => _messages;

    public override Task<long> ScheduleMessageAsync(
        ServiceBusMessage message,
        DateTimeOffset scheduledEnqueueTime,
        CancellationToken cancellationToken = default)
    {
        _messages.Add(new ScheduledMessage(message, scheduledEnqueueTime));
        return Task.FromResult(1L);
    }

    public override Task CloseAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public sealed record ScheduledMessage(ServiceBusMessage Message, DateTimeOffset ScheduledEnqueueTime);
}