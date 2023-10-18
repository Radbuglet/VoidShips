using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using VoidShips.game.voxel.math;
using Axis3 = VoidShips.game.voxel.math.Axis3;
using BlockFace = VoidShips.game.voxel.math.BlockFace;
using Segment3D = VoidShips.game.voxel.math.Segment3D;

namespace VoidShips.game.voxel;

public sealed class VoxelRaycast
{
    public Vector3 Position { get; private set; }
    public VoxelPointer CurrentBlock { get; private set; }
    public readonly Vector3 StepDelta;

    public float Distance { get; private set; }
    public readonly float MaxDistance;

    public VoxelRaycast(VoxelDataWorld world, Vector3 start, Vector3 end)
    {
        Position = start;
        CurrentBlock = world.GetPointer(VoxelCoords.EntityToWorldVec(start));

        var delta = end - start;
        Distance = 0;
        MaxDistance = delta.Length();
        StepDelta = MaxDistance > 0 ? delta / MaxDistance : Vector3.Zero;
    }

    public List<VoxelRaycastCollision> StepRaw()
    {
        var intersections = new List<VoxelRaycastCollision>();
        var stepSegment = new Segment3D(Position, Position + StepDelta);
        
        // Update the position and figure out where we moved to.
        Position += StepDelta;
        var newBlock = CurrentBlock.WithPos(Position.EntityToWorldVec());
        var blockDelta = newBlock.Pos - CurrentBlock.Pos;
        
        // Find all our intersections
        for (var axis = 0; axis < 3; axis++)
        {
            // Determine the face into which we traveled.
            var delta = blockDelta[axis];
            if (delta == 0) continue;
            var face = BlockFaceExt.Compose((Axis3) axis, delta.GetSignBiased());
            
            // Determine the position of the intersection.
            var plane = CurrentBlock.Pos.WorldVecToFacePlane(face);
            var intersection = plane.Intersection(stepSegment);
            Debug.Assert(intersection.IsValid);
            
            // Get the distance of the intersection
            var dist = Distance + intersection.Lerp;
            if (dist > MaxDistance) break;
            
            intersections.Add(new VoxelRaycastCollision
            {
                Pointer = CurrentBlock,  // This will be overwritten later.
                Position = intersection.Pos,
                Distance = dist,
                EntryFace = face
            });
        }
        
        // Sort by their entrance order
        intersections.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        
        // And update their block pointers.
        for (var i = 0; i < intersections.Count; i++)
        {
            CurrentBlock = CurrentBlock.Neighbor((math.BlockFace) intersections[i].EntryFace);
            intersections[i] = intersections[i] with
            {
                Pointer = CurrentBlock,
            };
        }
        
        // Update the total distance
        Distance += 1;

        return intersections;
    }
}

public struct VoxelRaycastCollision
{
    public VoxelPointer Pointer;
    public float Distance;
    public BlockFace EntryFace;
    public Vector3 Position;
}
