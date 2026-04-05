// -----------------------------------------------------------------------
// <copyright file="HyperlinkPool.cs" company="Ink.Net">
//   Port from Ink (JS) screen.ts — Hyperlink string interning pool
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Rendering.Screen;

/// <summary>
/// Hyperlink string interning pool. Index 0 = no hyperlink.
/// </summary>
public sealed class HyperlinkPool
{
    private readonly List<string> _strings = new() { "" }; // 0 = no hyperlink
    private readonly Dictionary<string, int> _map = new();

    /// <summary>Intern a hyperlink URL and return its ID. Null/empty returns 0.</summary>
    public int Intern(string? hyperlink)
    {
        if (string.IsNullOrEmpty(hyperlink)) return 0;
        if (_map.TryGetValue(hyperlink, out int id)) return id;
        id = _strings.Count;
        _strings.Add(hyperlink);
        _map[hyperlink] = id;
        return id;
    }

    /// <summary>Get hyperlink URL for an ID. Returns null for ID 0.</summary>
    public string? Get(int id) => id == 0 ? null : (id < _strings.Count ? _strings[id] : null);
}
