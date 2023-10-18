using Godot;
using VoidShips.Util;

namespace VoidShips.game.inventory;

[Component]
public sealed partial class ItemStackBase : Node
{
    public Node? Material;
    public int Count;
}
