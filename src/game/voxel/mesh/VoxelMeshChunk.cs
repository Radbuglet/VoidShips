using Godot;
using VoidShips.util.polyfill;

namespace VoidShips.game.voxel.mesh;

[Component]
public sealed partial class VoxelMeshChunk : Node
{
    internal long UpdateGeneration;

    public void UpdateMesh(VoxelMeshWorld meshWorld)
    {
        
    }
}
