using Godot;
using VoidShips.actors.voxel;
using VoidShips.Util;

namespace VoidShips.scripts;

[Component]
public sealed partial class VoxelDemoRunner : Node
{
    public override void _Ready()
    {
        var worldData = this.Component<VoxelDataWorld>();
        var worldMesh = this.Component<VoxelMeshWorld>();
        
        // Create main chunk
        var chunkScene = GD.Load<PackedScene>("res://res/actors/voxel_chunk.tscn");

        var mainChunk = chunkScene.Instantiate<Node>();
        mainChunk.Component<VoxelDataChunk>().ChunkPos = new Vector3I(0, 0, 0);
        worldData.AddChunk(mainChunk.Component<VoxelDataChunk>());
        
        // Populate it
        var rng = new RandomNumberGenerator();

        for (var i = 0; i < VoxelMath.ChunkVolume; i++)
        {
            var material = rng.RandiRange(0, 5) == 0 ? rng.RandiRange(1, 5) : 0;
            mainChunk.Component<VoxelDataChunk>().GetPointer(i).SetData((short)material);
        }

        // Update mesh
        var dirtyChunks = worldData.FlushDirtyChunks();
        worldMesh.UpdateMeshes(dirtyChunks);
    }
}
