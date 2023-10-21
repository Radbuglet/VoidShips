using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace VoidShips.game.voxel.math;

public static class VoxelCoordsExt
{
    public const int ChunkEdge = 16;
    public const int ChunkLayer = ChunkEdge * ChunkEdge;
    public const int ChunkVolume = ChunkLayer * ChunkEdge;
}

public static class EntityVecExt
{
    public static Vector3I EntityVecToWorldVec(this Vector3 entityVec)
    {
        return (Vector3I)entityVec.Floor();
    }

    public static AabbI EntityAabbToWorldAabb(this Aabb aabb)
    {
        return AabbIExt.FromCornersAbs(aabb.Position.EntityVecToWorldVec(), aabb.End.EntityVecToWorldVec());
    }
}

public static class WorldVecExt
{
    public static Vector3I Compose(Vector3I chunkVec, Vector3I blockVec)
    {
        return chunkVec + blockVec * VoxelCoordsExt.ChunkEdge;
    }

    public static Vector3I WorldVecToChunkVec(this Vector3I worldVec)
    {
        return new Vector3I(
            worldVec.X.DivEuclid(VoxelCoordsExt.ChunkEdge),
            worldVec.Y.DivEuclid(VoxelCoordsExt.ChunkEdge),
            worldVec.Z.DivEuclid(VoxelCoordsExt.ChunkEdge));
    }

    public static Vector3I WorldVecToBlockVec(this Vector3I worldVec)
    {
        return new Vector3I(
            worldVec.X.RemEuclid(VoxelCoordsExt.ChunkEdge),
            worldVec.Y.RemEuclid(VoxelCoordsExt.ChunkEdge),
            worldVec.Z.RemEuclid(VoxelCoordsExt.ChunkEdge));
    }

    public static Vector3 WorldVecToNegativeCorner(this Vector3I worldVec)
    {
        return worldVec;
    }

    public static AaPlane3 WorldVecToFacePlane(this Vector3I worldVec, BlockFace face)
    {
        return new AaPlane3(worldVec[(int)face.Axis()] + (face.IsSignNegative() ? 0 : 1), face);
    }

    public static IEnumerable<Vector3I> WorldAabbIterBlocksExcl(this AabbI aabb)
    {
        for (var x = aabb.Position.X; x < aabb.End.X; x++)
        for (var y = aabb.Position.Y; y < aabb.End.Y; y++)
        for (var z = aabb.Position.Z; z < aabb.End.Z; z++)
            yield return new Vector3I(x, y, z);
    }
    
    public static IEnumerable<Vector3I> WorldAabbIterBlocksIncl(this AabbI aabb)
    {
        for (var x = aabb.Position.X; x <= aabb.End.X; x++)
        for (var y = aabb.Position.Y; y <= aabb.End.Y; y++)
        for (var z = aabb.Position.Z; z <= aabb.End.Z; z++)
            yield return new Vector3I(x, y, z);
    }
}

public static class BlockVecExt
{
    public static bool BlockVecIsValid(this Vector3I blockVec)
    {
        return blockVec.X is >= 0 and < VoxelCoordsExt.ChunkEdge &&
               blockVec.Y is >= 0 and < VoxelCoordsExt.ChunkEdge &&
               blockVec.Z is >= 0 and < VoxelCoordsExt.ChunkEdge;
    }

    public static IEnumerable<(int, Vector3I)> BlockVecIter()
    {
        for (var i = 0; i < VoxelCoordsExt.ChunkVolume; i++)
        {
            yield return (i, BlockIndexToVec(i));
        }
    }

    public static int BlockVecToIndex(this Vector3I blockVec)
    {
        Debug.Assert(blockVec.BlockVecIsValid());
        return blockVec.X + blockVec.Y * VoxelCoordsExt.ChunkEdge + blockVec.Z * VoxelCoordsExt.ChunkLayer;
    }
    
    public static Vector3I? TryBlockVecFromIndex(int index)
    {
        return BlockIndexIsValid(index) ? BlockIndexToVec(index) : null;
    }

    public static Vector3I BlockIndexToVec(this int index)
    {
        return new Vector3I(
            index % VoxelCoordsExt.ChunkEdge, 
            index / VoxelCoordsExt.ChunkEdge % VoxelCoordsExt.ChunkEdge,
            index / VoxelCoordsExt.ChunkLayer);
    }

    public static bool BlockIndexIsValid(int index)
    {
        return index is >= 0 and < VoxelCoordsExt.ChunkVolume;
    }
}