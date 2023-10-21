using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using VoidShips.game.voxel.math;
using VoidShips.game.voxel.registry;
using Axis3 = VoidShips.game.voxel.math.Axis3;
using BlockFace = VoidShips.game.voxel.math.BlockFace;
using Segment3D = VoidShips.game.voxel.math.Segment3D;

namespace VoidShips.game.voxel;

// === Collisions === //

public sealed class VoxelCollisionChecker
{
    public readonly VoxelDataWorld World;
    public readonly BlockRegistry Registry;
    public float Tolerance = 0.0005F;
    
    public VoxelCollisionChecker(VoxelDataWorld world, BlockRegistry registry)
    {
        World = world;
        Registry = registry;
    }

    public float CastVolume(AaQuad3 quad, float depth)
    {
        // N.B. to ensure that `tolerance` is respected, we have to increase our check volume by
        // `tolerance` so that we catch blocks that are outside the check volume but may nonetheless
        // wish to enforce a tolerance margin of their own.
        //
        // We do this to prevent the following scenario:
        //
        // ```
        // 0   1   2
        // *---*---*
        // | %--->*|
        // *---*---*
        //       |--   Let's say this is the required tolerance...
        //   |----|    ...and this is the actual movement delta.
        //
        // Because our movement delta never hits the block face at `x = 2`, it never requires the face to
        // contribute to tolerance checking, allowing us to bypass its tolerance and eventually "tunnel"
        // through the occluder.
        // ```
        //
        // If these additional blocks don't contribute to collision detection with their tolerance, we'll
        // just ignore them.
        var checkAabb = quad.Extrude(depth + Tolerance).EntityAabbToWorldAabb();
        var cachedLoc = World.GetPointer(checkAabb.Position);

        // TODO: https://github.com/Radbuglet/crucible/blob/be6acd16ba5f5db22bc59eed80478333ffecf830/src/foundation/shared/src/voxel/collision.rs#L358
        throw new NotImplementedException();
    }
}

// === RayCast === //

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
        CurrentBlock = world.GetPointer(start.EntityVecToWorldVec());

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
        var newBlock = CurrentBlock.WithPos(Position.EntityVecToWorldVec());
        var blockDelta = newBlock.Pos - CurrentBlock.Pos;
        
        // Find all our intersections
        for (var axis = 0; axis < 3; axis++)
        {
            // Determine the face into which we traveled.
            var delta = blockDelta[axis];
            if (delta == 0) continue;
            var face = BlockFaceExt.Compose((Axis3) axis, delta.BiasedSign());
            
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
