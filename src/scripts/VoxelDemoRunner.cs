using Godot;
using VoidShips.game.voxel;
using VoidShips.game.voxel.math;
using VoidShips.Util;

namespace VoidShips.scripts;

[Component]
public sealed partial class VoxelDemoRunner : Node
{
    private VoxelDataWorld? _worldData;
    private VoxelMeshWorld? _worldMesh;
    
    public override void _Ready()
    {
        _worldData = this.Component<VoxelDataWorld>();
        _worldMesh = this.Component<VoxelMeshWorld>();
        
        // Create main chunk
        var chunkScene = GD.Load<PackedScene>("res://res/actors/voxel_chunk.tscn");

        var mainChunk = chunkScene.Instantiate<Node>();
        mainChunk.Component<VoxelDataChunk>().ChunkPos = new Vector3I(0, 0, 0);
        _worldData.AddChunk(mainChunk.Component<VoxelDataChunk>());
        
        // Populate it
        for (var x = 0; x < VoxelCoordsExt.ChunkEdge; x++)
        for (var z = 0; z < VoxelCoordsExt.ChunkEdge; z++)
            mainChunk.Component<VoxelDataChunk>().GetPointer(new Vector3I(x, 0, z)).SetData(1);
    }

    public override void _Process(double delta)
    {
        // Update mesh
        var dirtyChunks = _worldData!.FlushDirtyChunks();
        _worldMesh!.UpdateMeshes(dirtyChunks);
    }
}
