using Microsoft.Extensions.Hosting;
using Sidio.Functions.Worker.ServiceBus.Examples;
using Sidio.Functions.Worker.ServiceBus.Middleware;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(
        workerApplication =>
        {
            // use one or the other:
            // workerApplication.UseExceptionInsightMiddleware();
            workerApplication.UseScheduledRetryMiddleware();
        })
    .ConfigureServices(ServiceConfiguration.Configuration)
    .Build();

host.Run();