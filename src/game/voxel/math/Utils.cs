using System;
using System.Collections.Generic;
using Godot;

namespace VoidShips.game.voxel.math;

// === Rust Polyfills === //

public static class EuclidDivisionUtils
{
    public static int DivEuclid(this int lhs, int rhs) {
        var q = lhs / rhs;
        if (lhs % rhs < 0)
            return rhs > 0 ? q - 1 : q + 1;
        
        return q;
    }

    public static int RemEuclid(this int lhs, int rhs)
    {
        var r = lhs % rhs;
        if (r < 0)
            return r + Math.Abs(rhs);
        
        return r;
    }
}

// === Sign === //

public enum Sign
{
    Positive,
    Negative,
}

public static class SignExt
{
    public static bool IsNegative(this Sign sign)
    {
        return sign == Sign.Negative;
    }
    
    public static Sign? CheckedSign(this int value)
    {
        return value == 0 ? null : value < 0 ? Sign.Negative : Sign.Positive;
    }
    
    public static Sign? CheckedSign(this float value)
    {
        return value == 0 ? null : value < 0 ? Sign.Negative : Sign.Positive;
    }
    
    public static Sign BiasedSign(this int value)
    {
        return value < 0 ? Sign.Negative : Sign.Positive;
    }
    
    public static Sign BiasedSign(this float value)
    {
        return value < 0 ? Sign.Negative : Sign.Positive;
    }

    public static Sign Flip(this Sign sign)
    {
        return sign.IsNegative() ? Sign.Positive : Sign.Negative;
    }
    
    public static Sign Multiply(this Sign sign, Sign other)
    {
        return (Sign)((int)sign ^ (int)other);
    }
    
    public static int Multiply(this Sign sign, int other)
    {
        return sign.IsNegative() ? -other : other;
    }
    
    public static float Multiply(this Sign sign, float other)
    {
        return sign.IsNegative() ? -other : other;
    }

    public static short AsUnitShort(this Sign sign)
    {
        return sign.IsNegative() ? (short) -1 : (short) 1;
    }
    
    public static int AsUnitInt(this Sign sign)
    {
        return sign.IsNegative() ? -1 : 1;
    }
    
    public static float AsUnitFloat(this Sign sign)
    {
        return sign.IsNegative() ? -1F : 1F;
    }
}

// === Axis2 === //

public enum Axis2
{
    X,
    Y,
}

public static class Axis2Ext
{
    public static Vector2I AsUnitI(this Axis2 axis)
    {
        return axis == Axis2.X ? Vector2I.Right : Vector2I.Down;
    }
    
    public static Vector2 AsUnitF(this Axis2 axis)
    {
        return axis == Axis2.X ? Vector2.Right : Vector2.Down;
    }
}

// === Axis3 === //

public enum Axis3
{
    X,
    Y,
    Z
}

public static class Axis3Ext
{
    public static IEnumerable<Axis3> Variants()
    {
        yield return Axis3.X;
        yield return Axis3.Y;
        yield return Axis3.Z;
    } 
    
    public static Vector3I AsUnitI(this Axis3 axis)
    {
        switch (axis)
        {
            case Axis3.X: return new Vector3I(1, 0, 0);
            case Axis3.Y: return new Vector3I(0, 1, 0);
            default: return new Vector3I(0, 0, 1);
        }
    }
    
    public static Vector3 AsUnitF(this Axis3 axis)
    {
        switch (axis)
        {
            case Axis3.X: return new Vector3(1, 0, 0);
            case Axis3.Y: return new Vector3(0, 1, 0);
            default: return new Vector3(0, 0, 1);
        }
    }

    public static (Axis3, Axis3) OrthoHv(this Axis3 axis)
    {
        switch (axis)
        {
            case Axis3.X: return (Axis3.Z, Axis3.Y);
            case Axis3.Y: return (Axis3.X, Axis3.Z);
            default: return (Axis3.X, Axis3.Y);
        }
    }

