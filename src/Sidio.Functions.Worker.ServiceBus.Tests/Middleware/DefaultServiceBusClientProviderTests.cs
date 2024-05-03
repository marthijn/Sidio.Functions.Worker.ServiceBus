using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Sidio.Functions.Worker.ServiceBus.Middleware;

namespace Sidio.Functions.Worker.ServiceBus.Tests.Middleware;

public sealed class DefaultServiceBusClientProviderTests
{
    private readonly Fixture _fixture = new ();

    [Fact]
    public void CreateClient_WithConnectionString_ReturnsClient()
    {
        // arrange
        var connectionName = _fixture.Create<string>();
        var memorySource = new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string>>
            {
                new ("ConnectionStrings:" + connectionName, "Endpoint=sb://foo.servicebus.windows.net/;SharedAccessKeyName=someKeyName;SharedAccessKey=someKeyValue")
            }
        };

        var configuration = new ConfigurationRoot(
            new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(memorySource)
            });

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration);

        var context = new Mock<FunctionContext>();
        context.SetupGet(x => x.InstanceServices).Returns(serviceCollection.BuildServiceProvider());

        var clientProvider = new DefaultServiceBusClientProvider(context.Object);

        // act
        var result = clientProvider.CreateClient(connectionName);

        // assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateClient_WithFqn_ReturnsClient()
    {
        // arrange
        var connectionName = _fixture.Create<string>();
        var memorySource = new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string>>
            {
                new ($"{connectionName}__fullyQualifiedNamespace", "sb://foo.servicebus.windows.net/")
            }
        };

        var configuration = new ConfigurationRoot(
            new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(memorySource)
            });

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration);

        var context = new Mock<FunctionContext>();
        context.SetupGet(x => x.InstanceServices).Returns(serviceCollection.BuildServiceProvider());

        var clientProvider = new DefaultServiceBusClientProvider(context.Object);

        // act
        var result = clientProvider.CreateClient(connectionName);

        // assert
        result.Should().NotBeNull();
    }
}