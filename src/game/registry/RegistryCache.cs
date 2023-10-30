using System.Collections.Generic;
using VoidShips.util.polyfill;

namespace VoidShips.game.registry;

public sealed class RegistryCache<T> where T : class
{
    private readonly List<T?> _cached = new();

    public T Lookup(AbstractRegistry registry, short id)
    {
        _cached.EnsureMinLength(id + 1, _ => null);
        return _cached[id] ?? (_cached[id] = registry.Lookup(id).Component<T>());
    }
}
