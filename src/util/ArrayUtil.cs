using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace VoidShips.util;

public static class ArrayUtil
{
    public static T[] InitArray<T>(int len, Func<int, T> generator)
    {
        var arr = new T[len];
        for (var i = 0; i < len; i++)
            arr[i] = generator(i);
        return arr;
    }
    
    public static T[] InitArray<T>(int len, Func<T> generator)
    {
        return InitArray(len, _ => generator());
    }

    public static bool Pop<T>(this List<T> values, [NotNullWhen(true)] out T? value)
    {
        if (values.Count == 0)
        {
            value = default;
            return false;
        }

        value = values[^1]!;
        values.RemoveAt(values.Count - 1);
        return true;
    }
}

