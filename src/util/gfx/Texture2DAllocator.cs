using System;
using System.Collections.Generic;
using Godot;
using VoidShips.util.mem;
using VoidShips.util.polyfill;

namespace VoidShips.util.gfx;

[Component]
public sealed partial class Texture2DAllocator : Node
{
    private const int MinLayers = 8;

    public Texture2DAllocatorConfig LayerConfig;
    private Texture2DArray? _gpuTextureArray;
    private readonly BitVector _reservedLayers = new();

    public override void _Ready()
    {
        var images = new Godot.Collections.Array<Image>();
        for (var i = 0; i < MinLayers; i++)
            images.Add(LayerConfig.CreateImage());

        _gpuTextureArray = new Texture2DArray();
        _gpuTextureArray.CreateFromImages(images);
    }

    public int Allocate(Image image)
    {
        // Find the smallest free layer index.
        var index = _reservedLayers.AddSmallest();
        
        // If we have enough layers in our texture array, reuse the layer.
        var layerCount = _gpuTextureArray!.GetLayers();
        if (index < layerCount)
        {
            _gpuTextureArray.UpdateLayer(image, index);
            return index;
        }

        // Otherwise, double the number of layers, uploading our new image in the process.
        var images = _gpuTextureArray!._Images;  // N.B. this clones the array already
        images.EnsureMinLength(layerCount * 2, i => i == index ? image : LayerConfig.CreateImage());
        _gpuTextureArray.CreateFromImages(images).AssertOk();
        
        return index;
    }

    public void Deallocate(int index)
    {
        _reservedLayers.Remove(index);
        
        // Truncate the array buffer
        var smallestLength = _reservedLayers.SmallestLength();
        var capacity = _gpuTextureArray!.GetLayers();

        while (smallestLength < capacity / 2)
            capacity /= 2;
        
        var images = new Godot.Collections.Array<Image>(_gpuTextureArray!._Images);
        images.Truncate(capacity);
        _gpuTextureArray.CreateFromImages(images).AssertOk();
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
