// -----------------------------------------------------------------------
// <copyright file="CharPool.cs" company="Ink.Net">
//   Port from Ink (JS) screen.ts — Character string interning pool
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Rendering.Screen;

/// <summary>
/// Character string interning pool shared across all screens.
/// Interned char IDs are valid across screens, so blit/diff can compare IDs as integers.
/// </summary>
public sealed class CharPool
{
    private readonly List<string> _strings = new() { " ", "" }; // 0 = space, 1 = empty (spacer)
    private readonly Dictionary<string, int> _map = new() { [" "] = 0, [""] = 1 };
    private readonly int[] _ascii = new int[128]; // charCode → index, -1 = not interned

    public const int SpaceIndex = 0;
    public const int EmptyIndex = 1;

    public CharPool()
    {
        Array.Fill(_ascii, -1);
        _ascii[32] = SpaceIndex; // ' '
    }

    /// <summary>Intern a character string and return its unique ID.</summary>
    public int Intern(string ch)
    {
        // ASCII fast-path
        if (ch.Length == 1)
        {
            int code = ch[0];
            if (code < 128)
            {
                int cached = _ascii[code];
                if (cached != -1) return cached;
                int index = _strings.Count;
                _strings.Add(ch);
                _ascii[code] = index;
                return index;
            }
        }

        if (_map.TryGetValue(ch, out int existing))
            return existing;

        int id = _strings.Count;
        _strings.Add(ch);
        _map[ch] = id;
        return id;
    }

    /// <summary>Get the string for a given ID.</summary>
    public string Get(int index) => index >= 0 && index < _strings.Count ? _strings[index] : " ";
}
