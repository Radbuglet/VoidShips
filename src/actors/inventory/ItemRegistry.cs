using System.Collections.Generic;
using Godot;
using VoidShips.Actors.Inventory.descriptor;
using VoidShips.Util;

namespace VoidShips.Actors.Inventory;

[Component]
public sealed partial class ItemRegistry : Node
{
	private readonly Dictionary<string, Node> _kinds = new();
	
	public void Register(Node kind)
	{
		_kinds.Add(kind.Component<ItemDescriptorBase>().Id!, kind);
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

	public Node BuildDefault(string id)
	{
		return Lookup(id)!.Component<ItemDescriptorBase>().BuildDefault();
	}
}
