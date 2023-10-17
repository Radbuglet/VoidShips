using VoidShips.actors.voxel;

namespace VoidShips.Actors.Character;

using Godot;
using VoidShips.Util;
using VoidShips.Constants;

[Component]
public sealed partial class CharacterController : Node
{
	private VoxelDataWorld? _world;
	private CharacterBody3D? _body;
	private CharacterMotor? _motor;
	private CameraController? _camera;

	private bool _isAttached;

	public override void _Ready()
	{
		_world = this.ParentGameObject<Node>().Component<VoxelDataWorld>();
		_body = this.GameObject<CharacterBody3D>();
		_motor = this.Component<CharacterMotor>();
		_camera = this.Component<CameraController>();
	}

	public override void _Process(double delta)
	{
		var fDelta = (float)delta;
		
		// Update attach state
		if (GameInputs.FpsEscape.JustPressed)
		{
			_isAttached = !_isAttached;
		}
		
		Input.MouseMode = _isAttached ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;

		if (!_isAttached) return;

		// Handle movement
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

		// Raycast demo
		var pointer = GetNode<Node3D>("../Pointer"); 
		pointer.GlobalPosition = _camera.GlobalPosition + _camera.Rotated(Vector3.Forward) * 7;
		
		var ray = new VoxelRaycast(
			_world!,
			_camera.GlobalPosition,
			_camera.GlobalPosition + _camera.Rotated(Vector3.Forward) * 7);

		while (true)
		{
			var collisions = ray.StepRaw();
			if (ray.Distance >= ray.MaxDistance) break;

			foreach (var collision in collisions)
			{
				if (collision.Pointer.GetData() != 0)
				{
					// collision.Pointer.Neighbor(collision.EntryFace.Inverse()).SetData(1);
					pointer.GlobalPosition = collision.Position;
					goto end;
				}
			}
		}
		
		end: {}
	}

	public override void _Input(InputEvent rawEv)
	{
		if (!_isAttached) return;

		if (rawEv is not InputEventMouseMotion ev)
			return;

		_camera!.Orientation += ev.Relative * (Mathf.Pi / 180f) * -0.1f;
	}
}
