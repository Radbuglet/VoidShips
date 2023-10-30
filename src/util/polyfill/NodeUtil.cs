using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace VoidShips.util.polyfill;

public static class NodeUtil
{
	public static string StringifyNode(this Node? node)
	{
		return node == null ? "<null node>" : $"<{node.GetType()} @ {(node.IsInsideTree() ? node.GetPath() : "<not in tree>")}: {node}>";
	}

	public static IEnumerable<Node> Descendants(this Node node, Func<Node, bool> filter)
	{
		return filter(node) ?
				node.GetChildren()
				.SelectMany(node => node.Descendants(filter))
				.Prepend(node)
			: Enumerable.Empty<Node>();
	}
}
