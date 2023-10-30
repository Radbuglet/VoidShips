using System.Collections.Generic;
using VoidShips.util;

namespace VoidShips.game.registry;

public sealed class RegistryCache<T> where T : class
{
    private readonly List<T?> _cached = new();

    public T Lookup(AbstractRegistry registry, short id)
    {
        for (var i = _cached.Count; i <= id; i++)
            _cached.Add(null);

        return _cached[id] ?? (_cached[id] = registry.Lookup(id).Component<T>());
    }
}
