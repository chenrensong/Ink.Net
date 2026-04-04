// -----------------------------------------------------------------------
// <copyright file="CursorHelpers.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) cursor-helpers.ts
// </copyright>
// -----------------------------------------------------------------------

using System.Text;

namespace Ink.Net.Terminal;

/// <summary>
/// Cursor position in terminal coordinates.
/// <para>Corresponds to JS <c>CursorPosition</c>.</para>
/// </summary>
public readonly struct CursorPosition : IEquatable<CursorPosition>
{
    public int X { get; }
    public int Y { get; }

    public CursorPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(CursorPosition other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is CursorPosition cp && Equals(cp);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public static bool operator ==(CursorPosition left, CursorPosition right) => left.Equals(right);
    public static bool operator !=(CursorPosition left, CursorPosition right) => !left.Equals(right);
}

/// <summary>
/// Input for cursor-only updates.
/// <para>Corresponds to JS <c>CursorOnlyInput</c>.</para>
/// </summary>
public sealed class CursorOnlyInput
{
    public bool CursorWasShown { get; init; }
    public int PreviousLineCount { get; init; }
    public CursorPosition? PreviousCursorPosition { get; init; }
    public int VisibleLineCount { get; init; }
    public CursorPosition? CurrentCursorPosition { get; init; }
}

/// <summary>
/// Utility functions for building ANSI cursor escape sequences.
/// <para>1:1 port of Ink JS <c>cursor-helpers.ts</c>.</para>
/// </summary>
public static class CursorHelpers
{
    /// <summary>Show cursor escape sequence.</summary>
    public const string ShowCursorEscape = "\u001B[?25h";

    /// <summary>Hide cursor escape sequence.</summary>
    public const string HideCursorEscape = "\u001B[?25l";

    // ─── ANSI escape builders (replacing ansi-escapes npm) ───────────

    /// <summary>Move cursor up by N lines.</summary>
    public static string CursorUp(int count) => count > 0 ? $"\u001B[{count}A" : "";

    /// <summary>Move cursor down by N lines.</summary>
    public static string CursorDown(int count) => count > 0 ? $"\u001B[{count}B" : "";

    /// <summary>Move cursor to column X.</summary>
    public static string CursorTo(int x) => $"\u001B[{x + 1}G";

    /// <summary>Erase N lines above the cursor.</summary>
    public static string EraseLines(int count)
    {
        if (count <= 0) return "";

        var sb = new StringBuilder();
        // First line: just clear
        sb.Append("\u001B[2K"); // Erase line

        for (int i = 1; i < count; i++)
        {
            sb.Append("\u001B[1A"); // Move up
            sb.Append("\u001B[2K"); // Erase line
        }

        sb.Append('\r'); // Go to beginning
        return sb.ToString();
    }

    // ─── Public API ──────────────────────────────────────────────────

    /// <summary>
    /// Compare two cursor positions. Returns true if they differ.
    /// <para>Corresponds to JS <c>cursorPositionChanged(a, b)</c>.</para>
    /// </summary>
    public static bool CursorPositionChanged(CursorPosition? a, CursorPosition? b)
    {
        if (a is null && b is null) return false;
        if (a is null || b is null) return true;
        return a.Value != b.Value;
    }

    /// <summary>
    /// Build escape sequence to move cursor from bottom of output to the target position.
    /// <para>Corresponds to JS <c>buildCursorSuffix(visibleLineCount, cursorPosition)</c>.</para>
    /// </summary>
    public static string BuildCursorSuffix(int visibleLineCount, CursorPosition? cursorPosition)
    {
        if (cursorPosition is null) return "";

        var pos = cursorPosition.Value;
        int moveUp = visibleLineCount - pos.Y;

        return (moveUp > 0 ? CursorUp(moveUp) : "") +
               CursorTo(pos.X) +
               ShowCursorEscape;
    }

    /// <summary>
    /// Build escape sequence to move cursor from previous position back to the bottom.
    /// <para>Corresponds to JS <c>buildReturnToBottom(previousLineCount, previousCursorPosition)</c>.</para>
    /// </summary>
    public static string BuildReturnToBottom(int previousLineCount, CursorPosition? previousCursorPosition)
    {
        if (previousCursorPosition is null) return "";

        var pos = previousCursorPosition.Value;
        int down = previousLineCount - 1 - pos.Y;

        return (down > 0 ? CursorDown(down) : "") + CursorTo(0);
    }

    /// <summary>
    /// Build the escape sequence for cursor-only updates (output unchanged, cursor moved).
    /// <para>Corresponds to JS <c>buildCursorOnlySequence(input)</c>.</para>
    /// </summary>
    public static string BuildCursorOnlySequence(CursorOnlyInput input)
    {
        string hidePrefix = input.CursorWasShown ? HideCursorEscape : "";
        string returnToBottom = BuildReturnToBottom(input.PreviousLineCount, input.PreviousCursorPosition);
        string cursorSuffix = BuildCursorSuffix(input.VisibleLineCount, input.CurrentCursorPosition);

        return hidePrefix + returnToBottom + cursorSuffix;
    }

    /// <summary>
    /// Build the prefix that hides cursor and returns to bottom before erasing or rewriting.
    /// <para>Corresponds to JS <c>buildReturnToBottomPrefix(...)</c>.</para>
    /// </summary>
    public static string BuildReturnToBottomPrefix(
        bool cursorWasShown, int previousLineCount, CursorPosition? previousCursorPosition)
    {
        if (!cursorWasShown) return "";

        return HideCursorEscape +
               BuildReturnToBottom(previousLineCount, previousCursorPosition);
    }
}
