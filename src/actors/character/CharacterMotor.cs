namespace VoidShips.Actors.Character;

using Godot;
using VoidShips.Util;
using VoidShips.Constants;

[Component]
public sealed partial class CharacterMotor : Node
{
	private CharacterBody3D Body => this.GameObject<CharacterBody3D>();

	public bool IsVerticallyImpeded { get; private set; }
	public bool IsOnGround => IsVerticallyImpeded && Body.Velocity.Y < 0;
	public bool IsOnCeiling => IsVerticallyImpeded && Body.Velocity.Y > 0;

	public override void _PhysicsProcess(double delta)
	{
		var fDelta = (float)delta;
		var target = Body;

		// Determine whether we're on the ground or the ceiling
		IsVerticallyImpeded = target.MoveAndCollide(
			Mathf.Sign(target.Velocity.Y) * Vector3.Up * GamePhysicsConf.SmallDistance,
			testOnly: true
		) != null;

		// Commit the actual physics step so we actually snap to the ground or ceiling
		var collisions = target.MoveAndSlide();

		// Accelerate the player
		if (IsVerticallyImpeded)
			target.Velocity = new Vector3(target.Velocity.X, 0, target.Velocity.Z);

		if (!IsOnGround)
			target.Velocity += Vector3.Down * GamePhysicsConf.GravitationalAccel * fDelta;

		target.Velocity *= IsOnGround ?
			Mathf.Pow(GamePhysicsConf.PlayerDragCoefGround, fDelta) :
			Mathf.Pow(GamePhysicsConf.PlayerDragCoefAir, fDelta);
	}
}
