using System;

namespace VoidShips.Util;

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
}

