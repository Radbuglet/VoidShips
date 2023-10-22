using Godot;
using VoidShips.game.voxel;
using VoidShips.game.voxel.loader;
using VoidShips.Util;

namespace VoidShips.game.character;

[Component]
public sealed partial class DebugHud : RichTextLabel
{
	public override void _Process(double delta)
	{
		var motor = this.Component<CharacterMotor>();
		var world = this.ParentGameObject<Node>().Component<VoxelDataWorld>();

		Text = $"VoidShips Debug Build\n"
			+ $"[color=gray]Velocity:[/color] {FormatVector(motor.Velocity)}\n"
			+ $"[color=gray]Horizontal Speed:[/color] {new Vector2(motor.Velocity.X, motor.Velocity.Z).Length()}\n"
			+ $"[color=gray]Chunk count:[/color] {world.ChunkCount}\n"
			+ $"[color=gray]FPS:[/color] {Engine.GetFramesPerSecond()}\n";
	}

	private static string FormatVector(Vector3 vec)
	{
		const string sep = "[color=gray], [/color]";
		return $"[color=red]{vec.X}[/color]{sep}[color=green]{vec.Y}[/color]{sep}[color=blue]{vec.Z}[/color]";
	}
}
