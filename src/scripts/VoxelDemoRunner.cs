using Godot;
using VoidShips.actors.voxel;
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
        var rng = new RandomNumberGenerator();

        for (var i = 0; i < VoxelMath.ChunkVolume; i++)
        {
            var material = rng.RandiRange(0, 5) == 0 ? rng.RandiRange(1, 5) : 0;
            mainChunk.Component<VoxelDataChunk>().GetPointer(i).SetData((short)material);
        }
    }

    public override void _Process(double delta)
    {
        // Update mesh
        var dirtyChunks = _worldData!.FlushDirtyChunks();
        _worldMesh!.UpdateMeshes(dirtyChunks);
    }
}
