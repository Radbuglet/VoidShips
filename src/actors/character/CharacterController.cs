namespace VoidShips.Actors.Character;

using Godot;
using VoidShips.Util;
using VoidShips.Constants;

[Component]
public sealed partial class CharacterController : Node
{
	private CharacterBody3D? _body;
	private CharacterMotor? _motor;
	private CameraController? _camera;

	public override void _Ready()
	{
		_body = this.GameObject<CharacterBody3D>();
		_motor = this.Component<CharacterMotor>();
		_camera = this.Component<CameraController>();

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Process(double delta)
	{
		var fDelta = (float)delta;

		if (GameInputs.FpsJump.JustPressed && _motor!.IsOnGround)
		{
			var newVel = _body!.Velocity;
			newVel.Y = GamePhysicsConf.PlayerJumpImpulse;
			_body!.Velocity = newVel;
		}

		var heading = Vector3.Zero;

		if (GameInputs.FpsLeft.Pressed) heading += Vector3.Left;
		if (GameInputs.FpsRight.Pressed) heading += Vector3.Right;
		if (GameInputs.FpsForward.Pressed) heading += Vector3.Forward;
		if (GameInputs.FpsBackward.Pressed) heading += Vector3.Back;

		heading = heading.Normalized();

		_body!.Velocity += _camera!.RotatedHorizontally(
			heading * GamePhysicsConf.PlayerHorizontalAccel * fDelta);
	}

	public override void _Input(InputEvent rawEv)
	{
		if (!(rawEv is InputEventMouseMotion ev))
			return;

		_camera!.Orientation += ev.Relative * (Mathf.Pi / 180f) * -0.1f;
	}
}
