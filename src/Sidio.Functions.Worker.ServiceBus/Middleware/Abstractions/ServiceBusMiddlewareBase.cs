
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sidio.Functions.Worker.ServiceBus.Middleware.Abstractions;

/// <summary>
/// The base class for service bus middleware.
/// </summary>
public abstract class ServiceBusMiddlewareBase : IFunctionsWorkerMiddleware
{
    /// <inheritdoc />
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var serviceBusContextService = context.InstanceServices.GetService<IServiceBusContextService>() ?? new ServiceBusContextService();
        var logger = context.InstanceServices.GetService<ILogger<ServiceBusMiddlewareBase>>();
        var debugLoggerEnabled = logger != null && logger.IsEnabled(LogLevel.Debug);

        if (debugLoggerEnabled)
        {
            logger.LogDebug("Executing {MiddlewareType}", GetType().Name);
        }

        await BeforeInvocationAsync(context, serviceBusContextService, context.CancellationToken).ConfigureAwait(false);

        try
        {
            await next(context).ConfigureAwait(false);
            await AfterInvocationAsync(context, serviceBusContextService, context.CancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (debugLoggerEnabled)
            {
                logger.LogDebug(
                    ex,
                    "Exception of type {ExceptionType} occurred in {MiddlewareType}",
                    ex.GetType().Name,
                    GetType().Name);
            }

            var exceptionHandled =
                await OnExceptionAsync(context, serviceBusContextService, ex, context.CancellationToken).ConfigureAwait(false);
            if (!exceptionHandled)
            {
                throw;
            }
        }
        finally
        {
            await AlwaysAfterInvocationAsync(context, serviceBusContextService, context.CancellationToken).ConfigureAwait(false);

            if (debugLoggerEnabled)
            {
                logger.LogDebug("Executed {MiddlewareType}", GetType().Name);
            }
        }
    }

    /// <summary>
    /// Called before the function invocation.
    /// </summary>
    /// <param name="context">The function context.</param>
    /// <param name="serviceBusContextService">The service bus context service.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    protected virtual Task BeforeInvocationAsync(
        FunctionContext context,
        IServiceBusContextService serviceBusContextService,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called after the function invocation when the function invocation was completed without exceptions.
    /// </summary>
    /// <param name="context">The function context.</param>
    /// <param name="serviceBusContextService">The service bus context service.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    protected virtual Task AfterInvocationAsync(
        FunctionContext context,
        IServiceBusContextService serviceBusContextService,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Always called after the function invocation, even when an exception occurred.
    /// </summary>
    /// <param name="context">The function context.</param>
    /// <param name="serviceBusContextService">The service bus context service.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    protected virtual Task AlwaysAfterInvocationAsync(
        FunctionContext context,
        IServiceBusContextService serviceBusContextService,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the exception. Returns <c>true</c> if the exception is handled, otherwise the exception is thrown.
    /// </summary>
    /// <param name="context">The function context.</param>
    /// <param name="serviceBusContextService">The service bus context service.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="bool"/> indicating werther the exception is handled. Default is <c>false</c>.</returns>
    protected virtual Task<bool> OnExceptionAsync(
        FunctionContext context,
        IServiceBusContextService serviceBusContextService,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}