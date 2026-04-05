// -----------------------------------------------------------------------
// <copyright file="MouseTracking.cs" company="Ink.Net">
//   DEC mouse tracking support for terminal mouse events.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Terminal;

/// <summary>
/// Mouse button identifiers.
/// </summary>
public enum MouseButton
{
    Left = 0,
    Middle = 1,
    Right = 2,
    ScrollUp = 64,
    ScrollDown = 65,
    None = 3
}

/// <summary>
/// Mouse event type.
/// </summary>
public enum MouseEventType
{
    Press,
    Release,
    Move,
    ScrollUp,
    ScrollDown
}

/// <summary>
/// A parsed mouse event from the terminal.
/// </summary>
public readonly record struct MouseEvent(
    MouseEventType Type,
    MouseButton Button,
    int X,
    int Y,
    bool Shift,
    bool Alt,
    bool Ctrl
);

/// <summary>
/// DEC mouse tracking management.
/// Handles enabling/disabling mouse protocols and parsing mouse escape sequences.
/// </summary>
public sealed class MouseTracking : IDisposable
{
    private readonly TextWriter _output;
    private bool _enabled;

    /// <summary>Fired when a mouse event is received.</summary>
    public event Action<MouseEvent>? MouseEventReceived;

    public MouseTracking(TextWriter output)
    {
        _output = output;
    }

    /// <summary>
    /// Enable SGR mouse tracking (mode 1006) with button and motion events.
    /// </summary>
    public void Enable()
    {
        if (_enabled) return;
        _enabled = true;
        // Enable button event tracking (1002) + SGR encoding (1006)
        _output.Write("\x1b[?1002h\x1b[?1006h");
        _output.Flush();
    }

    /// <summary>Disable mouse tracking.</summary>
    public void Disable()
    {
        if (!_enabled) return;
        _enabled = false;
        _output.Write("\x1b[?1002l\x1b[?1006l");
        _output.Flush();
    }

    /// <summary>
    /// Try to parse a mouse event from raw terminal input.
    /// SGR format: CSI &lt; Pb ; Px ; Py M (press) or CSI &lt; Pb ; Px ; Py m (release)
    /// </summary>
    /// <param name="data">Raw input data.</param>
    /// <param name="evt">Parsed mouse event.</param>
    /// <returns>True if a mouse event was parsed.</returns>
    public bool TryParse(string data, out MouseEvent evt)
    {
        evt = default;
        if (!_enabled) return false;

        // SGR mouse: \x1b[<Pb;Px;PyM or \x1b[<Pb;Px;Pym
        if (data.Length < 6 || !data.StartsWith("\x1b[<")) return false;

        bool isRelease = data[^1] == 'm';
        bool isPress = data[^1] == 'M';
        if (!isRelease && !isPress) return false;

        var body = data.AsSpan(3, data.Length - 4); // Strip "\x1b[<" and final char
        Span<Range> parts = stackalloc Range[3];
        if (body.Split(parts, ';') != 3) return false;

        if (!int.TryParse(body[parts[0]], out var buttonCode)) return false;
        if (!int.TryParse(body[parts[1]], out var x)) return false;
        if (!int.TryParse(body[parts[2]], out var y)) return false;

        // Convert to 0-indexed
        x--;
        y--;

        bool shift = (buttonCode & 4) != 0;
        bool alt = (buttonCode & 8) != 0;
        bool ctrl = (buttonCode & 16) != 0;
        bool motion = (buttonCode & 32) != 0;
        int baseButton = buttonCode & 3;

        MouseEventType type;
        MouseButton button;

        if ((buttonCode & 64) != 0)
        {
            // Scroll events
            type = baseButton == 0 ? MouseEventType.ScrollUp : MouseEventType.ScrollDown;
            button = baseButton == 0 ? MouseButton.ScrollUp : MouseButton.ScrollDown;
        }
        else if (isRelease)
        {
            type = MouseEventType.Release;
            button = (MouseButton)baseButton;
        }
        else if (motion)
        {
            type = MouseEventType.Move;
            button = (MouseButton)baseButton;
        }
        else
        {
            type = MouseEventType.Press;
            button = (MouseButton)baseButton;
        }

        evt = new MouseEvent(type, button, x, y, shift, alt, ctrl);
        MouseEventReceived?.Invoke(evt);
        return true;
    }

    public void Dispose()
    {
        Disable();
    }
}
