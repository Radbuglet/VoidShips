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

public readonly struct AaQuad3
{
    public readonly AaPlane3 Plane;
    public readonly Rect2 Rect;

    public float Origin => Plane.Origin;
    public BlockFace Normal => Plane.Normal;

    public AaQuad3(AaPlane3 plane, Rect2 rect)
    {
        Plane = plane;
        Rect = rect;
    }

    public AaIntersection? Intersection(Segment3D segment)
    {
        var intersection = Plane.Intersection(segment);
        return Rect.HasPoint(intersection.Pos.FlattenHv(Normal.Axis())) ? intersection : null;
    }
}

// === AabbN === //

public static class VoxelAabb
{
    public static Rect2 RectFromCorners(Vector2 min, Vector2 max)
    {
        return new Rect2(min, max - min);
    }
}
