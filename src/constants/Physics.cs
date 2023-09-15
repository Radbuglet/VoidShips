namespace VoidShips.Constants;

using Godot;

public static class GamePhysicsConf
{
	// === World parameters === //

	public static readonly float GravitationalAccel = 70f;
	public static readonly float SmallDistance = 0.01f;

	// === Player parameters === //

	public static readonly float PlayerDragCoefGround = 0.05f;
	public static readonly float PlayerDragCoefAir = 0.06f;
	public static readonly float PlayerHorizontalAccel = 50f;
	public static readonly float PlayerJumpImpulse = 40f;
}
