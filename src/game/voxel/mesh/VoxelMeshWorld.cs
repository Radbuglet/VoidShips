using System.Collections.Generic;
using Godot;
using VoidShips.game.registry;
using VoidShips.game.voxel.registry;
using VoidShips.util.polyfill;

namespace VoidShips.game.voxel.mesh;

[Component]
public sealed partial class VoxelMeshWorld : Node
{
	[Export] public Material? MeshMaterial;

	private long _updateGeneration;
	
	private BlockRegistry? _blockRegistry;
	private readonly RegistryCache<BlockDescriptorVisual> _visualDescriptorCache = new();

	public override void _Ready()
	{
		_blockRegistry = this.Component<BlockRegistry>();
	}

	public void UpdateMeshes(IEnumerable<VoxelDataChunk> chunks)
	{
		_updateGeneration += 1;

		foreach (var mainChunk in chunks)
		foreach (var otherChunk in mainChunk.NeighborsAndSelf())
		{
			var otherMesh = otherChunk.Component<VoxelMeshChunk>();
			if (otherMesh.UpdateGeneration == _updateGeneration) continue;
			
			otherMesh.UpdateMesh(this);
			otherMesh.UpdateGeneration = _updateGeneration;
		}
	}

	public bool IsFullyOpaqueMaterial(short id)
	{
		return id != 0 && _visualDescriptorCache.Lookup(_blockRegistry!, id).IsFullyOpaque();
	}

	public bool IsVisibleMaterial(short id)
	{
		return id != 0 && _visualDescriptorCache.Lookup(_blockRegistry!, id).IsVisible();
	}
}
