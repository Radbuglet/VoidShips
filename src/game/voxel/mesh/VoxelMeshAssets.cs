using System.Collections.Generic;
using Godot;
using VoidShips.game.registry;
using VoidShips.game.voxel.registry;
using VoidShips.util.gfx;
using VoidShips.util.polyfill;

namespace VoidShips.game.voxel.mesh;

[Component]
public sealed partial class VoxelMeshAssets : Node
{
    [Export] public Shader? MeshShader;
    
    // Dependencies
    private BlockRegistry? _blockRegistry;
    private VaryingTexture2DAllocator? _textureAllocator;
    
    // Block tracking
    private readonly RegistryCache<BlockDescriptorVisual> _visualDescriptorCache = new();
    
    // Layer tracking
    private readonly Dictionary<VoxelMeshLayerDescriptor, int> _layerIdMap = new();
    private readonly List<Material> _layerMaterialMap = new();
    
    // === Handlers === //

    public override void _Ready()
    {
        _blockRegistry = this.Component<BlockRegistry>();
        _textureAllocator = this.Component<VaryingTexture2DAllocator>();
    }

    // === Block Lookups === //

    public bool IsBlockMaterialVisible(short id)
    {
        return id != 0 && _visualDescriptorCache.Lookup(_blockRegistry!, id).IsVisible();
    }
    
    public bool IsBlockMaterialFullyOpaque(short id)
    {
        return id != 0 && _visualDescriptorCache.Lookup(_blockRegistry!, id).IsFullyOpaque();
    }
    
    public (int, int) GetBlockMaterialMeshAndTextureFrame(short id)
    {
        var descriptor = _visualDescriptorCache.Lookup(_blockRegistry!, id);
        if (descriptor.MeshLayer != int.MaxValue)
            return (descriptor.MeshLayer, descriptor.TextureFrameIndex);

        var reservation = _textureAllocator!.Allocate(descriptor.Texture!);
        descriptor.MeshLayer = LookupLayerId(new VoxelMeshLayerDescriptor
        {
            TextureArray = reservation.Arena,
        });
        descriptor.TextureFrameIndex = reservation.Frame;
        
        return (descriptor.MeshLayer, descriptor.TextureFrameIndex);
    }
    
    // === Layer Lookups === //
    
    public int LookupLayerId(VoxelMeshLayerDescriptor descriptor)
    {
        if (_layerIdMap.TryGetValue(descriptor, out var id))
            return id;

        id = _layerMaterialMap.Count;
        var material = new ShaderMaterial
        {
            Shader = MeshShader,
        };
        material.SetShaderParameter("textures", descriptor.TextureArray);
        _layerMaterialMap.Add(material);
        _layerIdMap.Add(descriptor, id);
        return id;
    }

    public Material LookupLayerMaterial(int id)
    {
        return _layerMaterialMap[id];
    }
}

public struct VoxelMeshLayerDescriptor
{
    public Texture2DArray TextureArray;
}