    public static Vector3 ExtrudeVolumeF(this Axis3 axis, Vector2 rect, float perp)
    {
        var (ha, va) = axis.OrthoHv();
        var (hm, vm) = rect;
        var target = Vector3.Zero;
        target[(int) ha] = hm;
        target[(int) va] = vm;
        target[(int)axis] = perp;
        return target;
    }
    
    public static Vector3I ExtrudeVolumeI(this Axis3 axis, Vector2I rect, int perp)
    {
        var (ha, va) = axis.OrthoHv();
        var (hm, vm) = rect;
        var target = Vector3I.Zero;
        target[(int) ha] = hm;
        target[(int) va] = vm;
        target[(int)axis] = perp;
        return target;
    }

    public static Transform3D Rotate180Transform(this Axis3 axis)
    {
        return axis switch
        {
            Axis3.X => Transform3D.Identity.Rotated(new Vector3(0, 1, 0), Mathf.Pi),
            Axis3.Y => Transform3D.Identity.Rotated(new Vector3(0, 0, 1), Mathf.Pi),
            _ => Transform3D.Identity.Rotated(new Vector3(0, 1, 0), Mathf.Pi)
        };
    }
}

// === BlockFace === //

public enum BlockFace
{
    PosX,
    NegX,
    PosY,
    NegY,
    PosZ,
    NegZ
}

public static class BlockFaceExt
{
    public const int VariantCount = 6;
    public const BlockFace Top = BlockFace.PosY;
    public const BlockFace Bottom = BlockFace.NegY;

    public static readonly BlockFace[] Sides = {
        BlockFace.PosX,
        BlockFace.NegZ,
        BlockFace.NegX,
        BlockFace.PosZ,
    };

    public static BlockFace? BlockFaceFromVec(this Vector3I vec)
    {
        BlockFace? choice = null;
        
        foreach (var axis in Axis3Ext.Variants()) {
            var comp = vec[(int) axis];

            if (comp != 0 && choice != null) {
                return null;
            }

            choice = comp switch
            {
                1 => Compose(axis, Sign.Positive),
                -1 => Compose(axis, Sign.Negative),
                _ => choice
            };
        }

        return choice;
    }

    public static BlockFace Compose(Axis3 axis, Sign sign)
    {
        return (BlockFace)(((int)axis << 1) + (int) sign);
    }

    public static (Axis3, Sign) Decompose(this BlockFace face)
    {
        return (face.Axis(), face.GetSign());
    }

    public static Axis3 Axis(this BlockFace face)
    {
        return (Axis3)((int) face / 2);
    }
    
    public static Sign GetSign(this BlockFace face)
    {
        return face.IsSignNegative() ? Sign.Negative : Sign.Positive;
    }
    
    public static bool IsSignNegative(this BlockFace face)
    {
        return (int)face % 2 == 1;
    }
    
    public static BlockFace Inverse(this BlockFace face)
    {
        return (BlockFace)((int) face ^ 1);
    }

    public static IEnumerable<BlockFace> BlockFaces()
    {
        for (var i = 0; i < VariantCount; i++)
            yield return (BlockFace) i;
    }
    
    public static Vector3I UnitVectorI(this BlockFace face)
    {
        var (axis, sign) = face.Decompose();
        var target = Vector3I.Zero;
        target[(int)axis] = sign.AsUnitInt();
        return target;
    }
    
    public static Vector3 UnitVectorF(this BlockFace face)
    {
        var (axis, sign) = face.Decompose();
        var target = Vector3.Zero;
        target[(int)axis] = sign.AsUnitFloat();
        return target;
    }
    
    public static Vector2 FlattenHv(this Vector3 vec, Axis3 axisToFlatten)
    {
        var (h, v) = axisToFlatten.OrthoHv();
        return new Vector2(vec[(int)h], vec[(int)v]);
    }

    public static Vector3 Deepen(this Vector2 flat, Axis3 axisToDeepen, float depth)
    {
        var (h, v) = axisToDeepen.OrthoHv();
        var target = Vector3.Zero;
        target[(int)h] = flat.X;
        target[(int)v] = flat.Y;
        target[(int)axisToDeepen] = depth;
        return target;
    }
}
