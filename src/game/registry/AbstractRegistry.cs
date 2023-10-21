using System.Collections.Generic;
using Godot;
using VoidShips.Util;

namespace VoidShips.game.registry;

[Component]
public abstract partial class AbstractRegistry<T> : Node where T : Node
{
	private readonly Dictionary<string, T> _kinds = new();
	private readonly List<T> _indices = new();
	
	public void Register(T kind)
	{
		var descriptor = kind.Component<AbstractBaseDescriptor>();
		descriptor.Index = (short) _indices.Count;
		_kinds.Add(descriptor.Id!, kind);
		_indices.Add(kind);
	}

	public void RegisterChildren()
	{
		foreach (var child in GetChildren())
			Register(child.Component<T>());
	}

	public T? Lookup(string id)
	{
		return _kinds.TryGetValue(id, out var v) ? v : null;
	}
	
	public T Lookup(short id)
	{
		return _indices[id];
	}
}
