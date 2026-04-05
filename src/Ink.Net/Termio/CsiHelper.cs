// -----------------------------------------------------------------------
// <copyright file="CsiHelper.cs" company="Ink.Net">
//   Port from Ink (JS) termio/csi.ts — CSI sequence generators
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Termio;

/// <summary>
/// CSI (Control Sequence Introducer) helper methods for generating terminal sequences.
/// </summary>
public static class CsiHelper
{
    public const string CsiPrefix = "\x1b[";

    public static string Csi(params object[] args) => args.Length switch
    {
        0 => CsiPrefix,
        1 => $"{CsiPrefix}{args[0]}",
        _ => $"{CsiPrefix}{string.Join(';', args.SkipLast(1))}{args[^1]}",
    };

    // Cursor movement
    public static string CursorUp(int n = 1) => n == 0 ? "" : Csi(n, "A");
    public static string CursorDown(int n = 1) => n == 0 ? "" : Csi(n, "B");
    public static string CursorForward(int n = 1) => n == 0 ? "" : Csi(n, "C");
    public static string CursorBack(int n = 1) => n == 0 ? "" : Csi(n, "D");
    public static string CursorTo(int col) => Csi(col, "G");
    public static string CursorPosition(int row, int col) => Csi(row, col, "H");

    public static string CursorMove(int x, int y)
    {
        string result = "";
        if (x < 0) result += CursorBack(-x);
        else if (x > 0) result += CursorForward(x);
        if (y < 0) result += CursorUp(-y);
        else if (y > 0) result += CursorDown(y);
        return result;
    }

    // Erase
    public static readonly string EraseLine = Csi(2, "K");
    public static readonly string EraseScreen = Csi(2, "J");
    public static readonly string EraseScrollback = Csi(3, "J");
    public static string EraseLines(int n)
    {
        if (n <= 0) return "";
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < n; i++)
        {
            sb.Append(EraseLine);
            if (i < n - 1) sb.Append(CursorUp(1));
        }
        sb.Append(Csi("G")); // cursor to col 1
        return sb.ToString();
    }

    // Scroll
    public static string ScrollUp(int n = 1) => n == 0 ? "" : Csi(n, "S");
    public static string ScrollDown(int n = 1) => n == 0 ? "" : Csi(n, "T");
    public static string SetScrollRegion(int top, int bottom) => Csi(top, bottom, "r");
    public static readonly string ResetScrollRegion = Csi("r");

    // Save/Restore
    public static readonly string CursorSave = Csi("s");
    public static readonly string CursorRestore = Csi("u");
    public static readonly string CursorHome = Csi("H");
    public static readonly string CursorLeft = Csi("G");

    // Bracketed paste markers
    public static readonly string PasteStart = Csi("200~");
    public static readonly string PasteEnd = Csi("201~");
    public static readonly string FocusIn = Csi("I");
    public static readonly string FocusOut = Csi("O");

    // Kitty keyboard protocol
    public static readonly string EnableKittyKeyboard = Csi(">1u");
    public static readonly string DisableKittyKeyboard = Csi("<u");
    public static readonly string EnableModifyOtherKeys = Csi(">4;2m");
    public static readonly string DisableModifyOtherKeys = Csi(">4m");
}
