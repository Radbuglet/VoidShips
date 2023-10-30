using Godot;
using VoidShips.util;

namespace VoidShips.game.inventory.descriptor;

[Component]
public interface IItemDescriptorDefaultBuilder
{
    void BuildDefault(Node target);
}