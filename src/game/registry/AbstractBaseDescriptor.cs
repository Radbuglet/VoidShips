using Godot;
using VoidShips.util.polyfill;

namespace VoidShips.game.registry;

[Component]
public abstract partial class AbstractBaseDescriptor : Node
{
    public short Index { get; internal set; }

    [Export] public string? Id;
    [Export] public string? FriendlyName;
}
