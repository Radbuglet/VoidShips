using System;
using Godot;

namespace VoidShips.util.polyfill;

public static class ErrorUtil
{
    public static void AssertOk(this Error error)
    {
        if (error != Error.Ok) throw new Exception($"godot error: {error}");
    }
}