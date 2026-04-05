// -----------------------------------------------------------------------
// <copyright file="HyperlinkSupport.cs" company="Ink.Net">
//   Port from Ink (JS) supports-hyperlinks.ts — Terminal hyperlink detection
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Terminal;

/// <summary>
/// Detects whether the current terminal supports OSC 8 hyperlinks.
/// </summary>
public static class HyperlinkSupport
{
    private static bool? _cached;

    /// <summary>Check if the terminal supports clickable hyperlinks (OSC 8).</summary>
    public static bool IsSupported()
    {
        if (_cached.HasValue) return _cached.Value;

        _cached = Detect();
        return _cached.Value;
    }

    /// <summary>Reset the cached detection result (for testing).</summary>
    public static void ResetCache() => _cached = null;

    private static bool Detect()
    {
        // Force via env var
        string? force = Environment.GetEnvironmentVariable("FORCE_HYPERLINK");
        if (force is "1" or "true") return true;
        if (force is "0" or "false") return false;

        // Known supporting terminals
        string? termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
        if (termProgram is "iTerm.app" or "WezTerm" or "Hyper" or "ghostty")
            return true;

        // VS Code terminal
        if (termProgram == "vscode") return true;

        // Windows Terminal
        if (Environment.GetEnvironmentVariable("WT_SESSION") is not null)
            return true;

        // Kitty
        if (Environment.GetEnvironmentVariable("TERM")?.Contains("kitty") == true)
            return true;

        // COLORTERM=truecolor usually implies modern terminal
        string? colorTerm = Environment.GetEnvironmentVariable("COLORTERM");
        if (colorTerm is "truecolor" or "24bit") return true;

        // Default: not supported
        return false;
    }
}
