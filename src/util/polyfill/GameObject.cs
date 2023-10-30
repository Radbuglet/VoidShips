using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace VoidShips.util.polyfill;

public sealed partial class GameObject : RefCounted
{
	internal readonly Dictionary<Type, Node> Comps = new();
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class Component : Attribute
{
	internal static bool HasCompAttr(Type ty)
	{
		return GetCustomAttribute(ty, typeof(Component)) != null;
	}
}

public static class GameObjectExt
{
	private const string GameObjectGroup = "game_object";
	private const string CompMapMetaKey = "gobj_comp_map";

	public static bool IsGameObject(this Node target)
	{
		return target.IsInGroup(GameObjectGroup);
	}

	public static T? TryGameObject<T>(this Node target) where T : class
	{
		var iter = target;

		while (iter != null)
		{
			if (iter.IsGameObject())
				return (T)(object)iter;

			iter = iter.GetParentOrNull<Node>();
		}

		return null;
	}

	public static T ParentGameObject<T>(this Node target) where T : class
	{
		return target.GameObject<Node>().GetParent().GameObject<T>();
	}

	public static T GameObject<T>(this Node target) where T : class
	{
		var gObj = target.TryGameObject<T>();
		Debug.Assert(gObj != null, $"{target.StringifyNode()} does not have an ancestor game object node.");
		return gObj!;
	}

	public static T? TryComponent<T>(this Node target) where T : class
	{
		// Ensure that this type is actually a component.
		Debug.Assert(polyfill.Component.HasCompAttr(typeof(T)), $"Attempted to fetch a component of type {typeof(T)} from {target.StringifyNode()}, despite the fact that {typeof(T)} lacks the {nameof(polyfill.Component)} attribute.");

		// Get the game object's metadata.
		var gobj = target.GameObject<Node>();
		GameObject meta;

		if (gobj.HasMeta(CompMapMetaKey))
		{
			meta = (GameObject)gobj.GetMeta(CompMapMetaKey);
		}
		else
		{
			meta = new GameObject();
			gobj.SetMeta(CompMapMetaKey, meta);
		}

		// Try to fast-path the fetch.
		if (meta.Comps.TryGetValue(typeof(T), out var comp))
			return (T)(object)comp;

		// Otherwise, re-scan all descendants to find new components. This is slow but should only
		// ever happen once in well behaved code that respects archetypes.
		foreach (var descendant in gobj.Descendants(node => node == gobj || !node.IsGameObject()))
		{
			var ty = descendant.GetType();

			void AddComponentToMapIfApplicable(
				Type ty,
				Node instance
			)
			{
				// Ensure that the component is applicable
				if (!polyfill.Component.HasCompAttr(ty))
					return;

				// Ensure that it's not a duplicate
				if (meta.Comps.TryGetValue(ty, out var existing))
				{
					if (existing != instance)
						Debug.Fail($"Conflicting components on {gobj.StringifyNode()}: {instance.StringifyNode()} is trying to replace {existing.StringifyNode()} for the component type {ty}.");

					return;
				}

				// Add it to the map
				meta.Comps.Add(ty, instance);
			}

			foreach (var iface in ty.GetInterfaces())
			{
				AddComponentToMapIfApplicable(iface, descendant);
			}

			var tyIter = ty;

			while (tyIter != null)
			{
				AddComponentToMapIfApplicable(tyIter, descendant);
				tyIter = tyIter.BaseType;
			}
		}

		return meta.Comps.TryGetValue(typeof(T), out comp) ? (T)(object)comp : null;
	}

	public static T Component<T>(this Node target) where T : class
	{
		var comp = target.TryComponent<T>();
		Debug.Assert(comp != null, $"{target.StringifyNode()} does not have component of type {typeof(T)}.");
		return comp!;
	}

	public static T MakeGameObject<T>(this T node) where T : Node 
	{
		node.AddToGroup(GameObjectGroup);
		return node;
	}
}
