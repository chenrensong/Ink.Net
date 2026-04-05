// -----------------------------------------------------------------------
// <copyright file="DecMode.cs" company="Ink.Net">
//   Port from Ink (JS) termio/dec.ts — DEC private mode sequences
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Termio;

/// <summary>DEC private mode sequences.</summary>
public static class DecMode
{
    public const int CursorVisible = 25;
    public const int AltScreen = 47;
    public const int AltScreenClear = 1049;
    public const int MouseNormal = 1000;
    public const int MouseButton = 1002;
    public const int MouseAny = 1003;
    public const int MouseSgr = 1006;
    public const int FocusEvents = 1004;
    public const int BracketedPaste = 2004;
    public const int SynchronizedUpdate = 2026;

    public static string Set(int mode) => CsiHelper.Csi($"?{mode}h");
    public static string Reset(int mode) => CsiHelper.Csi($"?{mode}l");

    // Pre-generated sequences
    public static readonly string BeginSynchronizedUpdate = Set(SynchronizedUpdate);
    public static readonly string EndSynchronizedUpdate = Reset(SynchronizedUpdate);
    public static readonly string EnableBracketedPaste = Set(BracketedPaste);
    public static readonly string DisableBracketedPaste = Reset(BracketedPaste);
    public static readonly string EnableFocusEvents = Set(FocusEvents);
    public static readonly string DisableFocusEvents = Reset(FocusEvents);
    public static readonly string ShowCursor = Set(CursorVisible);
    public static readonly string HideCursor = Reset(CursorVisible);
    public static readonly string EnterAltScreen = Set(AltScreenClear);
    public static readonly string ExitAltScreen = Reset(AltScreenClear);
    public static readonly string EnableMouseTracking = Set(MouseNormal) + Set(MouseButton) + Set(MouseAny) + Set(MouseSgr);
    public static readonly string DisableMouseTracking = Reset(MouseSgr) + Reset(MouseAny) + Reset(MouseButton) + Reset(MouseNormal);
}
