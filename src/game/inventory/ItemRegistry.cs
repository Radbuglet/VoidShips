using Godot;
using VoidShips.game.inventory.descriptor;
using VoidShips.game.registry;
using VoidShips.Util;

namespace VoidShips.game.inventory;

[Component]
public sealed partial class ItemRegistry : AbstractRegistry
{
	public Node BuildDefault(string id)
	{
		return Lookup(id)!.Component<ItemDescriptorBase>().BuildDefault();
	}
}
