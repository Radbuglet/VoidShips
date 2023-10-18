using Godot;
using VoidShips.Util;

namespace VoidShips.game.inventory.descriptor;

[Component]
public sealed partial class ItemDescriptorVisual : Node
{
	[Export] public Color Color = Colors.Magenta;
}
