using System.Linq;
using Godot;
using VoidShips.game.voxel.math;
using VoidShips.Util;

namespace VoidShips.game.voxel;

[Component]
public sealed partial class VoxelMeshChunk : Node
{
    internal long UpdateGeneration;
    private MultiMeshInstance3D[]? _meshes;

    public void UpdateMesh(Material meshMaterial)
    {
        if (_meshes == null)
        {
            _meshes = new MultiMeshInstance3D[BlockFaceExt.VariantCount];
            for (var i = 0; i < _meshes.Length; i++)
                _meshes[i] = new MultiMeshInstance3D();
            
            foreach (var (i, mesh) in _meshes.AsEnumerable().Select((v, i) => (i, v)))
            {
                var face = (BlockFace)i;
                var centerOffset = Vector3.One * 0.5f;
                centerOffset[(int)face.Axis()] = 0f;

                mesh.Multimesh = new MultiMesh
                {
                    Mesh = new QuadMesh
                    {
                        Orientation = (PlaneMesh.OrientationEnum)face.Axis(),
                        FlipFaces = face.IsSignNegative(),
                        CenterOffset = centerOffset,
                        Material = meshMaterial,
                    },
                    TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
                    UseColors = true,
                    UseCustomData = false,
                    VisibleInstanceCount = 0,
                    InstanceCount = 5000,
                };

                AddChild(mesh);
            }
        }
        
        // Construct voxel mesh data
        foreach (var mesh in _meshes) mesh.Multimesh.VisibleInstanceCount = 0;
        
        var chunk = this.Component<VoxelDataChunk>();
        
        for (var i = 0; i < VoxelCoordsExt.ChunkVolume; i++)
        {
            var ptr = chunk.GetPointer(i);
            var mainData = ptr.GetData();
            if (mainData == 0) continue;

            foreach (var face in BlockFaceExt.BlockFaces())
            {
                var neighborPos = ptr.Neighbor(face);
                if (neighborPos.GetData() != 0) continue;

                var mm = _meshes[(int)face].Multimesh;
                
                var instanceCenter = ptr.Pos - chunk.MinWorldPos;
                if (!face.IsSignNegative())
                    instanceCenter[(int)face.Axis()] += 1;
                
                mm.SetInstanceTransform(mm.VisibleInstanceCount, Transform3D.Identity.Translated(instanceCenter));
                mm.SetInstanceColor(mm.VisibleInstanceCount++, Color.FromHsv(mainData / 5F, 0.5f, 0.5f));
            }
        }
    }
}
