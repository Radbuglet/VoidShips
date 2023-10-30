using System.Collections.Generic;
using Godot;
using VoidShips.util;

namespace VoidShips.game.voxel.mesh;

[Component]
public sealed partial class VoxelMeshWorld : Node
{
	[Export] public Material? MeshMaterial;

	private long _updateGeneration;

	public void UpdateMeshes(IEnumerable<VoxelDataChunk> chunks)
	{
		_updateGeneration += 1;

		foreach (var mainChunk in chunks)
		foreach (var otherChunk in mainChunk.NeighborsAndSelf())
		{
			var otherMesh = otherChunk.Component<VoxelMeshChunk>();
			if (otherMesh.UpdateGeneration == _updateGeneration) continue;
			
			otherMesh.UpdateMesh(MeshMaterial!);
			otherMesh.UpdateGeneration = _updateGeneration;
		}
	}
}
