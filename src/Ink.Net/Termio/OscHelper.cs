// -----------------------------------------------------------------------
// <copyright file="OscHelper.cs" company="Ink.Net">
//   Port from Ink (JS) termio/osc.ts — OSC sequence generators
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Termio;

/// <summary>OSC command numbers.</summary>
public static class OscCommand
{
    public const int SetTitleAndIcon = 0;
    public const int SetIcon = 1;
    public const int SetTitle = 2;
    public const int SetColor = 4;
    public const int SetCwd = 7;
    public const int Hyperlink = 8;
    public const int Clipboard = 52;
    public const int TabStatus = 21337;
}

/// <summary>OSC sequence helpers.</summary>
public static class OscHelper
{
    public static string Osc(params object[] parts) =>
        $"\x1b]{string.Join(';', parts)}\x07";

    /// <summary>Generate OSC 8 hyperlink start sequence.</summary>
    public static string LinkStart(string url, string? id = null)
    {
        if (string.IsNullOrEmpty(url)) return LinkEnd;
        string paramStr = string.IsNullOrEmpty(id) ? $"id={OscId(url)}" : $"id={id}";
        return Osc(OscCommand.Hyperlink, paramStr, url);
    }

    /// <summary>Generate OSC 8 hyperlink end sequence.</summary>
    public static readonly string LinkEnd = Osc(OscCommand.Hyperlink, "", "");

    /// <summary>Generate OSC 52 clipboard write sequence.</summary>
    public static string SetClipboard(string text)
    {
        var b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text));
        return Osc(OscCommand.Clipboard, "c", b64);
    }

    /// <summary>Wrap a sequence for tmux DCS passthrough.</summary>
    public static string WrapForTmux(string sequence)
    {
        var escaped = sequence.Replace("\x1b", "\x1b\x1b");
        return $"\x1bPtmux;{escaped}\x1b\\";
    }

    private static string OscId(string url)
    {
        int h = 0;
        foreach (char c in url) h = ((h << 5) - h + c) | 0;
        return ((uint)h).ToString("x");
    }
}
