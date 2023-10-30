using VoidShips.game.registry;
using VoidShips.util;

namespace VoidShips.game.voxel.registry;

[Component]
public sealed partial class BlockRegistry : AbstractRegistry
{
    private readonly RegistryCache<BlockDescriptorBase> _baseCache = new();

    public BlockDescriptorBase LookupBaseDescriptor(short id)
    {
        return _baseCache.Lookup(this, id);
    }
}