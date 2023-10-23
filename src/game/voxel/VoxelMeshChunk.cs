using System.Linq;
using Godot;
using VoidShips.game.voxel.math;
using VoidShips.Util;

namespace VoidShips.game.voxel;

[Component]
public sealed partial class VoxelMeshChunk : Node
{
    internal long UpdateGeneration;
    private MultiMeshInstance3D? _mesh;

    private static readonly Transform3D[] BlockFaceTransforms = ArrayUtil.InitArray(BlockFaceExt.VariantCount, i =>
    {
        var face = (BlockFace) i;
        var xform = Transform3D.Identity;
        
        // Apply the rotation
        switch (face.Axis())
        {
            case Axis3.X:
                xform = xform.Rotated(new Vector3(0, 0, 1), -Mathf.Pi / 2);
                break;
            case Axis3.Y:
                break;
            case Axis3.Z:
                xform = xform.Rotated(new Vector3(1, 0, 0), Mathf.Pi / 2);
                break;
        }
        
        // Apply the flip
        if (face.IsSignNegative())
            xform = face.Axis().Rotate180Transform() * xform;
        
        // Apply the translation
        var translation = Vector3.One * 0.5F;
        translation[(int)face.Axis()] = face.IsSignNegative() ? 0F : 1F;
        xform = xform.Translated(translation);

        return xform;
    });

    public void UpdateMesh(Material meshMaterial)
    {
        if (_mesh == null)
        {
            _mesh = new MultiMeshInstance3D
            {
                Name = "ChunkMesh",
                Multimesh = new MultiMesh
                {
                    Mesh = new QuadMesh
                    {
                        Orientation = PlaneMesh.OrientationEnum.Y,
                        FlipFaces = false,
                        CenterOffset = Vector3.Zero,
                        Material = meshMaterial,
                    },
                    TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
                    UseColors = true,
                    UseCustomData = false,
                    VisibleInstanceCount = 0,
                    InstanceCount = 8,
                },
            };
            AddChild(_mesh);
        }
        
        // Construct voxel mesh data
        var chunk = this.Component<VoxelDataChunk>();
        var mm = _mesh.Multimesh;
        mm.VisibleInstanceCount = 0;
        
        for (var i = 0; i < VoxelCoordsExt.ChunkVolume; i++)
        {
            var ptr = chunk.GetPointer(i);
            var mainData = ptr.GetData();
            if (mainData == 0) continue;

            foreach (var face in BlockFaceExt.BlockFaces())
            {
                var neighborPos = ptr.Neighbor(face);
                if (neighborPos.GetData() != 0) continue;

                if (mm.VisibleInstanceCount >= mm.InstanceCount)
                {
                    var buffer = mm.Buffer;
                    System.Array.Resize(ref buffer, buffer.Length * 2);
                    mm.InstanceCount *= 2;
                    mm.Buffer = buffer;
                }

                var target = mm.VisibleInstanceCount++;
                mm.SetInstanceColor(target, Color.FromHsv(mainData / 5F, 0.5f, 0.5f));
                mm.SetInstanceTransform(target, BlockFaceTransforms[(int) face].Translated(ptr.Pos - chunk.MinWorldPos));
            }
        }
    }
}
