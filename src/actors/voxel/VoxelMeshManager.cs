using System.Linq;
using Godot;
using VoidShips.Util;

namespace VoidShips.actors.voxel;

[Component]
public sealed partial class VoxelMeshManager : Node
{
    [Export] public Material? MeshMaterial;
    
    private readonly MultiMeshInstance3D[] _meshes = new MultiMeshInstance3D[VoxelMath.BlockFaceCount];
    
    public override void _Ready()
    {
        for (var i = 0; i < _meshes.Length; i++)
            _meshes[i] = new MultiMeshInstance3D();
        
        // Construct mesh instances
        foreach (var (i, mesh) in _meshes.AsEnumerable().Select((v, i) => (i, v)))
        {
            var face = (BlockFace) i;
            var centerOffset = Vector3.One * 0.5f;
            centerOffset[(int) face.Axis()] = 0f;
            
            mesh.Multimesh = new MultiMesh
            {
                Mesh = new QuadMesh
                {
                    Orientation = (PlaneMesh.OrientationEnum) face.Axis(),
                    FlipFaces = face.IsSignNegative(),
                    CenterOffset = centerOffset,
                    Material = MeshMaterial,
                },
                TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
                UseColors = true,
                UseCustomData = false,
                VisibleInstanceCount = 0,
                InstanceCount = 5000,
            };
            
            AddChild(mesh);
        }
        
        // Construct voxel data
        var data = new byte[VoxelMath.ChunkVolume];
        var rng = new RandomNumberGenerator();

        for (var i = 0; i < data.Length; i++)
            data[i] = (byte) rng.RandiRange(0, 5);
        
        // Construct voxel mesh data
        for (var i = 0; i < data.Length; i++)
        {
            if (data[i] == 0) continue;
            
            var pos = VoxelMath.BlockIndexToPos(i);

            foreach (var face in VoxelMath.BlockFaces())
            {
                var neighborPos = pos + face.UnitVector();
                if (neighborPos.IsValidBlockPos() && data[neighborPos.BlockPosToIndex()] != 0) continue;

                var mm = _meshes[(int)face].Multimesh;
                
                var instanceCenter = pos;
                if (!face.IsSignNegative())
                    instanceCenter[(int)face.Axis()] += 1;
                
                mm.SetInstanceTransform(mm.VisibleInstanceCount, Transform3D.Identity.Translated(instanceCenter));
                mm.SetInstanceColor(mm.VisibleInstanceCount++, Color.FromHsv(data[i] / 5F, 0.5f, 0.5f));
            }
        }
    }
}