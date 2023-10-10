using Godot;
using VoidShips.Util;

namespace VoidShips.actors.ship;

[Component]
public sealed partial class ShipPartClient : Node
{
	private void ShipAttached(Vector3I pos)
	{
		this.GameObject<Node3D>().Position = pos * 10;
	}
}
