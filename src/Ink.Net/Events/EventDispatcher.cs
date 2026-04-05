// -----------------------------------------------------------------------
// <copyright file="EventDispatcher.cs" company="Ink.Net">
//   DOM-like event dispatcher supporting capture and bubble phases.
// </copyright>
// -----------------------------------------------------------------------

using Ink.Net.Dom;

namespace Ink.Net.Events;

/// <summary>
/// DOM-like event dispatcher supporting capture and bubble phases.
/// Events travel from root → target (capture) then target → root (bubble).
/// </summary>
public sealed class EventDispatcher
{
    /// <summary>
    /// Dispatch an event to a target element, following capture/bubble phases.
    /// </summary>
    /// <param name="target">The target element.</param>
    /// <param name="evt">The event to dispatch.</param>
    /// <returns>True if the event was not prevented.</returns>
    public bool Dispatch(DomElement target, InkEvent evt)
    {
        // Build path from root to target
        var path = new List<DomElement>();
        var current = target;
        while (current != null)
        {
            path.Add(current);
            current = current.ParentNode as DomElement;
        }
        path.Reverse(); // Now root → ... → target

        // Capture phase: root → target
        for (int i = 0; i < path.Count; i++)
        {
            if (evt.PropagationStopped) break;
            path[i].InvokeEventHandlers(evt, capture: true);
        }

        // Bubble phase: target → root (if event bubbles)
        if (evt.Bubbles && !evt.PropagationStopped)
        {
            for (int i = path.Count - 1; i >= 0; i--)
            {
                if (evt.PropagationStopped) break;
                path[i].InvokeEventHandlers(evt, capture: false);
            }
        }

        return !evt.DefaultPrevented;
    }
}
