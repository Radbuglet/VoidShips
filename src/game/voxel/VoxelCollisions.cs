using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using VoidShips.game.voxel.math;

namespace VoidShips.game.voxel;

// === Collisions === //

public static class VoxelCollisionCheckerExt
{
    private const float Tolerance = 0.0005F;

    public delegate bool ColliderFilter(VoxelPointer pointer, Node? meta);

    public static float CastVolume(this VoxelWorldPalletized facade, AaQuad3 quad, float delta, ColliderFilter? filter = null)
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
        var checkAabb = quad.Extrude(delta + Tolerance).EntityAabbToWorldAabb();
        var cachedLoc = facade.World.GetPointer(checkAabb.Position);
        var minDepth = delta;
        
        // For every block in the volume of potential occluders...
        foreach (var blockPos in checkAabb.WorldAabbIterBlocksIncl())
        {
            var block = cachedLoc.WithPos(blockPos);
            var blockMesh = facade.GetBlock(block).CollisionMesh;

            // For every occluder produced by that block...
            foreach (var (relOccluderQuad, meta) in blockMesh.PlanesFacing(quad.Normal.Inverse()))
            {
                // Transform the quad into the appropriate space
                var absOccluderQuad = relOccluderQuad.Offset(blockPos.WorldVecToNegativeCorner());
                
                // Filter occluders by whether we are affected by them.
                if (filter != null && !filter(block, meta)) {
                    continue;
                }

                if (!absOccluderQuad.Rect.Intersection(quad.Rect).HasArea())
                {
                    continue;
                }
                
                // Find its depth along the axis of movement
                var myDepth = absOccluderQuad.Origin;

                // And compare that to the starting depth to find the maximum allowable distance.
                var relDepth = Math.Abs(myDepth - quad.Origin);

                // Now, provide `tolerance`.
                // This step also ensures that, if the block plus its tolerance is outside of our delta,
                // it will have essentially zero effect on collision detection.
                relDepth -= Tolerance;

                // If `rel_depth` is negative, we were not within tolerance to begin with. We trace
                // this scenario for debug purposes.

                // We still clamp this to `[0..)`.
                minDepth = Math.Min(minDepth, relDepth);
            }
        }

        return minDepth;
    }

    public static Vector3 MoveRigidBody(this VoxelWorldPalletized facade, Aabb aabb, Vector3 delta, ColliderFilter? filter = null)
    {
        foreach (var axis in Axis3Ext.Variants()) {
            // Decompose the movement part
            var signedDelta = delta[(int) axis];
            var unsignedDelta = Math.Abs(signedDelta);
            var sign = signedDelta.BiasedSign();
            var face = BlockFaceExt.Compose(axis, sign);

            // Determine how far we can move
            var actualDelta = facade.CastVolume(aabb.FaceQuad(face), unsignedDelta, filter);

            // Commit the movement
            aabb.Position += face.UnitVectorF() * actualDelta;
        }

        return aabb.Position;
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
            CurrentBlock = CurrentBlock.Neighbor(intersections[i].EntryFace);
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
