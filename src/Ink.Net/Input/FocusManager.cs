// -----------------------------------------------------------------------
// <copyright file="FocusManager.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) FocusContext.ts + use-focus.ts + use-focus-manager.ts
//   Manages focusable component registration, Tab navigation, and focus state.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Input;

/// <summary>
/// Options for adding a focusable component.
/// </summary>
public sealed class FocusOptions
{
    /// <summary>
    /// Whether the component is currently active (can receive focus). Default true.
    /// <para>Corresponds to JS <c>useFocus({ isActive })</c>.</para>
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Auto-focus this component if nothing else is focused. Default false.
    /// <para>Corresponds to JS <c>useFocus({ autoFocus })</c>.</para>
    /// </summary>
    public bool AutoFocus { get; init; }
}

/// <summary>
/// Represents a focus registration handle. Dispose to unregister the focusable.
/// </summary>
public sealed class FocusRegistration : IDisposable
{
    private readonly FocusManager _manager;
    private bool _disposed;

    /// <summary>
    /// The unique ID of this focusable.
    /// </summary>
    public string Id { get; }

    internal FocusRegistration(FocusManager manager, string id)
    {
        _manager = manager;
        Id = id;
    }

    /// <summary>
    /// Whether this focusable is currently focused.
    /// <para>Corresponds to JS <c>useFocus().isFocused</c>.</para>
    /// </summary>
    public bool IsFocused => !_disposed && _manager.ActiveId == Id;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _manager.Remove(Id);
    }
}

/// <summary>
/// Manages focusable component registration and Tab navigation.
/// <para>
/// C# equivalent of JS <c>FocusContext</c> + <c>useFocus</c> + <c>useFocusManager</c>.
/// </para>
/// </summary>
public sealed class FocusManager
{
    private readonly record struct Focusable(string Id, bool IsActive);

    private readonly List<Focusable> _focusables = new();
    private readonly object _lock = new();
    private string? _activeId;
    private bool _isFocusEnabled = true;

    // Tab characters (same as JS)
    private const string Tab = "\t";
    private const string ShiftTab = "\u001B[Z";

    /// <summary>
    /// Raised whenever the currently focused element changes.
    /// </summary>
    public event Action<string?>? ActiveIdChanged;

    /// <summary>
    /// The ID of the currently focused component, or <c>null</c> if none.
    /// <para>Corresponds to JS <c>FocusContext.activeId</c>.</para>
    /// </summary>
    public string? ActiveId
    {
        get
        {
            lock (_lock) return _activeId;
        }
    }

    /// <summary>
    /// Whether focus management is enabled.
    /// </summary>
    public bool IsFocusEnabled
    {
        get
        {
            lock (_lock) return _isFocusEnabled;
        }
    }

    /// <summary>
    /// Register a focusable component and return a <see cref="FocusRegistration"/>.
    /// <para>Corresponds to JS <c>useFocus({ id, autoFocus, isActive })</c>.</para>
    /// </summary>
    /// <param name="id">
    /// Optional custom ID. If null, a random ID is generated.
    /// Corresponds to JS <c>useFocus({ id })</c>.
    /// </param>
    /// <param name="options">Focus options (autoFocus, isActive).</param>
    public FocusRegistration Add(string? id = null, FocusOptions? options = null)
    {
        options ??= new FocusOptions();
        id ??= Guid.NewGuid().ToString("N")[..8];

        lock (_lock)
        {
            _focusables.Add(new Focusable(id, options.IsActive));

            if (options.AutoFocus && _activeId == null)
            {
                SetActiveId(id);
            }
        }

        return new FocusRegistration(this, id);
    }

    /// <summary>
    /// Remove a focusable component by ID.
    /// <para>Corresponds to JS <c>FocusContext.remove(id)</c>.</para>
    /// </summary>
    public void Remove(string id)
    {
        lock (_lock)
        {
            if (_activeId == id)
            {
                SetActiveId(null);
            }

            _focusables.RemoveAll(f => f.Id == id);
        }
    }

    /// <summary>
    /// Activate a focusable component (make it eligible for focus).
    /// <para>Corresponds to JS <c>FocusContext.activate(id)</c>.</para>
    /// </summary>
    public void Activate(string id)
    {
        lock (_lock)
        {
            var idx = _focusables.FindIndex(f => f.Id == id);
            if (idx >= 0)
            {
                _focusables[idx] = _focusables[idx] with { IsActive = true };
            }
        }
    }

