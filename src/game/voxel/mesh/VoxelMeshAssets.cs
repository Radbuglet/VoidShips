using Godot;
using VoidShips.game.registry;
using VoidShips.game.voxel.registry;
using VoidShips.util.polyfill;

namespace VoidShips.game.voxel.mesh;

[Component]
public sealed partial class VoxelMeshAssets : Node
{
    private BlockRegistry? _blockRegistry;
    private readonly RegistryCache<BlockDescriptorVisual> _visualDescriptorCache = new();
    
    public override void _Ready()
    {
        _blockRegistry = this.Component<BlockRegistry>();
    }
    
    public bool IsFullyOpaqueMaterial(short id)
    {
        return id != 0 && _visualDescriptorCache.Lookup(_blockRegistry!, id).IsFullyOpaque();
    }

    public bool IsVisibleMaterial(short id)
    {
        return id != 0 && _visualDescriptorCache.Lookup(_blockRegistry!, id).IsVisible();
    }
}
