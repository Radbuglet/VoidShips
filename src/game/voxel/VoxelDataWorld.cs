using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using VoidShips.game.voxel.math;
using VoidShips.util;

namespace VoidShips.game.voxel;

[Component]
public sealed partial class VoxelDataWorld : Node
{
    [Signal] public delegate void ChunkAddedEventHandler(VoxelDataChunk chunk);
    [Signal] public delegate void ChunkAboutToRemoveEventHandler(VoxelDataChunk chunk);
    
    private readonly Dictionary<Vector3I, VoxelDataChunk> _chunks = new();
    internal HashSet<VoxelDataChunk> DirtyChunks = new();

    public int ChunkCount => _chunks.Count;

    public void AddChunk(VoxelDataChunk chunk)
    {
        // Attach game object
        var chunkObj = chunk.GameObject<Node>();
        chunkObj.Name = chunk.ChunkPos.ToString();
        AddChild(chunkObj);
        
        // Update neighbors
        foreach (var face in BlockFaceExt.BlockFaces())
        {
            _chunks.TryGetValue(chunk.ChunkPos + face.UnitVectorI(), out var neighbor);
            chunk.Neighbors[(int)face] = neighbor;
            if (neighbor != null)
                neighbor.Neighbors[(int)face.Inverse()] = chunk;
        }
        
        // Mark the chunk as dirty
        chunk.IsDirty = true;
        DirtyChunks.Add(chunk);

        // Register chunk
        chunk.VoxelWorld = this;
        _chunks.Add(chunk.ChunkPos, chunk);
        
        // Send signal
        EmitSignal(SignalName.ChunkAdded, chunk);
    }

    public void RemoveChunk(VoxelDataChunk chunk)
    {
        Debug.Assert(chunk.VoxelWorld == this);
        
        // Send signal
        EmitSignal(SignalName.ChunkAboutToRemove, chunk);
        
        // Remove the chunk from the map
        _chunks.Remove(chunk.ChunkPos);
        chunk.VoxelWorld = null;
        
        // Unlink neighbors and mark them as dirty
        foreach (var face in BlockFaceExt.BlockFaces())
        {
            var neighbor = chunk.Neighbor(face);
            if (neighbor == null) continue;
            
            neighbor.Neighbors[(int) face.Inverse()] = null;
            neighbor.MarkDirty();
        }
        
        // Unmark as dirty
        if (chunk.IsDirty) DirtyChunks.Remove(chunk);

        // Remove game object
        chunk.GameObject<Node>().QueueFree();
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
        foreach (var chunk in oldSet)
            chunk.IsDirty = false;
        
        DirtyChunks = new HashSet<VoxelDataChunk>();
        return oldSet;
    }
}
