using Godot;
using VoidShips.util.mem;
using VoidShips.util.polyfill;

namespace VoidShips.util.gfx;

public sealed partial class Texture2DAllocator : Node
{
    private const int MinFrames = 8;

    public Texture2DAllocatorConfig FrameConfig;
    private Texture2DArray? _gpuTextureArray;
    private readonly BitVector _reservedFrames = new();

    public Texture2DArray TextureArray => _gpuTextureArray!;

    public override void _Ready()
    {
        var images = new Godot.Collections.Array<Image>();
        for (var i = 0; i < MinFrames; i++)
            images.Add(FrameConfig.CreateImage());

        _gpuTextureArray = new Texture2DArray();
        _gpuTextureArray.CreateFromImages(images).AssertOk();
    }

    public int Allocate(Image image)
    {
        // Find the smallest free frame index.
        var index = _reservedFrames.AddSmallest();
        
        // If we have enough frames in our texture array, reuse the frame.
        var frameCount = _gpuTextureArray!.GetLayers();
        if (index < frameCount)
        {
            _gpuTextureArray.UpdateLayer(image, index);
            return index;
        }

        // Otherwise, double the number of frames, uploading our new image in the process.
        var images = _gpuTextureArray!._Images;  // N.B. this clones the array already
        images.EnsureMinLength(frameCount * 2, i => i == index ? image : FrameConfig.CreateImage());
        _gpuTextureArray.CreateFromImages(images).AssertOk();
        
        return index;
    }

    public void Deallocate(int index)
    {
        _reservedFrames.Remove(index);
        
        // Truncate the array buffer
        var smallestLength = _reservedFrames.SmallestLength();
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
