using System.Collections.Immutable;
using Microsoft.Azure.Functions.Worker;
using Sidio.Functions.Worker.ServiceBus.Middleware;

namespace Sidio.Functions.Worker.ServiceBus.Tests.Middleware;

public sealed class InputBindingsExtensionsTests
{
    [Fact]
    public void GetServiceBusTrigger_WhenTriggerExists_ReturnsTrigger()
    {
        // arrange
        var bindings = new Dictionary<string, BindingMetadata>
        {
            { "serviceBusTrigger", new TestBindingData() }
        };

        // act
        var result = bindings.ToImmutableDictionary().GetServiceBusTriggerBinding();

        // assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetServiceBusTrigger_WhenTriggerDoesNotExist_ReturnsNull()
    {
        // arrange
        var bindings = new Dictionary<string, BindingMetadata>
        {
            { "serviceBusTrigger", new TestBindingData("testTrigger") }
        };

        // act
        var result = bindings.ToImmutableDictionary().GetServiceBusTriggerBinding();

        // assert
        result.Should().BeNull();
    }

    private sealed class TestBindingData : BindingMetadata
    {
        public TestBindingData(string type = "serviceBusTrigger")
        {
            Type = type;
        }

        public override string Name => "Test";

        public override string Type { get; }

        public override BindingDirection Direction => BindingDirection.In;
    }
}