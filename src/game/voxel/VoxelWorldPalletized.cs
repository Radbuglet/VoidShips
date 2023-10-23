using Godot;
using VoidShips.game.voxel.registry;
using VoidShips.Util;

namespace VoidShips.game.voxel;

[Component]
public sealed partial class VoxelWorldPalletized : Node
{
    private VoxelDataWorld? _world;
    private BlockRegistry? _registry;

    public VoxelDataWorld World => _world!;
    public BlockRegistry Registry => _registry!;

    public override void _Ready()
    {
        _world = this.Component<VoxelDataWorld>();
        _registry = this.Component<BlockRegistry>();
    }

    public BlockDescriptorBase GetBlock(VoxelPointer pointer)
    {
        return Registry.Lookup(pointer.GetData()).Component<BlockDescriptorBase>();
    }
}
