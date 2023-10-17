namespace VoidShips.Actors.Character;

using Godot;
using VoidShips.Util;

[Component]
public sealed partial class CameraController : Camera3D
{
	private Vector2 _orientation;

	public Vector2 Orientation
	{
		get => _orientation;
		set =>
			_orientation = new Vector2(
				value.X % Mathf.Tau,
				Mathf.Clamp(value.Y, -Mathf.Pi / 2, Mathf.Pi / 2)
			);
	}

	public Vector3 RotatedHorizontally(Vector3 relative)
	{
		return relative.Rotated(Vector3.Up, Orientation.X);
	}

	public Vector3 Rotated(Vector3 relative)
	{
		return relative.Rotated(Vector3.Right, Orientation.Y).Rotated(Vector3.Up, Orientation.X);
	}

	public override void _Process(double delta)
	{
		Basis = Basis.Identity
			.Rotated(Vector3.Right, Orientation.Y)
			.Rotated(Vector3.Up, Orientation.X);
	}
}
