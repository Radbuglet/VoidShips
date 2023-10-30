using System;
using Godot;
using VoidShips.game.inventory.descriptor;
using VoidShips.util.polyfill;

namespace VoidShips.game.inventory;

[Component]
public sealed partial class InventoryData : Node
{
	[Export] public int InventorySize;
	
	private Node?[]? _stacks;
	
	public void AddStack(Node stack)
	{
		_stacks ??= new Node?[InventorySize];
		
		var stackBase = stack.Component<ItemStackBase>();
		var maxStacking = stackBase.Material!.Component<ItemDescriptorBase>().MaxAutomaticStacking;

		for (var i = 0; i < _stacks.Length; i++)
		{
			var otherStack = _stacks[i];
			
			// If there's a stack here already, see if we can insert ourselves into it.
			if (otherStack != null)
			{
				var otherStackBase = otherStack.Component<ItemStackBase>();
				if (stackBase.Material != otherStackBase.Material)
					continue;

				var transfer = Math.Min(maxStacking - otherStackBase.Count, stackBase.Count);
				otherStackBase.Count += transfer;
				stackBase.Count -= transfer;

				if (stackBase.Count == 0)
					stackBase.QueueFree();
			}
			else
			{
				// Otherwise, insert the entire stack directly.
				_stacks[i] = stack;
				AddChild(stack);
			}
		}
	}
}
