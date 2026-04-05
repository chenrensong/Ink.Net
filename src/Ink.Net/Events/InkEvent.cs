// -----------------------------------------------------------------------
// <copyright file="InkEvent.cs" company="Ink.Net">
//   Base class for all Ink events. Mirrors the DOM Event model.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Events;

/// <summary>
/// Base class for all Ink events. Mirrors the DOM Event model used by the source ink engine.
/// Supports capture/bubble phases and stop propagation.
/// </summary>
public abstract class InkEvent
{
    /// <summary>Gets the event type name (e.g. "click", "input").</summary>
    public string Type { get; }

    /// <summary>Gets or initializes whether this event bubbles up through the DOM.</summary>
    public bool Bubbles { get; init; } = true;

    /// <summary>Gets whether <see cref="PreventDefault"/> has been called.</summary>
    public bool DefaultPrevented { get; private set; }

    /// <summary>Gets whether <see cref="StopPropagation"/> has been called.</summary>
    public bool PropagationStopped { get; private set; }

    /// <summary>Gets whether <see cref="StopImmediatePropagation"/> has been called.</summary>
    public bool ImmediatePropagationStopped { get; private set; }

    /// <summary>
    /// Initializes a new instance of <see cref="InkEvent"/> with the specified type.
    /// </summary>
    /// <param name="type">The event type name.</param>
    protected InkEvent(string type)
    {
        Type = type;
    }

    /// <summary>Prevent the default action for this event.</summary>
    public void PreventDefault() => DefaultPrevented = true;

    /// <summary>Stop the event from propagating further.</summary>
    public void StopPropagation() => PropagationStopped = true;

    /// <summary>Stop propagation and prevent any remaining handlers on the current target.</summary>
    public void StopImmediatePropagation()
    {
        ImmediatePropagationStopped = true;
        PropagationStopped = true;
    }
}
