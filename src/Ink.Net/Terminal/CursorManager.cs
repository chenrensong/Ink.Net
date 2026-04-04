// -----------------------------------------------------------------------
// <copyright file="CursorManager.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) use-cursor.ts + CursorContext.ts
//   Manages terminal cursor position and visibility.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Terminal;

/// <summary>
/// Manages the terminal cursor position for an Ink application.
/// <para>
/// C# equivalent of the JS <c>useCursor</c> hook and <c>CursorContext</c>.
/// Setting a position makes the cursor visible at the specified coordinates.
/// Setting <c>null</c> hides the cursor.
/// </para>
/// </summary>
public sealed class CursorManager
{
    private CursorPosition? _cursorPosition;
    private readonly object _lock = new();

    /// <summary>
    /// Raised when the cursor position changes.
    /// </summary>
    public event Action<CursorPosition?>? CursorPositionChanged;

    /// <summary>
    /// The current cursor position, or <c>null</c> if cursor is hidden.
    /// <para>Corresponds to JS <c>CursorContext.setCursorPosition</c>.</para>
    /// </summary>
    public CursorPosition? Position
    {
        get
        {
            lock (_lock) return _cursorPosition;
        }
    }

    /// <summary>
    /// Set the cursor position. Pass <c>null</c> to hide the cursor.
    /// <para>Corresponds to JS <c>useCursor().setCursorPosition(position)</c>.</para>
    /// </summary>
    public void SetCursorPosition(CursorPosition? position)
    {
        lock (_lock)
        {
            if (CursorHelpers.CursorPositionChanged(_cursorPosition, position))
            {
                _cursorPosition = position;
                CursorPositionChanged?.Invoke(position);
            }
        }
    }

    /// <summary>
    /// Set the cursor position by x/y coordinates.
    /// </summary>
    public void SetCursorPosition(int x, int y)
    {
        SetCursorPosition(new CursorPosition(x, y));
    }

    /// <summary>
    /// Hide the cursor (same as <c>SetCursorPosition(null)</c>).
    /// </summary>
    public void HideCursor()
    {
        SetCursorPosition(null);
    }
}
