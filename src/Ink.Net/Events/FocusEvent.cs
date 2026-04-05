// -----------------------------------------------------------------------
// <copyright file="FocusEvent.cs" company="Ink.Net">
//   Focus/blur event for DOM-like focus management.
// </copyright>
// -----------------------------------------------------------------------

using Ink.Net.Dom;

namespace Ink.Net.Events;

/// <summary>
/// Focus/blur event for DOM-like focus management.
/// </summary>
public sealed class FocusEvent : InkEvent
{
    /// <summary>Gets the element that is gaining or losing focus to/from.</summary>
    public DomElement? RelatedTarget { get; }

    /// <summary>
    /// Initializes a new <see cref="FocusEvent"/> with the specified type.
    /// </summary>
    /// <param name="type">The event type (e.g. "focus" or "blur").</param>
    /// <param name="relatedTarget">The element gaining/losing focus to/from.</param>
    public FocusEvent(string type, DomElement? relatedTarget = null) : base(type)
    {
        RelatedTarget = relatedTarget;
    }
}
