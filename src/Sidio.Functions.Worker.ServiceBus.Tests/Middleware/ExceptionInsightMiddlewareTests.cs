using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sidio.Functions.Worker.ServiceBus.Middleware;
using Sidio.Functions.Worker.ServiceBus.Middleware.Abstractions;

namespace Sidio.Functions.Worker.ServiceBus.Tests.Middleware;

public sealed class ExceptionInsightMiddlewareTests
{
    private readonly Fixture _fixture = new ();

    [Fact]
    public async Task Invoke_NoExceptionThrown_ShouldNotThrow()
    {
        // arrange
        var serviceCollection = CreateServiceCollection(out _);
        var middleware = CreateMiddleware();
        var context = new Mock<FunctionContext>();
        context.SetupGet(x => x.InstanceServices).Returns(serviceCollection.BuildServiceProvider());

        // act
        var action = () => middleware.Invoke(context.Object, _ => Task.CompletedTask);

        // assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Invoke_ExceptionThrownWithMaxDeliveryCount_ExceptionHandled()
    {
        // arrange
        var serviceCollection = CreateServiceCollection(out var serviceBusContextService);
        var middleware = CreateMiddleware(0);
        var context = new Mock<FunctionContext>();
        context.SetupGet(x => x.InstanceServices).Returns(serviceCollection.BuildServiceProvider());

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage();
        serviceBusContextService.Setup(
                x => x.GetServiceBusReceivedMessageAsync(It.IsAny<FunctionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        var actions = new ServiceBusMessageActionsMock();
        serviceBusContextService.Setup(x => x.GetServiceBusMessageActionsAsync(It.IsAny<FunctionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(actions);

        // act
        var action = () => middleware.Invoke(context.Object,
            _ => throw new Exception("test exception"));

        // assert
        await action.Should().NotThrowAsync();
        actions.IsDeadLettered.Should().BeTrue();
    }

    [Fact]
    public async Task Invoke_ExceptionThrown_ThrowException()
    {
        // arrange
        var exceptionMessage = _fixture.Create<string>();
        var serviceCollection = CreateServiceCollection(out var serviceBusContextService);
        var middleware = CreateMiddleware(1);
        var context = new Mock<FunctionContext>();
        context.SetupGet(x => x.InstanceServices).Returns(serviceCollection.BuildServiceProvider());

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage();
        serviceBusContextService.Setup(
                x => x.GetServiceBusReceivedMessageAsync(It.IsAny<FunctionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        var actions = new ServiceBusMessageActionsMock();
        serviceBusContextService.Setup(x => x.GetServiceBusMessageActionsAsync(It.IsAny<FunctionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(actions);

        // act
        var action = () => middleware.Invoke(context.Object,
            _ => throw new Exception(exceptionMessage));

        // assert
        await action.Should().ThrowAsync<Exception>().WithMessage(exceptionMessage);
    }

    private static ExceptionInsightMiddleware CreateMiddleware(int maxDeliveryCount = 10)
    {
        var options = Options.Create(new ExceptionInsightMiddlewareOptions
        {
            MaxDeliveryCount = maxDeliveryCount
        });
        var logger = new NullLogger<ExceptionInsightMiddleware>();
        return new ExceptionInsightMiddleware(options, logger);
    }

    private static ServiceCollection CreateServiceCollection(out Mock<IServiceBusContextService> serviceBusContextService)
    {
        serviceBusContextService = new Mock<IServiceBusContextService>();
        var serviceBusContextServiceObject = serviceBusContextService.Object;

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IServiceBusContextService>(_ => serviceBusContextServiceObject);
        return serviceCollection;
    }
}