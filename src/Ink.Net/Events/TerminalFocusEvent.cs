// -----------------------------------------------------------------------
// <copyright file="TerminalFocusEvent.cs" company="Ink.Net">
//   Event fired when terminal window gains/loses focus.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Events;

/// <summary>
/// Event fired when the terminal window gains or loses focus.
/// </summary>
public sealed class TerminalFocusEvent : InkEvent
{
    /// <summary>Gets whether the terminal is currently focused.</summary>
    public bool IsFocused { get; }

    /// <summary>
    /// Initializes a new <see cref="TerminalFocusEvent"/>.
    /// </summary>
    /// <param name="isFocused">True if the terminal gained focus; false if it lost focus.</param>
    public TerminalFocusEvent(bool isFocused) : base(isFocused ? "terminalfocus" : "terminalblur")
    {
        IsFocused = isFocused;
    }
}
