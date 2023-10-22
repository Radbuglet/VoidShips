using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using VoidShips.game.voxel.math;
using VoidShips.Util;

namespace VoidShips.game.voxel;

[Component]
public sealed partial class VoxelDataChunk : Node
{
	internal VoxelDataChunk?[] Neighbors = new VoxelDataChunk[VoxelCoordsExt.ChunkVolume]; 
	
	public int AirBlockCount { get; private set; }
	public VoxelDataWorld? VoxelWorld { get; internal set; }
	private readonly short[] _rawBlockData = new short[VoxelCoordsExt.ChunkVolume];

	private Vector3I _chunkPos;
	public Vector3I MinWorldPos { get; private set; }
	public Vector3I ChunkPos
	{
		get => _chunkPos;
		set
		{
			Debug.Assert(VoxelWorld == null);
			_chunkPos = value;
			MinWorldPos = ChunkPos * VoxelCoordsExt.ChunkEdge;
			this.GameObject<Node3D>().Transform = Transform3D.Identity.Translated(MinWorldPos);
		}
	}

	internal bool IsDirty;

	public VoxelDataChunk? Neighbor(BlockFace face)
	{
		return Neighbors[(int) face];
	}

	public IEnumerable<VoxelDataChunk> NeighborsAndSelf()
	{
		yield return this;
		
		foreach (var face in BlockFaceExt.BlockFaces())
		{
			var neighbor = Neighbor(face);
			if (neighbor != null) yield return neighbor;
		}
	}

	public VoxelPointer GetPointer()
	{
		return new VoxelPointer(VoxelWorld!, this, MinWorldPos);
	}
	
	public VoxelPointer GetPointer(int index)
	{
		return new VoxelPointer(VoxelWorld!, this, MinWorldPos + index.BlockIndexToVec());
	}
	
	public VoxelPointer GetPointer(Vector3I pos)
	{
		Debug.Assert(pos.BlockVecIsValid());
		return new VoxelPointer(VoxelWorld!, this, MinWorldPos + pos);
	}

	public short GetBlockData(int index)
	{
		return _rawBlockData[index];
	}

	public void SetBlockData(int index, short data)
	{
		if (!IsDirty)
		{
			IsDirty = true;
			VoxelWorld?.DirtyChunks.Add(this);
		}

		var wasAir = _rawBlockData[index] == 0 ? 1 : 0;
		var isAir = data == 0 ? 1 : 0;
		AirBlockCount += isAir - wasAir;

		_rawBlockData[index] = data;
	}
}
