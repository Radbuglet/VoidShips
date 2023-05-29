using Godot;
using VoidShips.Util;

[ComponentTy]
public partial class MyTestComponent : Node
{
	public void DoSomething()
	{
		GD.Print("Whee!");
	}
}
