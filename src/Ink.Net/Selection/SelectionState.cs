// -----------------------------------------------------------------------
// <copyright file="SelectionState.cs" company="Ink.Net">
//   Port from Ink (JS) selection.ts — Text selection state management
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Selection;

/// <summary>
/// A point in screen-buffer coordinates (0-indexed column/row).
/// </summary>
public readonly record struct SelectionPoint(int Col, int Row);

/// <summary>
/// Selection span with kind for word/line mode.
/// </summary>
public sealed class SelectionSpan
{
    public SelectionPoint Lo { get; set; }
    public SelectionPoint Hi { get; set; }
    public SelectionKind Kind { get; set; }
}

/// <summary>
/// Selection granularity kind.
/// </summary>
public enum SelectionKind
{
    Char,
    Word,
    Line
}

/// <summary>
/// Text selection state for fullscreen mode.
/// Tracks a linear selection in screen-buffer coordinates.
/// Corresponds to JS selection.ts SelectionState.
/// </summary>
public sealed class SelectionState
{
    /// <summary>Where the mouse-down occurred. Null when no selection.</summary>
    public SelectionPoint? Anchor { get; set; }

    /// <summary>Current drag position (updated on mouse-move while dragging).</summary>
    public SelectionPoint? Focus { get; set; }

    /// <summary>True between mouse-down and mouse-up.</summary>
    public bool IsDragging { get; set; }

    /// <summary>Anchor span for word/line mode multi-click selection.</summary>
    public SelectionSpan? AnchorSpan { get; set; }

    /// <summary>Text from rows scrolled off above during drag-to-scroll.</summary>
    public List<string> ScrolledOffAbove { get; } = new();

    /// <summary>Text from rows scrolled off below during drag-to-scroll.</summary>
    public List<string> ScrolledOffBelow { get; } = new();

    /// <summary>Whether the selection is active (has both anchor and focus).</summary>
    public bool IsActive => Anchor.HasValue && Focus.HasValue;

    /// <summary>Get the normalized selection range (start ≤ end).</summary>
    public (SelectionPoint Start, SelectionPoint End)? GetRange()
    {
        if (!Anchor.HasValue || !Focus.HasValue)
            return null;

        var a = Anchor.Value;
        var f = Focus.Value;

        if (a.Row < f.Row || (a.Row == f.Row && a.Col <= f.Col))
            return (a, f);
        return (f, a);
    }

    /// <summary>Start a new selection at the given position.</summary>
    public void Start(int col, int row)
    {
        var point = new SelectionPoint(col, row);
        Anchor = point;
        Focus = point;
        IsDragging = true;
        AnchorSpan = null;
        ScrolledOffAbove.Clear();
        ScrolledOffBelow.Clear();
    }

    /// <summary>Update the focus point during drag.</summary>
    public void Update(int col, int row)
    {
        Focus = new SelectionPoint(col, row);
    }

    /// <summary>End the drag (mouse-up).</summary>
    public void End()
    {
        IsDragging = false;
    }

    /// <summary>Clear the selection entirely.</summary>
    public void Clear()
    {
        Anchor = null;
        Focus = null;
        IsDragging = false;
        AnchorSpan = null;
        ScrolledOffAbove.Clear();
        ScrolledOffBelow.Clear();
    }

    /// <summary>
    /// Check if a screen cell is within the selection range.
    /// </summary>
    public bool Contains(int col, int row)
    {
        var range = GetRange();
        if (range is null) return false;

        var (start, end) = range.Value;

        if (row < start.Row || row > end.Row)
            return false;

        if (row == start.Row && row == end.Row)
            return col >= start.Col && col <= end.Col;

        if (row == start.Row)
            return col >= start.Col;

        if (row == end.Row)
            return col <= end.Col;

        return true; // Middle rows are fully selected
    }
}
