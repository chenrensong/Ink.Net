// -----------------------------------------------------------------------
// <copyright file="KeyInfo.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) use-input.ts — Key type
//   Structured key information for input handlers.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Input;

/// <summary>
/// Event type for key events (kitty protocol only).
/// </summary>
public enum InputEventType
{
    /// <summary>Key was pressed.</summary>
    Press,
    /// <summary>Key is being held (repeat).</summary>
    Repeat,
    /// <summary>Key was released.</summary>
    Release,
}

/// <summary>
/// Handy information about a key that was pressed.
/// <para>1:1 port of JS <c>Key</c> type from <c>use-input.ts</c>.</para>
/// </summary>
public sealed class KeyInfo
{
    /// <summary>Up arrow key was pressed.</summary>
    public bool UpArrow { get; set; }

    /// <summary>Down arrow key was pressed.</summary>
    public bool DownArrow { get; set; }

    /// <summary>Left arrow key was pressed.</summary>
    public bool LeftArrow { get; set; }

    /// <summary>Right arrow key was pressed.</summary>
    public bool RightArrow { get; set; }

    /// <summary>Page Down key was pressed.</summary>
    public bool PageDown { get; set; }

    /// <summary>Page Up key was pressed.</summary>
    public bool PageUp { get; set; }

    /// <summary>Home key was pressed.</summary>
    public bool Home { get; set; }

    /// <summary>End key was pressed.</summary>
    public bool End { get; set; }

    /// <summary>Return (Enter) key was pressed.</summary>
    public bool Return { get; set; }

    /// <summary>Escape key was pressed.</summary>
    public bool Escape { get; set; }

    /// <summary>Ctrl key was pressed.</summary>
    public bool Ctrl { get; set; }

    /// <summary>Shift key was pressed.</summary>
    public bool Shift { get; set; }

    /// <summary>Tab key was pressed.</summary>
    public bool Tab { get; set; }

    /// <summary>Backspace key was pressed.</summary>
    public bool Backspace { get; set; }

    /// <summary>Delete key was pressed.</summary>
    public bool Delete { get; set; }

    /// <summary>Meta key was pressed.</summary>
    public bool Meta { get; set; }

    /// <summary>Super key (Cmd on Mac, Win on Windows) was pressed. Only available with kitty keyboard protocol.</summary>
    public bool Super { get; set; }

    /// <summary>Hyper key was pressed. Only available with kitty keyboard protocol.</summary>
    public bool Hyper { get; set; }

    /// <summary>Caps Lock is active. Only available with kitty keyboard protocol.</summary>
    public bool CapsLock { get; set; }

    /// <summary>Num Lock is active. Only available with kitty keyboard protocol.</summary>
    public bool NumLock { get; set; }

    /// <summary>Event type for key events. Only available with kitty keyboard protocol.</summary>
    public InputEventType? EventType { get; set; }

    /// <summary>
    /// Create a <see cref="KeyInfo"/> from a <see cref="ParsedKey"/>.
    /// <para>
    /// Corresponds to the key construction logic in JS <c>useInput</c> hook.
    /// </para>
    /// </summary>
    public static (string Input, KeyInfo Key) FromParsedKey(ParsedKey keypress)
    {
        var key = new KeyInfo
        {
            UpArrow = keypress.Name == "up",
            DownArrow = keypress.Name == "down",
            LeftArrow = keypress.Name == "left",
            RightArrow = keypress.Name == "right",
            PageDown = keypress.Name == "pagedown",
            PageUp = keypress.Name == "pageup",
            Home = keypress.Name == "home",
            End = keypress.Name == "end",
            Return = keypress.Name == "return",
            Escape = keypress.Name == "escape",
            Ctrl = keypress.Ctrl,
            Shift = keypress.Shift,
            Tab = keypress.Name == "tab",
            Backspace = keypress.Name == "backspace",
            Delete = keypress.Name == "delete",
            // `parseKeypress` parses \u001B\u001B[A (meta + up arrow) as meta = false
            // but with option = true, so we need to take this into account here
            Meta = keypress.Meta || keypress.Name == "escape" || keypress.Option,
            // Kitty keyboard protocol modifiers
            Super = keypress.Super ?? false,
            Hyper = keypress.Hyper ?? false,
            CapsLock = keypress.CapsLock ?? false,
            NumLock = keypress.NumLock ?? false,
        };

        // Map kitty event type
        if (keypress.EventType.HasValue)
        {
            key.EventType = keypress.EventType.Value switch
            {
                KeyEventType.Press => InputEventType.Press,
                KeyEventType.Repeat => InputEventType.Repeat,
                KeyEventType.Release => InputEventType.Release,
                _ => null,
            };
        }

        // Determine input string
        string input;
        if (keypress.IsKittyProtocol == true)
        {
            if (keypress.IsPrintable == true)
            {
                input = keypress.Text ?? keypress.Name;
            }
            else if (keypress.Ctrl && keypress.Name.Length == 1)
            {
                input = keypress.Name;
            }
            else
            {
                input = "";
            }
        }
        else if (keypress.Ctrl)
        {
            input = keypress.Name ?? "";
        }
        else
        {
            input = keypress.Sequence;
        }

        if (keypress.IsKittyProtocol != true && keypress.Name is not null && KeypressParser.NonAlphanumericKeys.Contains(keypress.Name))
        {
            input = "";
        }

        // Strip meta if it's still remaining after parseKeypress
        if (input.StartsWith('\u001B'))
        {
            input = input[1..];
        }

        if (input.Length == 1 && char.IsUpper(input[0]))
        {
            key.Shift = true;
        }

        return (input, key);
    }
}
