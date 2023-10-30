using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace VoidShips.util.polyfill;

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
    
    public static bool Pop<T>(this IList<T> values)
    {
        return values.Pop(out _);
    }

    public static bool Pop<T>(this IList<T> values, [NotNullWhen(true)] out T? value)
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

    public static void EnsureMinLength<T>(this IList<T> list, int len, Func<int, T> generator)
    {
        for (var i = list.Count; i < len; i++)
            list.Add(generator(i));
    }

    public static void Truncate<T>(this IList<T> list, int len)
    {
        while (list.Count > len)
            list.Pop();
    }
}

