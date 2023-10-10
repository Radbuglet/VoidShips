using Godot;
using VoidShips.Actors.Inventory;
using VoidShips.actors.ship;
using VoidShips.Util;

namespace VoidShips.scripts;

[Component]
public partial class GameSceneInitializer : Node
{
    [Export] public Node? LocalPlayer;
    
    public override void _Ready()
    {
        this.Component<ItemRegistry>().RegisterChildren();

        var shipScene = GD.Load<PackedScene>("res://res/actors/basic_ship_part.tscn");
        {
            var shipPart = shipScene.Instantiate<Node>();
            this.Component<ShipController>().AddPart(shipPart);
            
            shipPart = shipScene.Instantiate<Node>();
            shipPart.Component<ShipPart>().Position = new Vector3I(1, 0, 0);
            this.Component<ShipController>().AddPart(shipPart);
        }

        LocalPlayer!.Component<InventoryData>().AddStack(this.Component<ItemRegistry>().BuildDefault("stone"));
    }
}