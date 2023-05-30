namespace VoidShips.Actors.Character;

using Godot;
using VoidShips.Util;

[Component]
public sealed partial class CharacterMotor : Node
{
    [Export] public Vector3 Velocity;

    public override void _PhysicsProcess(double delta)
    {
        var fDelta = (float)delta;
        var whee = this.GameObject<PhysicsBody3D>();
        Velocity += Vector3.Down * 9.8f * fDelta;
        whee.MoveAndCollide(Velocity * fDelta);
    }
}
