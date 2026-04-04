// -----------------------------------------------------------------------
// <copyright file="InputHandler.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) use-input.ts + App.tsx input handling
//   Provides keyboard input handling with parsed KeyInfo events.
// </copyright>
// -----------------------------------------------------------------------

using System.Text;

namespace Ink.Net.Input;

/// <summary>
/// Delegate for handling keyboard input events.
/// <para>Corresponds to JS <c>useInput(handler)</c> callback signature.</para>
/// </summary>
/// <param name="input">The input string (character or empty for special keys).</param>
/// <param name="key">Structured key information.</param>
public delegate void InputCallback(string input, KeyInfo key);

/// <summary>
/// Registration handle for an input handler. Dispose to unregister.
/// </summary>
public sealed class InputRegistration : IDisposable
{
    private readonly InputHandler _handler;
    private readonly InputCallback _callback;
    private bool _disposed;

    internal InputRegistration(InputHandler handler, InputCallback callback)
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
/// Manages keyboard input handling for an Ink application.
/// <para>
/// C# equivalent of the JS <c>useInput</c> hook combined with <c>App.tsx</c> stdin handling.
/// Parses raw terminal input into structured <see cref="KeyInfo"/> events.
/// </para>
/// </summary>
public sealed class InputHandler : IDisposable
{
    private readonly InputParser _inputParser = new();
    private readonly List<InputCallback> _handlers = new();
    private readonly object _lock = new();
    private bool _exitOnCtrlC = true;
    private bool _disposed;

    // Pending escape flush timer
    private Timer? _pendingFlushTimer;
    private const int PendingInputFlushDelayMs = 20;

    /// <summary>
    /// Whether to exit the app on Ctrl+C. Default true.
    /// <para>Corresponds to JS <c>exitOnCtrlC</c> option.</para>
    /// </summary>
    public bool ExitOnCtrlC
    {
        get => _exitOnCtrlC;
        set => _exitOnCtrlC = value;
    }

    /// <summary>
    /// Raised for raw input events (before parsing into KeyInfo).
    /// Used internally by FocusManager for Tab navigation.
    /// </summary>
    internal event Action<string>? RawInput;

    /// <summary>
    /// Raised when Ctrl+C is pressed and <see cref="ExitOnCtrlC"/> is true.
    /// </summary>
    public event Action? CtrlCPressed;

    /// <summary>
    /// Raised when paste text is received via bracketed paste mode.
    /// </summary>
    public event Action<string>? PasteReceived;

    /// <summary>
    /// Register an input handler callback. Returns a registration that can be disposed to unregister.
    /// <para>Corresponds to JS <c>useInput(handler)</c>.</para>
    /// </summary>
    public InputRegistration Register(InputCallback callback)
    {
        lock (_lock)
        {
            _handlers.Add(callback);
        }

        return new InputRegistration(this, callback);
    }

    /// <summary>
    /// Unregister an input handler callback.
    /// </summary>
    internal void Unregister(InputCallback callback)
    {
        lock (_lock)
        {
            _handlers.Remove(callback);
        }
    }

    /// <summary>
    /// Feed raw terminal input data. This parses the data and dispatches events.
    /// <para>Corresponds to JS <c>handleReadable</c> in <c>App.tsx</c>.</para>
    /// </summary>
    public void HandleData(string data)
    {
        if (_disposed) return;

        ClearPendingFlush();

        var events = _inputParser.Push(data);
        foreach (var evt in events)
        {
            if (evt.IsPaste)
            {
                // Dispatch paste event
                PasteReceived?.Invoke(evt.Value);
            }
            else
            {
                EmitInput(evt.Value);
            }
        }

        if (_inputParser.HasPendingEscape())
        {
            SchedulePendingFlush();
        }
    }

    /// <summary>
    /// Reset the input parser state.
    /// </summary>
    public void Reset()
    {
        ClearPendingFlush();
        _inputParser.Reset();
    }

    private void EmitInput(string rawInput)
    {
        // Handle Ctrl+C
        if (rawInput == "\x03" && _exitOnCtrlC)
        {
            CtrlCPressed?.Invoke();
            return;
        }

        // Emit raw input for internal consumers (e.g. FocusManager)
        RawInput?.Invoke(rawInput);

        // Parse keypress and dispatch to handlers
        var keypress = KeypressParser.Parse(rawInput);
        var (input, key) = KeyInfo.FromParsedKey(keypress);

        // If exitOnCtrlC, skip handlers for Ctrl+C
        if (input == "c" && key.Ctrl && _exitOnCtrlC)
        {
            return;
        }

        InputCallback[] snapshot;
        lock (_lock)
        {
            snapshot = _handlers.ToArray();
        }

        foreach (var handler in snapshot)
        {
            handler(input, key);
        }
    }

    private void SchedulePendingFlush()
    {
        ClearPendingFlush();
        _pendingFlushTimer = new Timer(_ =>
        {
            _pendingFlushTimer = null;
            var pending = _inputParser.FlushPendingEscape();
            if (pending != null)
            {
                EmitInput(pending);
            }
        }, null, PendingInputFlushDelayMs, Timeout.Infinite);
    }

    private void ClearPendingFlush()
    {
        _pendingFlushTimer?.Dispose();
        _pendingFlushTimer = null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        ClearPendingFlush();
        lock (_lock)
        {
            _handlers.Clear();
        }
    }
}
