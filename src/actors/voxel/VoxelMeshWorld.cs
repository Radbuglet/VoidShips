using System.Collections.Generic;
using Godot;
using VoidShips.Util;

namespace VoidShips.actors.voxel;

[Component]
public sealed partial class VoxelMeshWorld : Node
{
    [Export] public Material? MeshMaterial;

    public void UpdateMeshes(IEnumerable<VoxelDataChunk> chunks)
    {
        foreach (var chunk in chunks)
            chunk.Component<VoxelMeshChunk>().UpdateMesh(MeshMaterial!);
    }
}
