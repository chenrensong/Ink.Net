// -----------------------------------------------------------------------
// <copyright file="InputEvent.cs" company="Ink.Net">
//   Generic input event for raw text input.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Events;

/// <summary>
/// Generic input event for raw text input.
/// </summary>
public sealed class InputEvent : InkEvent
{
    /// <summary>Gets the raw input text data.</summary>
    public string Data { get; }

    /// <summary>
    /// Initializes a new <see cref="InputEvent"/> with the specified text data.
    /// </summary>
    /// <param name="data">The raw input text.</param>
    public InputEvent(string data) : base("input")
    {
        Data = data;
    }
}
