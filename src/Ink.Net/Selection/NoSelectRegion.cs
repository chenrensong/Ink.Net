// -----------------------------------------------------------------------
// <copyright file="NoSelectRegion.cs" company="Ink.Net">
//   Tracks regions excluded from text selection (NoSelect component).
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Selection;

/// <summary>
/// Manages no-select regions on screen. Content in these regions is excluded
/// from text selection. Corresponds to JS NoSelect component + screen.markNoSelectRegion.
/// </summary>
public sealed class NoSelectRegion
{
    private readonly HashSet<(int X, int Y)> _noSelectCells = new();

    /// <summary>Mark a rectangular region as non-selectable.</summary>
    public void Mark(int x, int y, int width, int height)
    {
        for (int row = y; row < y + height; row++)
        {
            for (int col = x; col < x + width; col++)
            {
                _noSelectCells.Add((col, row));
            }
        }
    }

    /// <summary>Check if a cell is in a no-select region.</summary>
    public bool IsNoSelect(int x, int y) => _noSelectCells.Contains((x, y));

    /// <summary>Clear all no-select regions (called before each render).</summary>
    public void Clear() => _noSelectCells.Clear();
}
