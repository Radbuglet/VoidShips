namespace VoidShips.Util;

using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public static class NodeUtil
{
    public static string StringifyNode(this Node node)
    {
        return $"<{node.GetType()} @ {node.GetPath()}: {node}>";
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
