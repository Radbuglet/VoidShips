namespace VoidShips.Actors.Character;

using Godot;
using VoidShips.Util;

[Component]
public sealed partial class DebugHud : RichTextLabel
{
	public override void _Process(double delta)
	{
		var body = this.GameObject<CharacterBody3D>();

		Text = $"VoidShips Debug Build\n"
			+ $"[color=gray]Velocity:[/color] {FormatVector(body.Velocity)}\n"
			+ $"[color=gray]Horizontal Speed:[/color] {new Vector2(body.Velocity.X, body.Velocity.Z).Length()}\n"
			+ $"[color=gray]FPS:[/color] {Engine.GetFramesPerSecond()}\n";
	}

	private static string FormatVector(Vector3 vec)
	{
		const string sep = "[color=gray], [/color]";
		return $"[color=red]{vec.X}[/color]{sep}[color=green]{vec.Y}[/color]{sep}[color=blue]{vec.Z}[/color]";
	}
}
