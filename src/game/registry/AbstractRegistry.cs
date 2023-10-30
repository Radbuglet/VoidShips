using System.Collections.Generic;
using Godot;
using VoidShips.util.polyfill;

namespace VoidShips.game.registry;

public abstract partial class AbstractRegistry : Node
{
	private readonly Dictionary<string, Node> _kinds = new();
	private readonly List<Node> _indices = new();
	
	public void Register(Node kind)
	{
		var descriptor = kind.Component<AbstractBaseDescriptor>();
		descriptor.Index = (short) _indices.Count;
		_kinds.Add(descriptor.Id!, kind);
		_indices.Add(kind);
	}

	public void RegisterChildren()
	{
		foreach (var child in GetChildren())
			Register(child);
	}

	public Node? Lookup(string id)
	{
		return _kinds.TryGetValue(id, out var v) ? v : null;
	}
	
	public Node Lookup(short id)
	{
		return _indices[id];
	}
}
