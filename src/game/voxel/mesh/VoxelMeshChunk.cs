using System.Collections.Generic;
using Godot;
using VoidShips.game.voxel.math;
using VoidShips.util.polyfill;

namespace VoidShips.game.voxel.mesh;

[Component]
public sealed partial class VoxelMeshChunk : Node
{
    internal long UpdateGeneration;
    private readonly List<VoxelMeshChunkLayer?> _layers = new(); 

    public void UpdateMesh(VoxelMeshWorld meshWorld)
    {
        var chunk = this.Component<VoxelDataChunk>();
        var assetRegistry = meshWorld.AssetRegistry!;

        foreach (var layer in _layers)
            layer?.BeginUpload();
        
        for (var i = 0; i < VoxelCoordsExt.ChunkVolume; i++)
        {
            var ptr = chunk.GetPointer(i);
            var mainData = ptr.GetData();
            if (!assetRegistry.IsBlockMaterialVisible(mainData)) continue;

            foreach (var face in BlockFaceExt.BlockFaces())
            {
                var neighborPos = ptr.Neighbor(face);
                if (assetRegistry.IsBlockMaterialFullyOpaque(neighborPos.GetData())) continue;

                var (layerIdx, frameIdx) = assetRegistry.GetBlockMaterialMeshAndTextureFrame(mainData);

                GetLayer(assetRegistry, layerIdx).PushFace(ptr.Pos, face, new Color(frameIdx, 0, 0));
            }
        }
        
        foreach (var layer in _layers)
            layer?.FinishUpload();
    }

    public VoxelMeshChunkLayer GetLayer(VoxelMeshAssets assets, int layerIndex)
    {
        if (_layers.TryGet(layerIndex, out var layer) && layer != null)
            return layer;
        
        _layers.EnsureMinLength(layerIndex + 1, id =>
        {
            if (id != layerIndex) return null;

            var newLayer = new VoxelMeshChunkLayer
            {
                Name = "VoxelMeshChunkLayer",
                MeshMaterial = assets.LookupLayerMaterial(id),
            };
            AddChild(newLayer);
            return newLayer;
        });

        return _layers[layerIndex]!;
    }
}
