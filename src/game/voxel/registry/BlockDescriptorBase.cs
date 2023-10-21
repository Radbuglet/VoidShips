using Godot;
using VoidShips.game.registry;

namespace VoidShips.game.voxel.registry;

public sealed partial class BlockDescriptorBase : AbstractBaseDescriptor
{
    public readonly AxisAlignedMesh CollisionMesh = new();

    public override void _Ready()
    {
        CollisionMesh.AddAabb(new Aabb(Vector3.Zero, Vector3.One), null);
    }
}
