using Godot;
using VoidShips.Constants;
using VoidShips.game.voxel;
using VoidShips.game.voxel.math;
using VoidShips.Util;

namespace VoidShips.game.character;

[Component]
public sealed partial class CharacterMotor : Node
{
	[Export] public Vector3 ColliderSize = new(0.8F, 1.7F, 0.8F);
	
	private Node3D? _body;
	private VoxelWorldPalletized? _world;

	private bool[] _touching = new bool[BlockFaceExt.VariantCount];
	public Vector3 Velocity;
	
	public bool IsOnGround => IsTouching(BlockFaceExt.Bottom) && Velocity.Y < 0;
	public bool IsOnCeiling => IsTouching(BlockFaceExt.Top) && Velocity.Y > 0;

	public override void _Ready()
	{
		_body = this.GameObject<Node3D>();
		_world = this.ParentGameObject<Node>().Component<VoxelWorldPalletized>();
	}

	public override void _PhysicsProcess(double delta)
	{
		var fDelta = (float)delta;
		
		// Get the character's collider
		var posOffset = new Vector3(ColliderSize.X / 2, 0f, ColliderSize.Z / 2);
		var rbAabb = new Aabb(_body!.GlobalPosition - posOffset, ColliderSize);

		// Determine the surfaces we're touching.
		for (var i = 0; i < BlockFaceExt.VariantCount; i++)
		{
			_touching[i] = _world!.CastVolume(
				rbAabb.FaceQuad((BlockFace)i),
				GamePhysicsConf.SmallDistance) < GamePhysicsConf.SmallDistance;
		}
		
		// Clear velocities moving into a block
		foreach (var axis in Axis3Ext.Variants())
		{
			var face = BlockFaceExt.Compose(axis, Velocity[(int)axis].BiasedSign());
			if (IsTouching(face))
				Velocity[(int)axis] = 0;
		}
		
		// Accelerate the player
		if (!IsOnGround)
			Velocity += Vector3.Down * GamePhysicsConf.GravitationalAccel * fDelta;

		Velocity *= IsOnGround ?
			Mathf.Pow(GamePhysicsConf.PlayerDragCoefGround, fDelta) :
			Mathf.Pow(GamePhysicsConf.PlayerDragCoefAir, fDelta);

		// Commit the actual physics step so we actually snap to the ground or ceiling
		var newAabbPos = _world!.MoveRigidBody(
			rbAabb,
			Velocity * (float) delta);
		
		_body!.GlobalPosition = newAabbPos + posOffset;
	}

	public bool IsTouching(BlockFace face)
	{
		return _touching[(int)face];
	}
}
