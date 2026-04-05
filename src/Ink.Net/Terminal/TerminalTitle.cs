// -----------------------------------------------------------------------
// <copyright file="TerminalTitle.cs" company="Ink.Net">
//   Terminal title and tab status helpers via OSC sequences.
//   Corresponds to JS useTerminalTitle / useTabStatus.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Terminal;

/// <summary>
/// Helpers for setting terminal window title and tab status via OSC sequences.
/// </summary>
public sealed class TerminalTitle : IDisposable
{
    private readonly TextWriter _output;

    public TerminalTitle(TextWriter output)
    {
        _output = output;
    }

    /// <summary>
    /// Set the terminal window title (OSC 2).
    /// Corresponds to JS useTerminalTitle.
    /// </summary>
    public void SetTitle(string title)
    {
        // OSC 2 ; title ST
        _output.Write($"\x1b]2;{title}\x07");
        _output.Flush();
    }

    /// <summary>
    /// Set the tab/icon title (OSC 1).
    /// Used by some terminal emulators for tab labels.
    /// </summary>
    public void SetIconTitle(string title)
    {
        // OSC 1 ; title ST
        _output.Write($"\x1b]1;{title}\x07");
        _output.Flush();
    }

    /// <summary>
    /// Set both window and icon title (OSC 0).
    /// </summary>
    public void SetBothTitles(string title)
    {
        // OSC 0 ; title ST
        _output.Write($"\x1b]0;{title}\x07");
        _output.Flush();
    }

    /// <summary>
    /// Save the current title (if terminal supports it).
    /// </summary>
    public void Save()
    {
        // CSI 22 ; 0 t — save title on stack
        _output.Write("\x1b[22;0t");
        _output.Flush();
    }

    /// <summary>
    /// Restore a previously saved title.
    /// </summary>
    public void Restore()
    {
        // CSI 23 ; 0 t — restore title from stack
        _output.Write("\x1b[23;0t");
        _output.Flush();
    }

    public void Dispose()
    {
        // Optionally restore title on dispose
    }
}

/// <summary>
/// Tab status helpers using iTerm2/Kitty custom escape sequences.
/// Corresponds to JS useTabStatus.
/// </summary>
public static class TabStatus
{
    /// <summary>
    /// Set a custom tab status indicator text.
    /// Only works in terminals that support it (iTerm2, Kitty).
    /// </summary>
    public static void Set(TextWriter output, string status)
    {
        // iTerm2: OSC 1337 ; SetBadgeFormat=base64 ST
        // We use a simpler approach: set icon title (OSC 1)
        output.Write($"\x1b]1;{status}\x07");
        output.Flush();
    }
}
