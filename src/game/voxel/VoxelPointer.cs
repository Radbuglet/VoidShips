using Godot;
using VoidShips.game.voxel.math;

namespace VoidShips.game.voxel;

public readonly struct VoxelPointer
{
    public readonly VoxelDataWorld World;
    public readonly VoxelDataChunk? Cache;
    public readonly Vector3I Pos;

    public VoxelPointer(VoxelDataWorld world, VoxelDataChunk? cache, Vector3I pos)
    {
        World = world;
        Cache = cache;
        Pos = pos;
    }

    public VoxelPointer(VoxelDataWorld world, Vector3I pos)
    {
        World = world;
        Cache = null;
        Pos = pos;
    }

    public VoxelPointer WithWorld(VoxelDataWorld world)
    {
        return new VoxelPointer(world, null, Pos);
    }

    public VoxelPointer WithPos(Vector3I pos)
    {
        return new VoxelPointer(World, pos.WorldVecToChunkVec() == Pos.WorldVecToChunkVec() ? Cache : null, pos);
    }
    
    public static VoxelPointer operator +(VoxelPointer ptr, Vector3I rel)
    {
        return ptr.WithPos(ptr.Pos + rel);
    }

    public VoxelPointer Neighbor(BlockFace face)
    {
        var newPos = Pos + face.UnitVector();
        var newCache = Cache;
        if (Pos.WorldVecToChunkVec() != newPos.WorldVecToChunkVec()) newCache = newCache?.Neighbor(face);

        return new VoxelPointer(World, newCache, newPos);
    }
    
    public int GetBlockIndex()
    {
        return Pos.WorldVecToBlockVec().BlockPosToIndex();
    }
    
    public VoxelPointer FetchChunk()
    {
        return FetchChunk(out _);
    }

    public VoxelPointer FetchChunk(out VoxelDataChunk? chunk)
    {
        if (Cache != null)
        {
            chunk = Cache;
            return this;
        }

        var mutated = new VoxelPointer(World, World.GetChunk(Pos.WorldVecToChunkVec()), Pos);
        chunk = mutated.Cache;
        return mutated;
    }

    public short GetData()
    {
        FetchChunk(out var chunk);
        return chunk?.GetBlockData(GetBlockIndex()) ?? 0;
    }
    
    public void SetData(short data)
    {
        FetchChunk(out var chunk);
        chunk?.SetBlockData(GetBlockIndex(), data);
    }
}
