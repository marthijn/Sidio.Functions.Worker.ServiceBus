using System.Diagnostics.CodeAnalysis;

namespace Sidio.Functions.Worker.ServiceBus.Middleware;

/// <summary>
/// The options for the scheduled retry middleware.
/// </summary>
[ExcludeFromCodeCoverage]
public record ScheduledRetryMiddlewareOptions : ExceptionInsightMiddlewareOptions
{
    /// <summary>
    /// Gets the backoff mode.
    /// </summary>
    public ScheduledRetryBackoffMode BackoffMode { get; init; } = ScheduledRetryBackoffMode.Exponential;

    /// <summary>
    /// Gets the backoff in seconds.
    /// </summary>
    public int BackoffInSeconds { get; init; } = 10;
}