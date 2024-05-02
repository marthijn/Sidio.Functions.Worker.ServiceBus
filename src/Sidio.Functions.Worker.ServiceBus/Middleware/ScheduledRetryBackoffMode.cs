namespace Sidio.Functions.Worker.ServiceBus.Middleware;

/// <summary>
/// The backoff mode for the scheduled retry middleware.
/// </summary>
public enum ScheduledRetryBackoffMode
{
    /// <summary>
    /// Exponential backoff mode.
    /// </summary>
    Exponential,

    /// <summary>
    /// Linear backoff mode.
    /// </summary>
    Linear
}