using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace VoidShips.game.voxel.math;

public static class VoxelCoords
{
    public const int ChunkEdge = 16;
    public const int ChunkLayer = ChunkEdge * ChunkEdge;
    public const int ChunkVolume = ChunkLayer * ChunkEdge;
    
    // === EntityVec === //
    
    public static Vector3I EntityToWorldVec(this Vector3 entityVec)
    {
        return (Vector3I) entityVec.Floor();
    }

    // === WorldVec === //

    public static Vector3I WorldVecFromParts(Vector3I chunkVec, Vector3I blockVec)
    {
        return chunkVec + blockVec * ChunkEdge;
    }

    public static Vector3I WorldToChunkVec(this Vector3I worldVec)
    {
        return new Vector3I(
            worldVec.X.DivEuclid(ChunkEdge),
            worldVec.Y.DivEuclid(ChunkEdge),
            worldVec.Z.DivEuclid(ChunkEdge));
    }
    
    public static Vector3I WorldToBlockVec(this Vector3I worldVec)
    {
        return new Vector3I(
            worldVec.X.RemEuclid(ChunkEdge),
            worldVec.Y.RemEuclid(ChunkEdge),
            worldVec.Z.RemEuclid(ChunkEdge));
    }

    public static Vector3 WorldToNegativeCornerVec(this Vector3I worldVec)
    {
        return worldVec;
    }
    
    public static AaPlane3 WorldVecToFacePlane(this Vector3I worldVec, BlockFace face)
    {
        return new AaPlane3(worldVec[(int)face.Axis()] + (face.IsSignNegative() ? 0 : 1), face);        
    }
    
    // === BlockVec === //

    public static bool BlockVecIsValid(this Vector3I blockVec)
    {
        return blockVec.X is >= 0 and < ChunkEdge && blockVec.Y is >= 0 and < ChunkEdge && blockVec.Z is >= 0 and < ChunkEdge;
    }

    public static IEnumerable<(int, Vector3I)> BlockVecIter()
    {
        for (var i = 0; i < ChunkVolume; i++)
        {
            yield return (i, BlockVecFromIndex(i));
        }
    }

    public static int BlockVecToIndex(this Vector3I blockVec)
    {
        Debug.Assert(blockVec.BlockVecIsValid());
        return blockVec.X + blockVec.Y * ChunkEdge + blockVec.Z * ChunkLayer;
    }
    
    public static Vector3I? TryBlockVecFromIndex(int index)
    {
        return BlockIsValidIndex(index) ? BlockVecFromIndex(index) : null;
    }

    public static Vector3I BlockVecFromIndex(int index)
    {
        return new Vector3I(
            index % ChunkEdge, 
            index / ChunkEdge % ChunkEdge,
            index / ChunkLayer);
    }

    public static bool BlockIsValidIndex(int index)
    {
        return index is >= 0 and < ChunkVolume;
    }
}