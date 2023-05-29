namespace VoidShips.Util;

using Godot;

public static class NodeUtil
{
	public static string StringifyNode(this Node node)
	{
		return $"<{node.GetType()} @ {node.GetPath()}: {node}>";
	}
}
