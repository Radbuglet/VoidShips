namespace VoidShips.Actors.Character;

using Godot;
using VoidShips.Util;

[Component]
public sealed partial class CharacterController : Node
{
    private CharacterMotor? _motor;

    public override void _Ready()
    {
        _motor = this.Component<CharacterMotor>();
    }

    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.Space))
            _motor!.Velocity = Vector3.Up * 10f;
    }
}