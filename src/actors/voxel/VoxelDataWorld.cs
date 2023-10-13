using System.Collections.Generic;
using Godot;
using VoidShips.Util;

namespace VoidShips.actors.voxel;

[Component]
public sealed partial class VoxelDataWorld : Node
{
    [Signal] public delegate void ChunkAddedEventHandler(VoxelDataChunk chunk);
    
    private readonly Dictionary<Vector3I, VoxelDataChunk> _chunks = new();
    internal HashSet<VoxelDataChunk> DirtyChunks = new();

    public void AddChunk(VoxelDataChunk chunk)
    {
        // Attach game object
        var chunkObj = chunk.GameObject<Node>();
        chunkObj.Name = chunk.ChunkPos.ToString();
        AddChild(chunkObj);
        
        // Update neighbors
        foreach (var face in VoxelMath.BlockFaces())
        {
            _chunks.TryGetValue(chunk.ChunkPos + face.UnitVector(), out var neighbor);
            chunk.Neighbors[(int)face] = neighbor;
            if (neighbor != null)
                neighbor.Neighbors[(int)face.Inverse()] = chunk;
        }

        // Register chunk
        chunk.VoxelWorld = this;
        _chunks.Add(chunk.ChunkPos, chunk);
        DirtyChunks.Add(chunk);
        EmitSignal(SignalName.ChunkAdded, chunk);
    }

    public VoxelDataChunk? GetChunk(Vector3I pos)
    {
        return _chunks.TryGetValue(pos, out var chunk) ? chunk : null;
    }

    public VoxelPointer GetPointer(Vector3I pos)
    {
        return new VoxelPointer(this, pos);
    }

    public HashSet<VoxelDataChunk> FlushDirtyChunks()
    {
        var oldSet = DirtyChunks;
        DirtyChunks = new HashSet<VoxelDataChunk>();
        return oldSet;
    }
}
