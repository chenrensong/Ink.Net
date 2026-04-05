// -----------------------------------------------------------------------
// <copyright file="KeyboardEvent.cs" company="Ink.Net">
//   Keyboard event wrapping a parsed KeyInfo.
// </copyright>
// -----------------------------------------------------------------------

using Ink.Net.Input;

namespace Ink.Net.Events;

/// <summary>
/// Keyboard event wrapping a parsed <see cref="KeyInfo"/>.
/// </summary>
public sealed class KeyboardEvent : InkEvent
{
    /// <summary>Gets the parsed key information.</summary>
    public KeyInfo Key { get; }

    /// <summary>Gets the raw escape sequence that produced this event.</summary>
    public string RawSequence { get; }

    /// <summary>
    /// Initializes a new <see cref="KeyboardEvent"/> with the specified key info.
    /// </summary>
    /// <param name="type">The event type (e.g. "keydown", "keyup").</param>
    /// <param name="key">The parsed key information.</param>
    /// <param name="rawSequence">The raw escape sequence (default: empty).</param>
    public KeyboardEvent(string type, KeyInfo key, string rawSequence = "") : base(type)
    {
        Key = key;
        RawSequence = rawSequence;
    }
}
