using Godot;
using VoidShips.Util;

namespace VoidShips.game.ship;

[Component]
public sealed partial class ShipPart : Node
{
	[Signal]
	public delegate void AttachedEventHandler(Vector3I pos);

	[Signal]
	public delegate void NeighborModifiedEventHandler();

	public Vector3I Position;
}
