// -----------------------------------------------------------------------
// <copyright file="DeclaredCursor.cs" company="Ink.Net">
//   Declared cursor position system. Components declare where the terminal
//   cursor should be placed. Corresponds to JS useDeclaredCursor.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Terminal;

/// <summary>
/// Declared cursor position system. Components declare where the terminal
/// cursor should be placed (e.g., text input caret). The renderer picks
/// the active declaration and positions the cursor after rendering.
/// Corresponds to JS useDeclaredCursor.
/// </summary>
public sealed class DeclaredCursor
{
    private readonly List<CursorDeclaration> _declarations = new();
    private CursorDeclaration? _active;

    /// <summary>Gets the currently active cursor declaration, or null if none.</summary>
    public CursorDeclaration? Active => _active;

    /// <summary>
    /// Declare a cursor position. Returns a handle that can be updated or disposed.
    /// </summary>
    /// <param name="column">The cursor column (0-indexed).</param>
    /// <param name="row">The cursor row (0-indexed).</param>
    /// <param name="visible">Whether the cursor should be visible.</param>
    /// <returns>A <see cref="CursorDeclaration"/> handle.</returns>
    public CursorDeclaration Declare(int column, int row, bool visible = true)
    {
        var decl = new CursorDeclaration(this, column, row, visible);
        _declarations.Add(decl);
        UpdateActive();
        return decl;
    }

    /// <summary>Remove a cursor declaration.</summary>
    internal void Remove(CursorDeclaration decl)
    {
        _declarations.Remove(decl);
        UpdateActive();
    }

    /// <summary>Re-evaluate which declaration is active (last visible wins).</summary>
    internal void UpdateActive()
    {
        // Last visible declaration wins (stack behavior, like focus)
        _active = null;
        for (int i = _declarations.Count - 1; i >= 0; i--)
        {
            if (_declarations[i].Visible)
            {
                _active = _declarations[i];
                break;
            }
        }
    }
}

/// <summary>
/// A cursor position declaration from a component.
/// </summary>
public sealed class CursorDeclaration : IDisposable
{
    private readonly DeclaredCursor _owner;
    private int _column;
    private int _row;
    private bool _visible;
    private bool _disposed;

    /// <summary>Gets the declared cursor column (0-indexed).</summary>
    public int Column => _column;

    /// <summary>Gets the declared cursor row (0-indexed).</summary>
    public int Row => _row;

    /// <summary>Gets whether the cursor should be visible.</summary>
    public bool Visible => _visible;

    /// <summary>
    /// Initializes a new <see cref="CursorDeclaration"/>.
    /// </summary>
    internal CursorDeclaration(DeclaredCursor owner, int column, int row, bool visible)
    {
        _owner = owner;
        _column = column;
        _row = row;
        _visible = visible;
    }

    /// <summary>Update the cursor position.</summary>
    /// <param name="column">New cursor column.</param>
    /// <param name="row">New cursor row.</param>
    /// <param name="visible">Optionally update visibility.</param>
    public void Update(int column, int row, bool? visible = null)
    {
        _column = column;
        _row = row;
        if (visible.HasValue) _visible = visible.Value;
        _owner.UpdateActive();
    }

    /// <summary>Show or hide the cursor.</summary>
    /// <param name="visible">Whether the cursor should be visible.</param>
    public void SetVisible(bool visible)
    {
        _visible = visible;
        _owner.UpdateActive();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _owner.Remove(this);
    }
}
