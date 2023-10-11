using System.Diagnostics;
using Godot;
using VoidShips.Util;

namespace VoidShips.actors.voxel;

[Component]
public sealed partial class VoxelDataChunk : Node
{
    internal VoxelDataChunk?[] Neighbors = new VoxelDataChunk[VoxelMath.ChunkVolume]; 
    
    public VoxelDataWorld? VoxelWorld { get; internal set; }
    public readonly short[] RawBlockData = new short[VoxelMath.ChunkVolume];

    private Vector3I _chunkPos;
    public Vector3I ChunkPos
    {
        get => _chunkPos;
        set
        {
            Debug.Assert(VoxelWorld == null);
            _chunkPos = value;
        }
    }

    public VoxelDataChunk? Neighbor(BlockFace face)
    {
        return Neighbors[(int) face];
    }
}
