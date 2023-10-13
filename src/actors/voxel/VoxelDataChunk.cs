using System.Diagnostics;
using Godot;
using VoidShips.Util;

namespace VoidShips.actors.voxel;

[Component]
public sealed partial class VoxelDataChunk : Node
{
    internal VoxelDataChunk?[] Neighbors = new VoxelDataChunk[VoxelMath.ChunkVolume]; 
    
    public VoxelDataWorld? VoxelWorld { get; internal set; }
    private readonly short[] _rawBlockData = new short[VoxelMath.ChunkVolume];

    private Vector3I _chunkPos;
    private Vector3I _minWorldPos;
    public Vector3I ChunkPos
    {
        get => _chunkPos;
        set
        {
            Debug.Assert(VoxelWorld == null);
            _chunkPos = value;
            _minWorldPos = ChunkPos * VoxelMath.ChunkEdgeLength;
        }
    }

    private bool _isDirty;

    public VoxelDataChunk? Neighbor(BlockFace face)
    {
        return Neighbors[(int) face];
    }

    public VoxelPointer GetPointer()
    {
        return new VoxelPointer(VoxelWorld!, this, _minWorldPos);
    }
    
    public VoxelPointer GetPointer(int index)
    {
        return new VoxelPointer(VoxelWorld!, this, _chunkPos + VoxelMath.BlockIndexToPos(index));
    }

    public short GetBlockData(int index)
    {
        return _rawBlockData[index];
    }

    public void SetBlockData(int index, short data)
    {
        if (!_isDirty)
        {
            _isDirty = true;
            VoxelWorld?.DirtyChunks.Add(this);
        }

        _rawBlockData[index] = data;
    }
}
