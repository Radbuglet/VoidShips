using Godot;

namespace VoidShips.Util;

public static class MathUtil
{
    public static readonly Vector3[] Faces =
    {
        Vector3.Up,
        Vector3.Down,
        Vector3.Left,
        Vector3.Right,
        Vector3.Forward,
        Vector3.Back
    };
    
    public static readonly Vector3I[] FacesI =
    {
        Vector3I.Up,
        Vector3I.Down,
        Vector3I.Left,
        Vector3I.Right,
        Vector3I.Forward,
        Vector3I.Back
    };
}