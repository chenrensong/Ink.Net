// -----------------------------------------------------------------------
// <copyright file="SearchHighlight.cs" company="Ink.Net">
//   Port from Ink (JS) searchHighlight.ts — Search match highlighting
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Selection;

/// <summary>
/// A search match at a specific screen position.
/// </summary>
public readonly record struct SearchMatch(int StartCol, int Row, int Length);

/// <summary>
/// Search highlight state. Tracks search matches and the current match index.
/// Corresponds to JS useSearchHighlight.
/// </summary>
public sealed class SearchHighlight
{
    private readonly List<SearchMatch> _matches = new();
    private int _currentIndex = -1;

    /// <summary>All current search matches.</summary>
    public IReadOnlyList<SearchMatch> Matches => _matches;

    /// <summary>Index of the current (focused) match, or -1 if none.</summary>
    public int CurrentIndex
    {
        get => _currentIndex;
        set => _currentIndex = _matches.Count > 0 ? Math.Clamp(value, -1, _matches.Count - 1) : -1;
    }

    /// <summary>The current match, or null if none.</summary>
    public SearchMatch? CurrentMatch =>
        _currentIndex >= 0 && _currentIndex < _matches.Count ? _matches[_currentIndex] : null;

    /// <summary>Total number of matches.</summary>
    public int Count => _matches.Count;

    /// <summary>Update the set of matches.</summary>
    public void SetMatches(IEnumerable<SearchMatch> matches)
    {
        _matches.Clear();
        _matches.AddRange(matches);
        if (_currentIndex >= _matches.Count)
            _currentIndex = _matches.Count - 1;
    }

    /// <summary>Clear all matches.</summary>
    public void Clear()
    {
        _matches.Clear();
        _currentIndex = -1;
    }

    /// <summary>Move to the next match.</summary>
    public void Next()
    {
        if (_matches.Count == 0) return;
        _currentIndex = (_currentIndex + 1) % _matches.Count;
    }

    /// <summary>Move to the previous match.</summary>
    public void Previous()
    {
        if (_matches.Count == 0) return;
        _currentIndex = (_currentIndex - 1 + _matches.Count) % _matches.Count;
    }

    /// <summary>
    /// Check if a cell is part of any search match and whether it's the current match.
    /// </summary>
    /// <param name="col">Column position.</param>
    /// <param name="row">Row position.</param>
    /// <param name="isCurrent">True if this cell is part of the current (focused) match.</param>
    /// <returns>True if the cell is part of any match.</returns>
    public bool IsHighlighted(int col, int row, out bool isCurrent)
    {
        isCurrent = false;
        for (int i = 0; i < _matches.Count; i++)
        {
            var m = _matches[i];
            if (m.Row == row && col >= m.StartCol && col < m.StartCol + m.Length)
            {
                if (i == _currentIndex)
                    isCurrent = true;
                return true;
            }
        }
        return false;
    }
}
