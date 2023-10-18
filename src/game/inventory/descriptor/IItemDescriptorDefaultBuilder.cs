using Godot;
using VoidShips.Util;

namespace VoidShips.game.inventory.descriptor;

[Component]
public interface IItemDescriptorDefaultBuilder
{
    void BuildDefault(Node target);
}