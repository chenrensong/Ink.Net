// -----------------------------------------------------------------------
// <copyright file="WindowSizeMonitor.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) use-window-size.ts
//   Monitors terminal window size changes and raises events.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Terminal;

/// <summary>
/// Terminal window dimensions.
/// <para>Corresponds to JS <c>WindowSize</c> type.</para>
/// </summary>
public readonly struct WindowSize : IEquatable<WindowSize>
{
    /// <summary>Number of columns (horizontal character cells).</summary>
    public int Columns { get; }

    /// <summary>Number of rows (vertical character cells).</summary>
    public int Rows { get; }

    public WindowSize(int columns, int rows)
    {
        Columns = columns;
        Rows = rows;
    }

    public bool Equals(WindowSize other) => Columns == other.Columns && Rows == other.Rows;
    public override bool Equals(object? obj) => obj is WindowSize ws && Equals(ws);
    public override int GetHashCode() => HashCode.Combine(Columns, Rows);
    public static bool operator ==(WindowSize left, WindowSize right) => left.Equals(right);
    public static bool operator !=(WindowSize left, WindowSize right) => !left.Equals(right);
    public override string ToString() => $"{Columns}x{Rows}";
}

/// <summary>
/// Monitors terminal window size and raises events on resize.
/// <para>
/// C# equivalent of the JS <c>useWindowSize</c> hook.
/// Uses a polling timer since .NET does not natively support <c>SIGWINCH</c> on all platforms.
/// </para>
/// </summary>
public sealed class WindowSizeMonitor : IDisposable
{
    private WindowSize _currentSize;
    private Timer? _pollingTimer;
    private bool _disposed;
    private readonly int _pollingIntervalMs;
    private readonly object _lock = new();

    /// <summary>
    /// Raised when the terminal window size changes.
    /// <para>Corresponds to JS <c>stdout.on('resize', ...)</c>.</para>
    /// </summary>
    public event Action<WindowSize>? Resized;

    /// <summary>
    /// Creates a new <see cref="WindowSizeMonitor"/>.
    /// </summary>
    /// <param name="pollingIntervalMs">Interval in ms to poll terminal size. Default 200ms.</param>
    public WindowSizeMonitor(int pollingIntervalMs = 200)
    {
        _pollingIntervalMs = pollingIntervalMs;
        var (cols, rows) = TerminalUtils.GetWindowSize();
        _currentSize = new WindowSize(cols, rows);
    }

    /// <summary>
    /// Current terminal window size.
    /// <para>Corresponds to JS <c>useWindowSize()</c> return value.</para>
    /// </summary>
    public WindowSize Size
    {
        get
        {
            lock (_lock) return _currentSize;
        }
    }

    /// <summary>
    /// Start polling for terminal size changes.
    /// </summary>
    public void Start()
    {
        if (_disposed || _pollingTimer != null) return;

        _pollingTimer = new Timer(_ => CheckSize(), null, _pollingIntervalMs, _pollingIntervalMs);
    }

    /// <summary>
    /// Stop polling for terminal size changes.
    /// </summary>
    public void Stop()
    {
        _pollingTimer?.Dispose();
        _pollingTimer = null;
    }

    /// <summary>
    /// Manually update the size (useful for testing or when size is known externally).
    /// </summary>
    public void SetSize(int columns, int rows)
    {
        lock (_lock)
        {
            var newSize = new WindowSize(columns, rows);
            if (_currentSize != newSize)
            {
                _currentSize = newSize;
                Resized?.Invoke(newSize);
            }
        }
    }

    private void CheckSize()
    {
        if (_disposed) return;

        try
        {
            var (cols, rows) = TerminalUtils.GetWindowSize();
            SetSize(cols, rows);
        }
        catch
        {
            // Ignore errors during polling
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
    }
}
