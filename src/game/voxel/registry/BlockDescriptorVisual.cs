using Godot;
using VoidShips.util.polyfill;

namespace VoidShips.game.voxel.registry;

[Component]
public sealed partial class BlockDescriptorVisual : Node
{
    [Export] public Texture2D? Texture;

    internal int MeshLayer = int.MaxValue;
    internal int TextureFrameIndex = 0;

    public bool IsFullyOpaque()
    {
        return Texture != null;
    }

    public bool IsVisible()
    {
        return Texture != null;
    }
}
