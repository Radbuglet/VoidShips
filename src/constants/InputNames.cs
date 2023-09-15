namespace VoidShips.Constants;

using Godot;

public static class GameInputs
{
	public static readonly InputActionId FpsJump = new("fps_jump");
	public static readonly InputActionId FpsLeft = new("fps_left");
	public static readonly InputActionId FpsRight = new("fps_right");
	public static readonly InputActionId FpsForward = new("fps_forward");
	public static readonly InputActionId FpsBackward = new("fps_backward");
}

public readonly struct InputActionId
{
	public readonly string Id;

	public InputActionId(string id)
	{
		Id = id;
	}

	public bool Pressed => Input.IsActionPressed(Id);
	public bool JustPressed => Input.IsActionJustPressed(Id);
	public bool JustReleased => Input.IsActionJustReleased(Id);
}
