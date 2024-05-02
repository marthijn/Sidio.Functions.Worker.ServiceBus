using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;

namespace Sidio.Functions.Worker.ServiceBus.Middleware;

/// <summary>
/// The extension methods for the <see cref="IFunctionsWorkerApplicationBuilder"/> interface.
/// </summary>
[ExcludeFromCodeCoverage]
public static class FunctionsWorkerApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the <see cref="ExceptionInsightMiddleware"/>.
    /// </summary>
    /// <param name="builder">The function worker application builder.</param>
    /// <returns>The <see cref="IFunctionsWorkerApplicationBuilder"/>.</returns>
    public static IFunctionsWorkerApplicationBuilder UseExceptionInsightMiddleware(this IFunctionsWorkerApplicationBuilder builder)
    {
        builder.UseWhen<ExceptionInsightMiddleware>(context => context.IsServiceBusTrigger());
        return builder;
    }
}