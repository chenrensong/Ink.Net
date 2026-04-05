// -----------------------------------------------------------------------
// <copyright file="TerminalQuerier.cs" company="Ink.Net">
//   Terminal capability detection.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Terminal;

/// <summary>
/// Detects terminal capabilities and type.
/// </summary>
public static class TerminalQuerier
{
    /// <summary>Whether the terminal supports hyperlinks (OSC 8).</summary>
    public static bool SupportsHyperlinks()
    {
        // Most modern terminals support OSC 8
        string? term = Environment.GetEnvironmentVariable("TERM_PROGRAM");
        if (term is "iTerm.app" or "WezTerm" or "vscode" or "Hyper" or "ghostty")
            return true;

        string? colorTerm = Environment.GetEnvironmentVariable("COLORTERM");
        if (colorTerm is "truecolor" or "24bit") return true;

        // Kitty
        if (Environment.GetEnvironmentVariable("TERM")?.Contains("kitty") == true)
            return true;

        // Windows Terminal
        if (Environment.GetEnvironmentVariable("WT_SESSION") is not null)
            return true;

        return false;
    }

    /// <summary>Whether the terminal likely supports true color (24-bit).</summary>
    public static bool SupportsTrueColor()
    {
        string? colorTerm = Environment.GetEnvironmentVariable("COLORTERM");
        return colorTerm is "truecolor" or "24bit";
    }

    /// <summary>Whether the terminal supports the kitty keyboard protocol.</summary>
    public static bool SupportsKittyKeyboard()
    {
        string? term = Environment.GetEnvironmentVariable("TERM");
        return term?.Contains("kitty") == true ||
               Environment.GetEnvironmentVariable("TERM_PROGRAM") == "ghostty";
    }

    /// <summary>Whether we're inside tmux.</summary>
    public static bool IsTmux() => Environment.GetEnvironmentVariable("TMUX") is not null;

    /// <summary>Whether we're inside an SSH session.</summary>
    public static bool IsSsh() => Environment.GetEnvironmentVariable("SSH_CONNECTION") is not null;

    /// <summary>Get terminal name for diagnostics.</summary>
    public static string GetTerminalName()
    {
        return Environment.GetEnvironmentVariable("TERM_PROGRAM")
            ?? Environment.GetEnvironmentVariable("TERM")
            ?? "unknown";
    }
}
