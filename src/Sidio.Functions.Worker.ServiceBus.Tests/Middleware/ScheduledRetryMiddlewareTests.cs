using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sidio.Functions.Worker.ServiceBus.Middleware;
using Sidio.Functions.Worker.ServiceBus.Middleware.Abstractions;

namespace Sidio.Functions.Worker.ServiceBus.Tests.Middleware;

public sealed class ScheduledRetryMiddlewareTests
{
    private readonly Fixture _fixture = new ();

    [Fact]
    public async Task Invoke_ExceptionThrownAfterFirstDeliveryAttempt_ScheduleMessage()
    {
        // arrange
        var connectionName = _fixture.Create<string>();
        var currentUtc = DateTimeOffset.UtcNow;
        var serviceCollection = CreateServiceCollection(connectionName, out var serviceBusContextService, out var serviceBusSender);
        var middleware = CreateMiddleware();
        var context = new Mock<FunctionContext>();
        var functionDefinition = CreateFunctionDefinition(connectionName);
        context.SetupGet(x => x.InstanceServices).Returns(serviceCollection.BuildServiceProvider());
        context.SetupGet(x => x.FunctionDefinition).Returns(functionDefinition.Object);

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
        serviceBusSender.Messages.Should().HaveCount(1);
        actions.IsDeadLettered.Should().BeFalse();

        var resultMessage = serviceBusSender.Messages.First();
        resultMessage.ScheduledEnqueueTime.Should().BeAfter(currentUtc);
        resultMessage.Message.ApplicationProperties.Should().ContainKey(ScheduledRetryMiddleware.DeliveryAttempts).And.ContainValue(2);
    }

    [Fact]
    public async Task Invoke_MaxDeliveryAttempts_DeadLetterMessage()
    {
        // arrange
        var connectionName = _fixture.Create<string>();
        var serviceCollection = CreateServiceCollection(connectionName, out var serviceBusContextService, out var serviceBusSender);
        var middleware = CreateMiddleware();
        var context = new Mock<FunctionContext>();
        context.SetupGet(x => x.InstanceServices).Returns(serviceCollection.BuildServiceProvider());

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(properties: new Dictionary<string, object>
        {
            { ScheduledRetryMiddleware.DeliveryAttempts, 10 }
        });
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
        serviceBusSender.Messages.Should().HaveCount(0);
        actions.IsDeadLettered.Should().BeTrue();
    }

    private static ScheduledRetryMiddleware CreateMiddleware(
        int maxDeliveryCount = 10,
        ScheduledRetryBackoffMode backoffMode = ScheduledRetryBackoffMode.Exponential,
        int backoffInSeconds = 10)
    {
        var options = Options.Create(
            new ScheduledRetryMiddlewareOptions
            {
                MaxDeliveryCount = maxDeliveryCount,
                BackoffMode = backoffMode,
                BackoffInSeconds = backoffInSeconds,
            });
        var logger = new NullLogger<ScheduledRetryMiddleware>();
        return new ScheduledRetryMiddleware(options, logger);
    }

    private static ServiceCollection CreateServiceCollection(string connectionName, out Mock<IServiceBusContextService> serviceBusContextService, out ServiceBusSenderMock serviceBusSender)
    {
        serviceBusContextService = new Mock<IServiceBusContextService>();
        var serviceBusContextServiceObject = serviceBusContextService.Object;

        serviceBusSender = new ServiceBusSenderMock();

        var serviceBusClientMock = new Mock<ServiceBusClient>();
        serviceBusClientMock.Setup(x => x.CreateSender(It.IsAny<string>()))
            .Returns(serviceBusSender);

        var serviceBusClientProvider = new Mock<IServiceBusClientProvider>();
        serviceBusClientProvider.Setup(x => x.CreateClient(connectionName))
            .Returns(serviceBusClientMock.Object);

        var memorySource = new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string>>
            {
                new ("ConnectionStrings:" + connectionName, "test")
            }
        };

        var configuration = new ConfigurationRoot(
            new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(memorySource)
            });

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IServiceBusContextService>(_ => serviceBusContextServiceObject);
        serviceCollection.AddScoped<IServiceBusClientProvider>(_ => serviceBusClientProvider.Object);
        serviceCollection.AddSingleton<IConfiguration>(configuration);
        return serviceCollection;
    }

    private static Mock<FunctionDefinition> CreateFunctionDefinition(string connectionName)
    {
        var functionDefinition = new Mock<FunctionDefinition>();
        var functionParameter = new FunctionParameter(
            "test",
            typeof(ServiceBusReceivedMessage),
            new Dictionary<string, object>
            {
                {
                    "bindingAttribute",
                    new ServiceBusTriggerAttribute("test")
                    {
                        Connection = connectionName
                    }
                }
            });
        functionDefinition.SetupGet(x => x.Parameters).Returns(
        [
            ..new List<FunctionParameter>
            {
                functionParameter
            }
        ]);

        return functionDefinition;

    }
}