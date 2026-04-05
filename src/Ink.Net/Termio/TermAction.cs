// -----------------------------------------------------------------------
// <copyright file="TermAction.cs" company="Ink.Net">
//   Port from Ink (JS) termio/types.ts — Parsed terminal actions
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Termio;

/// <summary>All possible parsed terminal actions.</summary>
public abstract record TermAction
{
    public sealed record Text(string Content, TextStyle Style) : TermAction;
    public sealed record CursorMove(string Direction, int Count) : TermAction;
    public sealed record CursorPosition(int Row, int Col) : TermAction;
    public sealed record CursorColumn(int Col) : TermAction;
    public sealed record CursorSave : TermAction;
    public sealed record CursorRestore : TermAction;
    public sealed record CursorShow : TermAction;
    public sealed record CursorHide : TermAction;
    public sealed record EraseDisplay(string Region) : TermAction; // "toEnd","toStart","all","scrollback"
    public sealed record EraseLine(string Region) : TermAction; // "toEnd","toStart","all"
    public sealed record ScrollUp(int Count) : TermAction;
    public sealed record ScrollDown(int Count) : TermAction;
    public sealed record SetScrollRegion(int Top, int Bottom) : TermAction;
    public sealed record AlternateScreen(bool Enabled) : TermAction;
    public sealed record BracketedPaste(bool Enabled) : TermAction;
    public sealed record MouseTracking(string Mode) : TermAction; // "off","normal","button","any"
    public sealed record FocusEvents(bool Enabled) : TermAction;
    public sealed record LinkStart(string Url) : TermAction;
    public sealed record LinkEnd : TermAction;
    public sealed record WindowTitle(string Title) : TermAction;
    public sealed record Sgr(string Params) : TermAction;
    public sealed record Bell : TermAction;
    public sealed record Reset : TermAction;
    public sealed record Unknown(string Sequence) : TermAction;
}
