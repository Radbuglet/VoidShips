using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace VoidShips.actors.voxel;

public static class VoxelMath
{
    // === Constants === //
    
    public const int ChunkEdgeLength = 16;
    public const int ChunkVolume = ChunkEdgeLength * ChunkEdgeLength * ChunkEdgeLength;
    public const int BlockFaceCount = 6;

    // === BlockFace === //
    
    public static readonly Vector3[] FaceVectors =
    {
        new(1, 0, 0),
        new(-1, 0, 0),
        new(0, 1, 0),
        new(0, -1, 0),
        new(0, 0, 1),
        new(0, 0, -1)
    };
    
    public static readonly Vector3I[] FacesVectorsI =
    {
        new(1, 0, 0),
        new(-1, 0, 0),
        new(0, 1, 0),
        new(0, -1, 0),
        new(0, 0, 1),
        new(0, 0, -1)
    };

    public static Axis3 Axis(this BlockFace face)
    {
        return (Axis3)((int) face / 2);
    }
    
    public static bool IsSignNegative(this BlockFace face)
    {
        return (int)face % 2 == 1;
    }

    public static BlockFace Inverse(this BlockFace face)
    {
        return (BlockFace)((int) face ^ 1);
    }

    public static IEnumerable<BlockFace> BlockFaces()
    {
        for (var i = 0; i < BlockFaceCount; i++)
            yield return (BlockFace) i;
    }

    public static Vector3I UnitVector(this BlockFace face)
    {
        return FacesVectorsI[(int) face];
    }
    
    // === Voxel Vectors === //
    
    public static Vector3I BlockIndexToPos(int idx)
    {
        return new Vector3I(
            idx % ChunkEdgeLength, 
            idx / ChunkEdgeLength % ChunkEdgeLength,
            idx / ChunkEdgeLength / ChunkEdgeLength);
    }

    public static int BlockPosToIndex(this Vector3I pos)
    {
        Debug.Assert(pos.IsValidBlockPos(), $"{pos} is not a valid block pos");
        return pos.X + pos.Y * ChunkEdgeLength + pos.Z * ChunkEdgeLength * ChunkEdgeLength;
    }

    public static bool IsValidBlockPos(this Vector3I pos)
    {
        return pos.X is >= 0 and < 16 && pos.Y is >= 0 and < 16 && pos.Z is >= 0 and < 16;
    }

    public static Vector3I WorldVecToChunkVec(this Vector3I vec)
    {
        return new Vector3I(
            DivEuclid(vec.X, ChunkEdgeLength),
            DivEuclid(vec.Y, ChunkEdgeLength),
            DivEuclid(vec.Z, ChunkEdgeLength));
    }
    
    public static Vector3I WorldVecToBlockVec(this Vector3I vec)
    {
        return new Vector3I(
            RemEuclid(vec.X, ChunkEdgeLength),
            RemEuclid(vec.Y, ChunkEdgeLength),
            RemEuclid(vec.Z, ChunkEdgeLength));
    }
    
    // === Remainder Stuff === //
    
    public static int DivEuclid(this int lhs, int rhs) {
        var q = lhs / rhs;
        if (lhs % rhs < 0)
            return rhs > 0 ? q - 1 : q + 1;
        
        return q;
    }

    public static int RemEuclid(this int lhs, int rhs)
    {
        var r = lhs % rhs;
        if (r < 0)
            return r + Math.Abs(rhs);
        
        return r;
    }
}

public enum Axis3
{
    X,
    Y,
    Z
}

public enum BlockFace
{
    PosX,
    NegX,
    PosY,
    NegY,
    PosZ,
    NegZ
}