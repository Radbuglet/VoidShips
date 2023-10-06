using Godot;
using VoidShips.Util;

namespace VoidShips.Actors.Inventory.descriptor;

[Component]
public sealed partial class ItemDescriptorVisual : Node
{
	[Export] public Color Color = Colors.Magenta;
}
