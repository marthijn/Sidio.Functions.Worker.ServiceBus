﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Sidio.Functions.Worker.ServiceBus.Examples;

internal static class ServiceConfiguration
{
    public static Action<HostBuilderContext, IServiceCollection> Configuration =>
        (context, services) =>
        {
        };
}