using Microsoft.Azure.Functions.Worker;

namespace Sidio.Functions.Worker.ServiceBus.Tests;

internal sealed class BindingMetadataMock : BindingMetadata
{
    public BindingMetadataMock(string name, string type, BindingDirection direction)
    {
        Name = name;
        Type = type;
        Direction = direction;
    }

    public override string Name { get; }

    public override string Type { get; }

    public override BindingDirection Direction { get; }
}