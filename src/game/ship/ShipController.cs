using System.Collections.Generic;
using System.Linq;
using Godot;
using VoidShips.game.voxel;
using VoidShips.game.voxel.math;
using VoidShips.Util;

namespace VoidShips.game.ship;

[Component]
public sealed partial class ShipController : Node
{
    private readonly Dictionary<Vector3I, Node> _parts = new();

    public void AddPart(Node part)
    {
        var partData = part.Component<ShipPart>();
        var pos = partData.Position;
        
        AddChild(part);
        _parts.Add(pos, partData);

        partData.EmitSignal(nameof(ShipPart.SignalName.Attached), pos);
        
        foreach (var rel in VoxelMath.FacesVectorsI)
            GetPart(pos + rel)?.Component<ShipPart>().EmitSignal(nameof(ShipPart.SignalName.NeighborModified));
    }

    public Node? GetPart(Vector3I pos)
    {
        return _parts.TryGetValue(pos, out var part) ? part : null;
    }

    public IEnumerable<(Vector3I, Node)> IterParts()
    {
        return _parts.Select(v => (v.Key, v.Value));
    }
}