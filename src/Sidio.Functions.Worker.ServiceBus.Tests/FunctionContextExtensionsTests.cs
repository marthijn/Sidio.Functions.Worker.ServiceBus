using System.Collections.Immutable;
using Microsoft.Azure.Functions.Worker;

namespace Sidio.Functions.Worker.ServiceBus.Tests;

public sealed class FunctionContextExtensionsTests
{
    [Fact]
    public void IsServiceBusTrigger_WhenServiceBusTrigger_ReturnsTrue()
    {
        // arrange
        var trigger = new BindingMetadataMock("test", "serviceBusTrigger", BindingDirection.In);
        var inputBindings = new Dictionary<string, BindingMetadata>
        {
            {"ServiceBusTrigger", trigger}
        };
        var functionDefinition = CreateFunctionDefinitionMock(inputBindings).Object;
        var context = CreateFunctionContextMock(functionDefinition).Object;

        // act
        var result = context.IsServiceBusTrigger();

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsServiceBusTrigger_WhenNotServiceBusTrigger_ReturnsFalse()
    {
        // arrange
        var trigger = new BindingMetadataMock("test", "Test", BindingDirection.In);
        var inputBindings = new Dictionary<string, BindingMetadata>
        {
            {"ServiceBusTrigger", trigger}
        };
        var functionDefinition = CreateFunctionDefinitionMock(inputBindings).Object;
        var context = CreateFunctionContextMock(functionDefinition).Object;

        // act
        var result = context.IsServiceBusTrigger();

        // assert
        result.Should().BeFalse();
    }

    private static Mock<FunctionDefinition> CreateFunctionDefinitionMock(
        Dictionary<string, BindingMetadata> inputBindings)
    {
        var mock = new Mock<FunctionDefinition>();
        mock.SetupGet(x => x.InputBindings).Returns(inputBindings.ToImmutableDictionary());
        return mock;
    }

    private static Mock<FunctionContext> CreateFunctionContextMock(FunctionDefinition functionDefinition)
    {
        var mock = new Mock<FunctionContext>();
        mock.SetupGet(x => x.FunctionDefinition).Returns(functionDefinition);
        return mock;
    }
}