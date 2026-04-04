// -----------------------------------------------------------------------
// <copyright file="PasteHandler.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) use-paste.ts
//   Provides clipboard paste handling via bracketed paste mode.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Input;

/// <summary>
/// Delegate for handling paste events.
/// </summary>
/// <param name="text">The pasted text content.</param>
public delegate void PasteCallback(string text);

/// <summary>
/// Registration handle for a paste handler. Dispose to unregister.
/// </summary>
public sealed class PasteRegistration : IDisposable
{
    private readonly PasteHandler _handler;
    private readonly PasteCallback _callback;
    private bool _disposed;

    internal PasteRegistration(PasteHandler handler, PasteCallback callback)
    {
        _handler = handler;
        _callback = callback;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _handler.Unregister(_callback);
    }
}

/// <summary>
/// Manages clipboard paste handling for an Ink application.
/// <para>
/// C# equivalent of the JS <c>usePaste</c> hook.
/// Automatically enables/disables bracketed paste mode (<c>\x1b[?2004h</c> / <c>\x1b[?2004l</c>).
/// </para>
/// </summary>
public sealed class PasteHandler : IDisposable
{
    private readonly TextWriter _stdout;
    private readonly List<PasteCallback> _handlers = new();
    private readonly object _lock = new();
    private int _enabledCount;
    private bool _disposed;

    /// <summary>
    /// Creates a new <see cref="PasteHandler"/>.
    /// </summary>
    /// <param name="stdout">The stdout stream to write bracketed paste mode escape sequences.</param>
    public PasteHandler(TextWriter stdout)
    {
        _stdout = stdout;
    }

    /// <summary>
    /// Whether bracketed paste mode is currently enabled.
    /// </summary>
    public bool IsBracketedPasteModeEnabled => _enabledCount > 0;

    /// <summary>
    /// Register a paste handler callback. Returns a registration that can be disposed to unregister.
    /// <para>
    /// When the first handler is registered, bracketed paste mode is enabled.
    /// When the last handler is unregistered, bracketed paste mode is disabled.
    /// </para>
    /// <para>Corresponds to JS <c>usePaste(handler)</c>.</para>
    /// </summary>
    public PasteRegistration Register(PasteCallback callback)
    {
        lock (_lock)
        {
            _handlers.Add(callback);
        }

        EnableBracketedPasteMode(true);
        return new PasteRegistration(this, callback);
    }

    /// <summary>
    /// Unregister a paste handler callback.
    /// </summary>
    internal void Unregister(PasteCallback callback)
    {
        lock (_lock)
        {
            _handlers.Remove(callback);
        }

        EnableBracketedPasteMode(false);
    }

    /// <summary>
    /// Handle pasted text (called by <see cref="InputHandler"/> when paste is detected).
    /// </summary>
    public void HandlePaste(string text)
    {
        if (_disposed) return;

        PasteCallback[] snapshot;
        lock (_lock)
        {
            snapshot = _handlers.ToArray();
        }

        foreach (var handler in snapshot)
        {
            handler(text);
        }
    }

    /// <summary>
    /// Enable or disable bracketed paste mode reference counting.
    /// <para>Corresponds to JS <c>handleSetBracketedPasteMode</c> in <c>App.tsx</c>.</para>
    /// </summary>
    public void EnableBracketedPasteMode(bool enable)
    {
        if (enable)
        {
            if (_enabledCount == 0)
            {
                _stdout.Write("\u001B[?2004h");
            }

            _enabledCount++;
        }
        else
        {
            if (_enabledCount == 0) return;

            if (--_enabledCount == 0)
            {
                _stdout.Write("\u001B[?2004l");
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_enabledCount > 0)
        {
            _stdout.Write("\u001B[?2004l");
            _enabledCount = 0;
        }

        lock (_lock)
        {
            _handlers.Clear();
        }
    }
}
