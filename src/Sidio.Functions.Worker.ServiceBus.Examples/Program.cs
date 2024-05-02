using Microsoft.Extensions.Hosting;
using Sidio.Functions.Worker.ServiceBus.Examples;
using Sidio.Functions.Worker.ServiceBus.Middleware;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(
        workerApplication =>
        {
            workerApplication.UseExceptionInsightMiddleware();
        })
    .ConfigureServices(ServiceConfiguration.Configuration)
    .Build();

host.Run();