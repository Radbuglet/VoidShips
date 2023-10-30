using System.Collections.Generic;
using Godot;
using VoidShips.game.voxel.math;
using VoidShips.util.polyfill;

namespace VoidShips.game.voxel.loader;

[Component]
public sealed partial class VoxelLoaderImmediate : Node
{
    [Export] public PackedScene? ChunkScene;

    private VoxelDataWorld? _world;

    public override void _Ready()
    {
        _world = this.Component<VoxelDataWorld>();
    }
    
    public VoxelDataChunk LoadChunk(Vector3I chunkPos)
    {
        // Try to grab an existing chunk.
        {
            var chunk = _world!.GetChunk(chunkPos);
            if (chunk != null) return chunk;
        }
        
        // Otherwise, create it from scratch.
        {
            var chunk = ChunkScene!.Instantiate();
            var chunkData = chunk.Component<VoxelDataChunk>(); 
            chunkData.ChunkPos = chunkPos;
            _world!.AddChunk(chunkData);
            return chunkData;
        }
    }

    public VoxelDataChunk LoadChunk(VoxelPointer pointer)
    {
        // Try to grab an existing chunk.
        {
            var chunk = pointer.GetChunk();
            if (chunk != null) return chunk;
        }
        
        // Otherwise, create it from scratch.
        {
            var chunk = ChunkScene!.Instantiate();
            var chunkData = chunk.Component<VoxelDataChunk>(); 
            chunkData.ChunkPos = pointer.Pos.WorldVecToChunkVec();
            _world!.AddChunk(chunkData);
            return chunkData;
        }
    }

    public void DestroyEmptyImmediately(IEnumerable<VoxelDataChunk> dirty)
    {
        foreach (var chunk in dirty)
        {
            if (chunk.AirBlockCount == 0) chunk.VoxelWorld!.RemoveChunk(chunk);
        }
    }
}
