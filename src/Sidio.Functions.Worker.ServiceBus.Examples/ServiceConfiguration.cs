using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sidio.Functions.Worker.ServiceBus.Middleware;

namespace Sidio.Functions.Worker.ServiceBus.Examples;

internal static class ServiceConfiguration
{
    public static Action<HostBuilderContext, IServiceCollection> Configuration =>
        (context, services) =>
        {
            services.AddServiceBusClientForScheduledRetryMiddleware(context.Configuration["ConnectionStrings:ServiceBus-Example"]);
        };
}