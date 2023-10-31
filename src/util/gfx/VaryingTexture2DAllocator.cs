using System.Collections.Generic;
using Godot;
using VoidShips.util.polyfill;

namespace VoidShips.util.gfx;

[Component]
public sealed partial class VaryingTexture2DAllocator : Node
{
    private readonly Dictionary<Texture2D, VaryingTexture2DHandle> _textures = new();
    private readonly Dictionary<Texture2DAllocatorConfig, Texture2DAllocator> _arenas = new();

    public VaryingTexture2DHandle Allocate(Texture2D texture)
    {
        if (_textures.TryGetValue(texture, out var descriptor))
            return descriptor;

        var image = texture.GetImage();
        var config = new Texture2DAllocatorConfig(image);

        if (!_arenas.TryGetValue(config, out var arena))
        {
            arena = new Texture2DAllocator
            {
                Name = "TextureArena",
                FrameConfig = config,
            };
            AddChild(arena);
            _arenas.Add(config, arena);
        }

        descriptor = new VaryingTexture2DHandle
        {
            Arena = arena.TextureArray,
            Frame = arena.Allocate(image)
        };
        _textures.Add(texture, descriptor);

        return descriptor;
    }
}

public struct VaryingTexture2DHandle
{
    public Texture2DArray Arena;
    public int Frame;
}
