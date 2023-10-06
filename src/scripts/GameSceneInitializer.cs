using Godot;
using VoidShips.Actors.Inventory;
using VoidShips.Util;

namespace VoidShips.scripts;

[Component]
public partial class GameSceneInitializer : Node
{
    [Export] public Node? LocalPlayer;
    
    public override void _Ready()
    {
        this.Component<ItemRegistry>().RegisterChildren();
        
        LocalPlayer!.Component<InventoryData>().AddStack(this.Component<ItemRegistry>().BuildDefault("stone"));
    }
}