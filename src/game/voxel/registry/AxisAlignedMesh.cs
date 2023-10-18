using System.Collections.Generic;
using Godot;
using VoidShips.game.voxel.math;
using VoidShips.Util;

namespace VoidShips.game.voxel.registry;

public sealed partial class AxisAlignedMesh : Node
{
    private readonly List<(AaPlane3, Node)>[] _planes = ArrayUtil.InitArray(BlockFaceExt.VariantCount, () => new List<(AaPlane3, Node)>());
}
