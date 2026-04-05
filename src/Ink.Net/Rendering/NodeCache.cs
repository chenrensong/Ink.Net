// -----------------------------------------------------------------------
// <copyright file="NodeCache.cs" company="Ink.Net">
//   Port from Ink (JS) node-cache.ts — Cached layout bounds for rendered nodes
// </copyright>
// -----------------------------------------------------------------------

using System.Runtime.CompilerServices;
using Ink.Net.Dom;

namespace Ink.Net.Rendering;

/// <summary>
/// Cached layout bounds for a rendered node. Used for blit optimization and clearing.
/// </summary>
public readonly record struct CachedLayout(int X, int Y, int Width, int Height, int? Top = null);

/// <summary>
/// Pending clear rectangle for removed children.
/// </summary>
public readonly record struct PendingClear(int X, int Y, int Width, int Height, bool IsAbsolute);

/// <summary>
/// Node layout cache — tracks rendered bounds for blit optimization and clearing.
/// Uses ConditionalWeakTable for automatic cleanup when nodes are GC'd.
/// </summary>
public sealed class NodeCache
{
    private readonly ConditionalWeakTable<DomElement, StrongBox<CachedLayout>> _cache = new();
    private readonly ConditionalWeakTable<DomElement, List<PendingClear>> _pendingClears = new();
    private bool _absoluteNodeRemoved;

    /// <summary>Get cached layout for a node, or null if not cached.</summary>
    public CachedLayout? Get(DomElement node)
    {
        return _cache.TryGetValue(node, out var box) ? box.Value : null;
    }

    /// <summary>Set cached layout for a node.</summary>
    public void Set(DomElement node, CachedLayout layout)
    {
        if (_cache.TryGetValue(node, out var box))
            box.Value = layout;
        else
            _cache.AddOrUpdate(node, new StrongBox<CachedLayout>(layout));
    }

    /// <summary>Remove cached layout for a node.</summary>
    public void Remove(DomElement node)
    {
        _cache.Remove(node);
    }

    /// <summary>Add a pending clear for a parent node.</summary>
    public void AddPendingClear(DomElement parent, CachedLayout rect, bool isAbsolute)
    {
        if (!_pendingClears.TryGetValue(parent, out var list))
        {
            list = new List<PendingClear>();
            _pendingClears.AddOrUpdate(parent, list);
        }
        list.Add(new PendingClear(rect.X, rect.Y, rect.Width, rect.Height, isAbsolute));
        if (isAbsolute) _absoluteNodeRemoved = true;
    }

    /// <summary>Get and consume pending clears for a parent.</summary>
    public List<PendingClear>? ConsumePendingClears(DomElement parent)
    {
        if (!_pendingClears.TryGetValue(parent, out var list)) return null;
        _pendingClears.Remove(parent);
        return list;
    }

    /// <summary>Check and reset the absolute-node-removed flag.</summary>
    public bool ConsumeAbsoluteRemovedFlag()
    {
        bool had = _absoluteNodeRemoved;
        _absoluteNodeRemoved = false;
        return had;
    }
}
