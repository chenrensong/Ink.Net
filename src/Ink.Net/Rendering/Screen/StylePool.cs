// -----------------------------------------------------------------------
// <copyright file="StylePool.cs" company="Ink.Net">
//   Port from Ink (JS) screen.ts — ANSI style interning pool with transition cache
// </copyright>
// -----------------------------------------------------------------------

using System.Text;

namespace Ink.Net.Rendering.Screen;

/// <summary>
/// Represents a single ANSI style code with its opening and closing sequences.
/// </summary>
public readonly record struct AnsiStyleCode(string Code, string EndCode);

/// <summary>
/// ANSI style interning pool with transition caching.
/// Bit 0 of the ID encodes whether the style is visible on space characters.
/// </summary>
public sealed class StylePool
{
    private readonly Dictionary<string, int> _ids = new();
    private readonly List<AnsiStyleCode[]> _styles = new();
    private readonly Dictionary<long, string> _transitionCache = new();

    /// <summary>ID for the "no style" state.</summary>
    public int None { get; }

    public StylePool()
    {
        None = Intern(Array.Empty<AnsiStyleCode>());
    }

    /// <summary>
    /// Intern a set of ANSI style codes and return a unique ID.
    /// Bit 0 encodes visibility-on-space: odd = visible on spaces.
    /// </summary>
    public int Intern(AnsiStyleCode[] styles)
    {
        string key = styles.Length == 0 ? "" : string.Join('\0', styles.Select(s => s.Code));
        if (_ids.TryGetValue(key, out int id)) return id;

        int rawId = _styles.Count;
        _styles.Add(styles.Length == 0 ? Array.Empty<AnsiStyleCode>() : styles);
        id = (rawId << 1) | (styles.Length > 0 && HasVisibleSpaceEffect(styles) ? 1 : 0);
        _ids[key] = id;
        return id;
    }

    /// <summary>Get style codes for an encoded ID (strips bit 0).</summary>
    public AnsiStyleCode[] Get(int id)
    {
        int rawId = id >>> 1;
        return rawId < _styles.Count ? _styles[rawId] : Array.Empty<AnsiStyleCode>();
    }

    /// <summary>
    /// Get the ANSI transition string from one style to another.
    /// Cached for zero allocation after first call.
    /// </summary>
    public string Transition(int fromId, int toId)
    {
        if (fromId == toId) return "";
        long key = (long)fromId * 0x100000L + toId;
        if (_transitionCache.TryGetValue(key, out string? str)) return str;

        var fromCodes = Get(fromId);
        var toCodes = Get(toId);
        str = ComputeTransition(fromCodes, toCodes);
        _transitionCache[key] = str;
        return str;
    }

    /// <summary>Intern a style that is base + inverse.</summary>
    public int WithInverse(int baseId)
    {
        var baseCodes = Get(baseId);
        if (baseCodes.Any(c => c.EndCode == "\x1b[27m"))
            return baseId; // Already inverted
        var codes = baseCodes.Append(new AnsiStyleCode("\x1b[7m", "\x1b[27m")).ToArray();
        return Intern(codes);
    }

    private static bool HasVisibleSpaceEffect(AnsiStyleCode[] styles)
    {
        foreach (var s in styles)
        {
            if (s.EndCode is "\x1b[49m" or "\x1b[27m" or "\x1b[24m" or "\x1b[29m" or "\x1b[55m")
                return true;
        }
        return false;
    }

    private static string ComputeTransition(AnsiStyleCode[] from, AnsiStyleCode[] to)
    {
        if (to.Length == 0 && from.Length == 0) return "";

        var sb = new StringBuilder();

        // Close codes that are in 'from' but not in 'to'
        var toEndCodes = new HashSet<string>(to.Select(c => c.EndCode));
        foreach (var code in from)
        {
            if (!toEndCodes.Contains(code.EndCode))
                sb.Append(code.EndCode);
        }

        // Open codes that are in 'to' but not in 'from'
        var fromCodes = new HashSet<string>(from.Select(c => c.Code));
        foreach (var code in to)
        {
            if (!fromCodes.Contains(code.Code))
                sb.Append(code.Code);
        }

        return sb.ToString();
    }
}
