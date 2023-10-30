using Godot;
using VoidShips.game.voxel.math;
using VoidShips.util.gfx;
using VoidShips.util.polyfill;

namespace VoidShips.game.voxel.mesh;

[Component]
public sealed partial class VoxelMeshChunkLayer : Node
{
    [Export] public Vector3 MinBlockEntityPos;
    [Export] public Material? MeshMaterial;
    
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

    public override void _Ready()
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
                    Material = MeshMaterial,
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

    public void PushFace(Vector3I blockWorldPos, BlockFace face, Color data)
    {
        _mesh!.Multimesh.ReserveCapacityForOne();

        var target = _mesh!.Multimesh.VisibleInstanceCount++;
        _mesh!.Multimesh.SetInstanceColor(target, data);
        _mesh!.Multimesh.SetInstanceTransform(target, BlockFaceTransforms[(int) face].Translated(blockWorldPos - MinBlockEntityPos));
    }

    public void CompleteUpload()
    {
        // Try to shrink the buffer if we ended up being significantly smaller than it
        _mesh!.Multimesh.ShrinkCapacity();
    }
}