using Godot;
using VoidShips.game.voxel;
using VoidShips.game.voxel.loader;
using VoidShips.game.voxel.math;
using VoidShips.Util;

namespace VoidShips.scripts;

[Component]
public sealed partial class VoxelDemoRunner : Node
{
    private VoxelDataWorld? _worldData;
    private VoxelMeshWorld? _worldMesh;
    private VoxelLoaderImmediate? _worldLoader;
    
    public override void _Ready()
    {
        _worldData = this.Component<VoxelDataWorld>();
        _worldMesh = this.Component<VoxelMeshWorld>();
        _worldLoader = this.Component<VoxelLoaderImmediate>();
        
        // Populate an initial world
        for (var cx = 0; cx < 5; cx++)
        for (var cz = 0; cz < 5; cz++)
        {
            var chunk = _worldLoader.LoadChunk(new Vector3I(cx, 0, cz));
            
            for (var x = 0; x < VoxelCoordsExt.ChunkEdge; x++)
            for (var z = 0; z < VoxelCoordsExt.ChunkEdge; z++)
                chunk.Component<VoxelDataChunk>().GetPointer(new Vector3I(x, 0, z)).SetData(1);
        }
    }

    public override void _Process(double delta)
    {
        var dirtyChunks = _worldData!.FlushDirtyChunks();
        
        // Update meshes
        _worldMesh!.UpdateMeshes(dirtyChunks);
        
        // Delete empty
        _worldLoader!.DestroyEmptyImmediately(dirtyChunks);
    }
}