    /// <summary>
    /// Deactivate a focusable component (make it ineligible for focus).
    /// <para>Corresponds to JS <c>FocusContext.deactivate(id)</c>.</para>
    /// </summary>
    public void Deactivate(string id)
    {
        lock (_lock)
        {
            var idx = _focusables.FindIndex(f => f.Id == id);
            if (idx >= 0)
            {
                _focusables[idx] = _focusables[idx] with { IsActive = false };
            }

            if (_activeId == id)
            {
                SetActiveId(null);
            }
        }
    }

    /// <summary>
    /// Switch focus to the next focusable component.
    /// <para>Corresponds to JS <c>FocusContext.focusNext()</c>.</para>
    /// </summary>
    public void FocusNext()
    {
        lock (_lock)
        {
            var nextId = FindNextFocusable();
            if (nextId != null)
            {
                SetActiveId(nextId);
            }
        }
    }

    /// <summary>
    /// Switch focus to the previous focusable component.
    /// <para>Corresponds to JS <c>FocusContext.focusPrevious()</c>.</para>
    /// </summary>
    public void FocusPrevious()
    {
        lock (_lock)
        {
            var prevId = FindPreviousFocusable();
            if (prevId != null)
            {
                SetActiveId(prevId);
            }
        }
    }

    /// <summary>
    /// Focus a specific element by ID.
    /// <para>Corresponds to JS <c>FocusContext.focus(id)</c>.</para>
    /// </summary>
    public void Focus(string id)
    {
        lock (_lock)
        {
            if (_focusables.Any(f => f.Id == id))
            {
                SetActiveId(id);
            }
        }
    }

    /// <summary>
    /// Enable focus management for all components.
    /// <para>Corresponds to JS <c>useFocusManager().enableFocus()</c>.</para>
    /// </summary>
    public void EnableFocus()
    {
        lock (_lock)
        {
            _isFocusEnabled = true;
        }
    }

    /// <summary>
    /// Disable focus management. The currently active component (if any) loses focus.
    /// <para>Corresponds to JS <c>useFocusManager().disableFocus()</c>.</para>
    /// </summary>
    public void DisableFocus()
    {
        lock (_lock)
        {
            _isFocusEnabled = false;
            SetActiveId(null);
        }
    }

    /// <summary>
    /// Check if a specific component is currently focused.
    /// </summary>
    public bool IsFocused(string id)
    {
        lock (_lock)
        {
            return _activeId == id;
        }
    }

    /// <summary>
    /// Handle raw input for Tab/Shift-Tab navigation.
    /// <para>This is called by <see cref="InputHandler"/> via the <c>RawInput</c> event.</para>
    /// </summary>
    public void HandleInput(string rawInput)
    {
        lock (_lock)
        {
            if (!_isFocusEnabled || _focusables.Count == 0)
                return;

            if (rawInput == Tab)
            {
                var next = FindNextFocusable();
                if (next != null) SetActiveId(next);
            }
            else if (rawInput == ShiftTab)
            {
                var prev = FindPreviousFocusable();
                if (prev != null) SetActiveId(prev);
            }
        }
    }

    // ── helpers ──────────────────────────────────────────────────

    private void SetActiveId(string? id)
    {
        if (_activeId == id) return;
        _activeId = id;
        ActiveIdChanged?.Invoke(id);
    }

    /// <summary>
    /// Find the next active focusable after the current one.
    /// </summary>
    private string? FindNextFocusable()
    {
        var activeFocusables = _focusables.Where(f => f.IsActive).ToList();
        if (activeFocusables.Count == 0) return null;

        int currentIdx = _activeId != null
            ? activeFocusables.FindIndex(f => f.Id == _activeId)
            : -1;

        int nextIdx = (currentIdx + 1) % activeFocusables.Count;
        return activeFocusables[nextIdx].Id;
    }

    /// <summary>
    /// Find the previous active focusable before the current one.
    /// </summary>
    private string? FindPreviousFocusable()
    {
        var activeFocusables = _focusables.Where(f => f.IsActive).ToList();
        if (activeFocusables.Count == 0) return null;

        int currentIdx = _activeId != null
            ? activeFocusables.FindIndex(f => f.Id == _activeId)
            : 1;

        int prevIdx = (currentIdx - 1 + activeFocusables.Count) % activeFocusables.Count;
        return activeFocusables[prevIdx].Id;
    }
}
