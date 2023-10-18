using System.Diagnostics;
using Godot;

namespace VoidShips.game.voxel.math;

// === Segment3D === //

public struct Segment3D
{
    public Vector3 Start;
    public Vector3 End;
    
    public Vector3 Delta => End - Start;
    public float Length => Delta.Length();
    public float LengthSquared => Delta.LengthSquared();

    public Segment3D(Vector3 start, Vector3 end)
    {
        Start = start;
        End = end;
    }

    public static Segment3D FromOriginDelta(Vector3 origin, Vector3 delta)
    {
        return new Segment3D(origin, origin + delta);
    }
    
    public readonly Vector3 Lerp(float weight)
    {
        return Start.Lerp(End, weight);
    }
}

// === AaPlane3 === //

public struct AaPlane3
{
    public float Origin;
    public BlockFace Normal;
    public Axis3 Axis => Normal.Axis();
    public Sign Sign => Normal.GetSign();

    public AaPlane3(float origin, BlockFace normal)
    {
        Origin = origin;
        Normal = normal;
    }
    
    public readonly AaIntersection Intersection(Segment3D segment)
    {
        var axis = Normal.Axis();
        var depthLerp = Mathf.InverseLerp(segment.Start[(int) axis], segment.End[(int) axis], Origin);
        var pos = segment.Lerp(depthLerp);

        return new AaIntersection(pos, depthLerp);
    }
}

public readonly struct AaIntersection
{
    public readonly Vector3 Pos;
    public readonly float Lerp;
    public bool IsValid => Lerp is >= 0 and <= 1;

    public AaIntersection(Vector3 pos, float lerp)
    {
        Pos = pos;
        Lerp = lerp;
    }
}

// === AaQuad3 === //

public struct AaQuad3
{
    public AaPlane3 Plane;
    public Rect2 Rect;

    // Forwards
    public readonly float Origin => Plane.Origin;
    public readonly BlockFace Normal => Plane.Normal;
    public readonly Axis3 Axis => Plane.Axis;
    public readonly Sign Sign => Plane.Sign;
    
    // Getters
    public readonly Vector3 Start => Rect.Position.Deepen(Axis, Origin);
    public readonly Vector3 End => Rect.End.Deepen(Axis, Origin);

    public AaQuad3(AaPlane3 plane, Rect2 rect)
    {
        Plane = plane;
        Rect = rect;
    }

    public readonly AaIntersection? Intersection(Segment3D segment)
    {
        var intersection = Plane.Intersection(segment);
        return Rect.HasPoint(intersection.Pos.FlattenHv(Normal.Axis())) ? intersection : null;
    }

    public readonly Aabb Extrude(float depth)
    {
        Debug.Assert(depth >= 0);

        var position = Start;
        if (Sign.IsNegative()) position[(int)Axis] -= depth;

        return new Aabb(position, Rect.Size.Deepen(Axis, depth));
    }
}

// === AabbN === //

public static class RectExt
{
    public static Rect2 FromCorners(Vector2 min, Vector2 max)
    {
        return new Rect2(min, max - min);
    }
}

public static class AabbExt
{
    public static Aabb FromCorners(Vector3 min, Vector3 max)
    {
        return new Aabb(min, max - min);
    }
    
    public static AaQuad3 FaceQuad(this Aabb aabb, BlockFace face)
    {
        var (axis, sign) = face.Decompose();
        var rect = RectExt.FromCorners(aabb.Position.FlattenHv(axis), aabb.End.FlattenHv(axis));
        var origin = sign.IsNegative() ? aabb.Position[(int)axis] : aabb.End[(int)axis];
        return new AaQuad3(new AaPlane3(origin, face), rect);
    }
}
