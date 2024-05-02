using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus;

namespace Sidio.Functions.Worker.ServiceBus.Middleware;

/// <summary>
/// The options for the <see cref="ExceptionInsightMiddleware"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public record ExceptionInsightMiddlewareOptions
{
    /// <summary>
    /// Gets service bus maximum delivery count.
    /// Should be less or equivalent to the queue's maximum delivery count. Currently, there is no way
    /// to tell the max delivery count of a queue or topic by the <see cref="ServiceBusReceivedMessage"/>.
    /// </summary>
    public int MaxDeliveryCount { get; init; } = 10;
}