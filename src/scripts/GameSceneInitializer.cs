using Godot;
using VoidShips.game.inventory;
using VoidShips.game.voxel.registry;
using VoidShips.Util;

namespace VoidShips.scripts;

[Component]
public partial class GameSceneInitializer : Node
{
	[Export] public Node? LocalPlayer;
	
	public override void _Ready()
	{
		this.Component<ItemRegistry>().RegisterChildren();
		this.Component<BlockRegistry>().RegisterChildren();

		LocalPlayer!.Component<InventoryData>().AddStack(this.Component<ItemRegistry>().BuildDefault("stone"));
	}
}
