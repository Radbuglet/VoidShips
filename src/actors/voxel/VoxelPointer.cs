using Godot;

namespace VoidShips.actors.voxel;

public readonly struct VoxelPointer
{
    public readonly VoxelDataWorld World;
    public readonly VoxelDataChunk? Cache;
    public readonly Vector3I Pos;

    private VoxelPointer(VoxelDataWorld world, VoxelDataChunk? cache, Vector3I pos)
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

    public VoxelPointer Neighbor(BlockFace face)
    {
        var newPos = Pos + face.UnitVector();
        var newCache = Cache;
        if (Pos.WorldVecToChunkVec() != newPos.WorldVecToChunkVec()) newCache = newCache?.Neighbor(face);

        return new VoxelPointer(World, newCache, newPos);
    }
    
    public VoxelPointer GetChunk()
    {
        return GetChunk(out _);
    }

    public VoxelPointer GetChunk(out VoxelDataChunk? chunk)
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
}
