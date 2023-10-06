using Godot;
using VoidShips.Util;

namespace VoidShips.Actors.Inventory;

[Component]
public sealed partial class ItemStackBase : Node
{
    public Node? Material;
    public int Count;
}
