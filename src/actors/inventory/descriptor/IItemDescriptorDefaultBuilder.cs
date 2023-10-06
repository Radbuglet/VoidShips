using Godot;
using VoidShips.Util;

namespace VoidShips.Actors.Inventory.descriptor;

[Component]
public interface IItemDescriptorDefaultBuilder
{
    void BuildDefault(Node target);
}