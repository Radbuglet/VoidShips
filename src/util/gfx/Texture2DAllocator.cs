using System;
using System.Collections.Generic;
using Godot;
using VoidShips.util.polyfill;

namespace VoidShips.util.gfx;

// TODO: Implement array shrinking
[Component]
public sealed partial class Texture2DAllocator : Node
{
    public Texture2DAllocatorConfig LayerConfig;
    
    private Texture2DArray? _gpuTextureArray;
    private readonly List<int> _freeSlots = new();

    public override void _Ready()
    {
        var images = new Godot.Collections.Array<Image>();
        for (var i = 0; i < 8; i++)
        {
            images.Add(LayerConfig.CreateImage());
            _freeSlots.Add(i);
        }

        _gpuTextureArray = new Texture2DArray();
        _gpuTextureArray.CreateFromImages(images);
    }

    public int AllocImage(Image image)
    {
        if (_freeSlots.Pop(out var freeSlot))
        {
            _gpuTextureArray!.UpdateLayer(image, freeSlot);
            return freeSlot;
        }

        var images = new Godot.Collections.Array<Image>(_gpuTextureArray!._Images);
        var insertIndex = images.Count;
        var addCount = Math.Max(1, images.Count);
        for (var i = 0; i < addCount; i++)
        {
            images.Add(i == 0 ? image : LayerConfig.CreateImage());
            if (i > 0) _freeSlots.Add(insertIndex + i);
        }

        _gpuTextureArray.CreateFromImages(images);
        return insertIndex;
    }

    public void UnloadImage(int index)
    {
        _freeSlots.Add(index);
    }
}

public struct Texture2DAllocatorConfig
{
    [Export] public Vector2I ImageSize = Vector2I.One * 16;
    [Export] public bool UseMipMaps = true;
    [Export] public Image.Format Format = Image.Format.Rgb8;

    public Texture2DAllocatorConfig()
    {
    }
    
    public Texture2DAllocatorConfig(Image image)
    {
        ImageSize = image.GetSize();
        UseMipMaps = image.HasMipmaps();
        Format = image.GetFormat();
    }

    public Image CreateImage()
    {
        return Image.Create(ImageSize.X, ImageSize.Y, UseMipMaps, Format);
    }
}
