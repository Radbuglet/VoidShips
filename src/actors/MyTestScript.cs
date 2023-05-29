using Godot;
using VoidShips.Util;

public partial class MyTestScript : Node
{
	public override void _Ready()
	{
		this.Component<MyTestComponent>().DoSomething();
	}
}
