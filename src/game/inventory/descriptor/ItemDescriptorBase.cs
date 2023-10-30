using Godot;
using VoidShips.game.registry;
using VoidShips.util.polyfill;

namespace VoidShips.game.inventory.descriptor;

[Component]
public sealed partial class ItemDescriptorBase : AbstractBaseDescriptor
{
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
