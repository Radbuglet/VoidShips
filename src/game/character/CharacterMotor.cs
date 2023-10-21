using Godot;
using VoidShips.Constants;
using VoidShips.game.voxel;
using VoidShips.game.voxel.math;
using VoidShips.Util;

namespace VoidShips.game.character;

[Component]
public sealed partial class CharacterMotor : Node
{
	private Node3D? _body;
	private VoxelWorldFacade? _world;

	public Vector3 Velocity;

	public bool IsVerticallyImpeded { get; private set; }
	public bool IsOnGround => IsVerticallyImpeded && Velocity.Y < 0;
	public bool IsOnCeiling => IsVerticallyImpeded && Velocity.Y > 0;

	public override void _Ready()
	{
		_body = this.GameObject<Node3D>();
		_world = this.ParentGameObject<Node>().Component<VoxelWorldFacade>();
	}

	public override void _PhysicsProcess(double delta)
	{
		var fDelta = (float)delta;
		
		// Get the character's collider
		var posOffset = new Vector3(0.5f, 0f, 0.5f);
		var rbAabb = new Aabb(_body!.GlobalPosition - posOffset, new Vector3(1f, 2f, 1f));

		// Determine whether we're on the ground or the ceiling
		IsVerticallyImpeded = _world!.CastVolume(
			rbAabb.FaceQuad(BlockFaceExt.Bottom),
			GamePhysicsConf.SmallDistance) < GamePhysicsConf.SmallDistance;

		// Commit the actual physics step so we actually snap to the ground or ceiling
		// var collisions = target.MoveAndSlide();
		var newAabbPos = _world!.MoveRigidBody(
			rbAabb,
			Velocity * (float) delta);
		
		_body!.GlobalPosition = newAabbPos + posOffset;

		// Accelerate the player
		if (IsVerticallyImpeded)
			Velocity = new Vector3(Velocity.X, 0, Velocity.Z);

		if (!IsOnGround)
			Velocity += Vector3.Down * GamePhysicsConf.GravitationalAccel * fDelta;

		Velocity *= IsOnGround ?
			Mathf.Pow(GamePhysicsConf.PlayerDragCoefGround, fDelta) :
			Mathf.Pow(GamePhysicsConf.PlayerDragCoefAir, fDelta);
	}
}
