namespace VoidShips.Util;

using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class GameObject : Node
{
	[Export]
	public Godot.Collections.Array<NodePath> Components = new();

	private readonly Dictionary<Type, Node> ComponentMapCache = new();

	// === Component Collection === //

	public IReadOnlyDictionary<Type, Node> ComponentMap
	{
		get
		{
			if (Engine.IsEditorHint())
			{
				return ComputeComponentMapPure();
			}
			else
			{
				PopulateComponentMap(this, ComponentMapCache, Components, shouldPrintErrors: true);

				return ComponentMapCache;
			}
		}
	}

	public IReadOnlyDictionary<Type, Node> ComputeComponentMapPure()
	{
		return ComputeComponentMapPure(shouldPrintErrors: false);
	}

	private IReadOnlyDictionary<Type, Node> ComputeComponentMapPure(bool shouldPrintErrors)
	{
		if (Components.Count == 0)
		{
			return ComponentMap;
		}
		else
		{
			var componentMapClone = new Dictionary<Type, Node>(ComponentMapCache);
			PopulateComponentMap(this, componentMapClone, Components, shouldPrintErrors);

			return componentMapClone;
		}
	}

	private static void PopulateComponentMap(
		Node gObj,
		Dictionary<Type, Node> componentMap,
		Godot.Collections.Array<NodePath> components,
		bool shouldPrintErrors
	)
	{
		// === Deferred logging system === //

		// N.B. we really don't want to call userland code during the main routine of this function
		// since we leave the component map in quite a dangerous state while doing our work.
#if DEBUG
		List<Func<string>> errorMessages = new();
		void LogError(Func<string> errorCreator)
		{
			errorMessages.Add(errorCreator);
		}
#else
		static void LogError(Action<string> errorCreator)
		{
			// (no-op)
		}
#endif

		// === Main Routine === //

		void AddComponentToMapIfApplicable(
			Type ty,
			Node instance,
			ref bool foundApplicable
		)
		{
			// Ensure that the component is applicable
			if (!ComponentTy.HasAttr(ty))
				return;

			foundApplicable = true;

			// Ensure that it's not a duplicate
			if (componentMap.TryGetValue(ty, out var existing) && existing != instance)
			{
				LogError(() => $"Conflicting components on {gObj.StringifyNode()}: {instance.StringifyNode()} is trying to replace {existing.StringifyNode()} for the component type {ty}.");
			}

			// Add it to the map
			componentMap.Add(ty, instance);
		}

		components.FilterNonReentrant((i, componentPath) =>
		{
			// Validate component
			if (componentPath == null || componentPath.IsEmpty)
			{
				LogError(() => $"Element at index {i} of component list of {gObj.StringifyNode()} is null or empty.");
				return true;
			}

			var component = gObj.GetNodeOrNull<Node>(componentPath);

			if (component == null)
			{
				LogError(() => $"Component at index {i} of component list of {gObj.StringifyNode()} could not be resolved at path <{componentPath}>.");
				return true;
			}

			// Register component references where applicable
			bool foundApplicable = false;
			var ty = component.GetType();

			foreach (var iface in ty.GetInterfaces())
			{
				AddComponentToMapIfApplicable(iface, component, ref foundApplicable);
			}

			var tyIter = ty;

			while (tyIter != null)
			{
				AddComponentToMapIfApplicable(tyIter, component, ref foundApplicable);
				tyIter = tyIter.BaseType;
			}

			if (!foundApplicable)
				LogError(() => $"Component {component.StringifyNode()} at index {i} of component list of {gObj.StringifyNode()} has no applicable component types.");

			return false;
		});

		// === Optional error logging === //

#if DEBUG
		if (shouldPrintErrors)
		{
			foreach (var errorGetter in errorMessages)
			{
				Debug.Fail(errorGetter());
			}
		}
#endif
	}

	// === Querying === //

	public T? TryComponent<T>() where T : Node
	{
		Debug.Assert(ComponentTy.HasAttr(typeof(T)), $"Attempted to fetch a component of type {typeof(T)} from {this.StringifyNode()}, despite the fact that {typeof(T)} lacks the {nameof(ComponentTy)} attribute.");

		return ComponentMap.TryGetValue(typeof(T), out var value) ? (T)value : null;
	}

	public T Component<T>() where T : Node
	{
		var comp = TryComponent<T>();
		Debug.Assert(comp != null, $"{this.StringifyNode()} does not have component of type {typeof(T)}.");
		return comp!;
	}

	// === Debugging === //

	public override string ToString()
	{
		var compList = ComputeComponentMapPure(shouldPrintErrors: false).Select(entry =>
		{
			return $"{entry.Key} => {entry.Value.GetType()} at <{GetPathTo(entry.Value)}>";
		}).ToArray().Join();

		return $"<{nameof(GameObject)}: [{compList}]>";
	}
}

[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Interface)]
public sealed class ComponentTy : System.Attribute
{
	internal static bool HasAttr(Type ty)
	{
		return System.Attribute.GetCustomAttribute(ty, typeof(ComponentTy)) != null;
	}
}

public static class GameObjectExt
{
	public static GameObject? TryGameObject(this Node target)
	{
		var iter = target;

		while (iter != null)
		{
			if (iter is GameObject iterGObj)
				return iterGObj;

			iter = iter.GetParentOrNull<Node>();
		}

		return null;
	}

	public static GameObject GameObject(this Node target)
	{
		var gObj = target.TryGameObject();
		Debug.Assert(gObj != null, $"{target.StringifyNode()} does not have an ancestor {nameof(GameObject)} node.");
		return gObj!;
	}

	public static T? TryComponent<T>(this Node target) where T : Node
	{
		return target.GameObject().TryComponent<T>();
	}

	public static T Component<T>(this Node target) where T : Node
	{
		return target.GameObject().Component<T>();
	}
}
