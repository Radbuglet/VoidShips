using Godot;
using VoidShips.game.voxel.math;
using VoidShips.Util;

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
}
