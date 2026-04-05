// -----------------------------------------------------------------------
// <copyright file="ClickEvent.cs" company="Ink.Net">
//   Click event with screen coordinates. Used for mouse interaction support.
// </copyright>
// -----------------------------------------------------------------------

using Ink.Net.Dom;

namespace Ink.Net.Events;

/// <summary>
/// Click event with screen coordinates. Used for mouse interaction support.
/// </summary>
public sealed class ClickEvent : InkEvent
{
    /// <summary>Gets the screen column (0-indexed) where the click occurred.</summary>
    public int X { get; }

    /// <summary>Gets the screen row (0-indexed) where the click occurred.</summary>
    public int Y { get; }

    /// <summary>Gets the mouse button index (0 = primary, 1 = middle, 2 = secondary).</summary>
    public int Button { get; }

    /// <summary>Gets or sets the target element that was clicked.</summary>
    public DomElement? Target { get; internal set; }

    /// <summary>
    /// Initializes a new <see cref="ClickEvent"/> with the specified coordinates.
    /// </summary>
    /// <param name="x">Screen column (0-indexed).</param>
    /// <param name="y">Screen row (0-indexed).</param>
    /// <param name="button">Mouse button index (default: 0).</param>
    public ClickEvent(int x, int y, int button = 0) : base("click")
    {
        X = x;
        Y = y;
        Button = button;
    }
}
