// -----------------------------------------------------------------------
// <copyright file="TerminalFocusTracker.cs" company="Ink.Net">
//   Tracks terminal window focus state using DEC mode 1004.
//   Corresponds to JS useTerminalFocus / terminal-focus-state.ts.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Terminal;

/// <summary>
/// Tracks terminal window focus state using DEC mode 1004 (focus events).
/// Corresponds to JS useTerminalFocus / terminal-focus-state.ts.
/// </summary>
public sealed class TerminalFocusTracker : IDisposable
{
    private readonly TextWriter _output;
    private bool _isFocused = true;
    private bool _enabled;

    /// <summary>Gets whether the terminal window is currently focused.</summary>
    public bool IsFocused => _isFocused;

    /// <summary>Fired when focus state changes.</summary>
    public event Action<bool>? FocusChanged;

    /// <summary>
    /// Initializes a new <see cref="TerminalFocusTracker"/>.
    /// </summary>
    /// <param name="output">The output stream to write escape sequences to.</param>
    public TerminalFocusTracker(TextWriter output)
    {
        _output = output;
    }

    /// <summary>Enable DEC mode 1004 focus tracking.</summary>
    public void Enable()
    {
        if (_enabled) return;
        _enabled = true;
        // CSI ? 1004 h — enable focus reporting
        _output.Write("\x1b[?1004h");
        _output.Flush();
    }

    /// <summary>Disable DEC mode 1004 focus tracking.</summary>
    public void Disable()
    {
        if (!_enabled) return;
        _enabled = false;
        // CSI ? 1004 l — disable focus reporting
        _output.Write("\x1b[?1004l");
        _output.Flush();
    }

    /// <summary>
    /// Handle incoming terminal data to detect focus/blur sequences.
    /// Call this with raw input data from stdin.
    /// </summary>
    /// <param name="data">Raw input data.</param>
    /// <returns>True if a focus sequence was consumed.</returns>
    public bool HandleInput(string data)
    {
        if (!_enabled) return false;

        // CSI I = focus gained, CSI O = focus lost
        if (data == "\x1b[I")
        {
            if (!_isFocused)
            {
                _isFocused = true;
                FocusChanged?.Invoke(true);
            }
            return true;
        }

        if (data == "\x1b[O")
        {
            if (_isFocused)
            {
                _isFocused = false;
                FocusChanged?.Invoke(false);
            }
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Disable();
    }
}
