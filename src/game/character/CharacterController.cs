using System.Linq;
using Godot;
using VoidShips.constants;
using VoidShips.game.voxel;
using VoidShips.game.voxel.loader;
using VoidShips.game.voxel.math;
using VoidShips.util;

namespace VoidShips.game.character;

[Component]
public sealed partial class CharacterController : Node
{
	private VoxelDataWorld? _world;
	private VoxelLoaderImmediate? _loader;
	private CharacterMotor? _motor;
	private CameraController? _camera;

	private bool _isAttached;

	public override void _Ready()
	{
		_world = this.ParentGameObject<Node>().Component<VoxelDataWorld>();
		_loader = this.ParentGameObject<Node>().Component<VoxelLoaderImmediate>();
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
			var newVel = _motor!.Velocity;
			newVel.Y = GamePhysicsConf.PlayerJumpImpulse;
			_motor!.Velocity = newVel;
		}

		var heading = Vector3.Zero;

		if (GameInputs.FpsLeft.Pressed) heading += Vector3.Left;
		if (GameInputs.FpsRight.Pressed) heading += Vector3.Right;
		if (GameInputs.FpsForward.Pressed) heading += Vector3.Forward;
		if (GameInputs.FpsBackward.Pressed) heading += Vector3.Back;

		heading = heading.Normalized();

		_motor!.Velocity += _camera!.RotatedHorizontally(
			heading * GamePhysicsConf.PlayerHorizontalAccel * fDelta);

		// Raycast demo
		var pointer = GetNode<Node3D>("../Pointer"); 
		pointer.Visible = false;
		
		var ray = new VoxelRaycast(
			_world!,
			_camera.GlobalPosition,
			_camera.GlobalPosition + _camera.Rotated(Vector3.Forward) * 7);
		
		while (ray.Distance < ray.MaxDistance)
		{
			var collisions = ray.StepRaw();
		
			foreach (var collision in collisions.Where(collision => collision.Pointer.GetData() != 0))
			{
				if (GameInputs.FpsPlace.JustPressed)
				{
					var block = collision.Pointer.Neighbor(collision.EntryFace.Inverse());
					_loader!.LoadChunk(block);
					block.SetData(1);
				}
				if (GameInputs.FpsBreak.JustPressed)
					collision.Pointer.SetData(0);
				
				pointer.GlobalPosition = collision.Pointer.Pos + Vector3.One * 0.5f;
				pointer.Visible = true;
				goto end;
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
