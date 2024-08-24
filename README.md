Sidio.Functions.Worker.ServiceBus
=============
A collection of useful service bus features for .NET isolated middleware.

[![build](https://github.com/marthijn/Sidio.Functions.Worker.ServiceBus/actions/workflows/build.yml/badge.svg)](https://github.com/marthijn/Sidio.Functions.Worker.ServiceBus/actions/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/v/Sidio.Functions.Worker.ServiceBus)](https://www.nuget.org/packages/Sidio.Functions.Worker.ServiceBus/)
[![Coverage Status](https://coveralls.io/repos/github/marthijn/Sidio.Functions.Worker.ServiceBus/badge.svg?branch=main)](https://coveralls.io/github/marthijn/Sidio.Functions.Worker.ServiceBus?branch=main)

# Installation
Add the NuGet package to your project.

# Middleware
## Middleware template
The abstract class `ServiceBusMiddlewareBase` supports the following methods:
- `BeforeInvocationAsync`: executed before the invocation of the `FunctionExecutionDelegate`.
- `AfterInvocationAsync`: executed after the invocation of the `FunctionExecutionDelegate`, unless an exception is thrown.
- `AlwaysAfterInvocation`: executed after the invocation of the `FunctionExecutionDelegate`, regardless of an exception being thrown.
- `OnExceptionAsync`: executed when an exception is thrown during the invocation of the `FunctionExecutionDelegate`. Return `true` when the exception is handled, `false` when the exception should be rethrown.

## ExceptionInsightMiddleware
The `ExceptionInsightMiddleware` adds exception details to a dead-lettered message:
- Dead letter reason
- Exception message

Please note: it is currently not possible to read the maximum delivery count setting of a queue or topic. Therefore, you have to configure the `ExceptionInsightMiddlewareOptions` manually
and ensure the maximum delivery count is set to a value equal or less than the service bus setting.

### Usage
```csharp
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(
        workerApplication =>
        {
            workerApplication.UseExceptionInsightMiddleware();
        })
    // ...
```

## ScheduledRetryMiddleware
### Usage
The `ScheduledRetryMiddleware` is an extension of the `ExceptionInsightMiddleware`, you should not register both middleware.
```csharp
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(
        workerApplication =>
        {
            workerApplication.UseScheduledRetryMiddleware();
        })
    // ...
```

### Configuration
- MaxDeliveryCount: the maximum number of delivery attempts before a message is dead-lettered.
- BackoffMode: the backoff mode to use for rescheduling messages.
- BackoffInSeconds: the number of seconds to wait before rescheduling a message.

### Service Bus client provider
It's possible to use a custom `IServiceBusClientProvider` to provide a `ServiceBusClient` for sending messages:
```csharp
services.AddScoped<IServiceBusClientProvider, MyServiceBusClientProvider>();
```

### Manual dead-letter resubmission
When you want to resubmit a dead-lettered message manually, make sure the `ScheduledRetryMiddleware.DeliveryAttempts` property is removed.

# Extensions
## FunctionContext extensions
### GetServiceBusTrigger
Gets the `ServiceBusTriggerAttribute` from the function context:
```csharp
var attribute = functionContext.GetServiceBusTrigger();
```

### IsServiceBusTrigger
Determines whether the function is a service bus trigger. Can be used when registering middleware:
```csharp
functionsWorkerApplicationBuilder.UseWhen<MySerivceBusMiddleware>(context => context.IsServiceBusTrigger());
```

# Known issues
- The `ExceptionInsightMiddleware` and `ScheduledRetryMiddleware` only support a trigger with a `ServiceBusRecievedMessage` binding. A trigger-binding to a `string` is not supported.