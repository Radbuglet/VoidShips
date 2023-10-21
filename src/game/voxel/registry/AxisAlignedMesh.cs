using System.Collections.Generic;
using System.Linq;
using Godot;
using VoidShips.game.voxel.math;
using VoidShips.Util;

namespace VoidShips.game.voxel.registry;

public sealed class AxisAlignedMesh
{
    private readonly List<(Aabb, Node?)> _volumes = new();
    
    private readonly List<(AaQuad3, Node?)>[] _quads = ArrayUtil.InitArray(
        BlockFaceExt.VariantCount,
        () => new List<(AaQuad3, Node?)>());

    public void AddAabb(Aabb aabb, Node? node)
    {
        _volumes.Add((aabb, node));
    }

    public void AddQuad(AaQuad3 plane, Node? data)
    {
        _quads[(int) plane.Normal].Add((plane, data));
    }

    public IEnumerable<(Aabb, Node?)> Volumes()
    {
        return _volumes;
    }

    public IEnumerable<(AaQuad3, Node?)> VolumePlanesFacing(BlockFace face)
    {
        return _volumes.Select(v => (v.Item1.FaceQuad(face), v.Item2));
    }

    public IEnumerable<(AaQuad3, Node?)> QuadPlanesFacing(BlockFace face)
    {
        return _quads[(int) face];
    }

    public IEnumerable<(AaQuad3, Node?)> PlanesFacing(BlockFace face)
    {
        return VolumePlanesFacing(face).Concat(QuadPlanesFacing(face));
    }
}
