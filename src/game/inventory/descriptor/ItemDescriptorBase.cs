using Godot;
using VoidShips.Util;

namespace VoidShips.game.inventory.descriptor;

[Component]
public sealed partial class ItemDescriptorBase : Node
{
    [Export] public string? Id;
    [Export] public string? FriendlyName;
    [Export] public int MaxAutomaticStacking;

    public Node BuildDefault()
    {
        var stack = new Node().MakeGameObject();
        stack.Name = Id;
        stack.AddChild(new ItemStackBase { Material = this.GameObject<Node>(), Count = 1 });
        this.TryComponent<IItemDescriptorDefaultBuilder>()?.BuildDefault(stack);
        
        return stack;
    }
}
