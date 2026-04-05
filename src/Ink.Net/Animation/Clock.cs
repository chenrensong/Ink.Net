// -----------------------------------------------------------------------
// <copyright file="Clock.cs" company="Ink.Net">
//   Shared animation clock providing synchronized ticking for all subscribers.
//   Corresponds to JS ClockContext/createClock.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Animation;

/// <summary>
/// Shared animation clock that provides synchronized ticking for all subscribers.
/// Corresponds to JS ClockContext/createClock.
/// </summary>
public sealed class Clock : IDisposable
{
    private readonly Dictionary<Action, bool> _subscribers = new();
    private Timer? _timer;
    private int _tickIntervalMs;
    private long _startTime;
    private long _tickTime;
    private bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="Clock"/> with the specified tick interval.
    /// </summary>
    /// <param name="tickIntervalMs">Tick interval in milliseconds (default: ~30fps).</param>
    public Clock(int tickIntervalMs = 33)
    {
        _tickIntervalMs = tickIntervalMs;
    }

    /// <summary>
    /// Subscribe to clock ticks.
    /// </summary>
    /// <param name="onChange">Callback invoked on each tick.</param>
    /// <param name="keepAlive">If true, keeps the clock running even when other subscribers don't need it.</param>
    /// <returns>Disposable to unsubscribe.</returns>
    public IDisposable Subscribe(Action onChange, bool keepAlive = false)
    {
        _subscribers[onChange] = keepAlive;
        UpdateInterval();
        return new Unsubscriber(this, onChange);
    }

    /// <summary>
    /// Get the current elapsed time in milliseconds, synchronized within a tick.
    /// </summary>
    /// <returns>Elapsed milliseconds since the clock started.</returns>
    public long Now()
    {
        if (_startTime == 0)
            _startTime = Environment.TickCount64;

        if (_timer != null && _tickTime > 0)
            return _tickTime;

        return Environment.TickCount64 - _startTime;
    }

    /// <summary>Set the tick interval in milliseconds.</summary>
    /// <param name="ms">New tick interval.</param>
    public void SetTickInterval(int ms)
    {
        if (ms == _tickIntervalMs) return;
        _tickIntervalMs = ms;
        UpdateInterval();
    }

    private void Tick(object? state)
    {
        if (_disposed) return;
        _tickTime = Environment.TickCount64 - _startTime;
        foreach (var onChange in _subscribers.Keys.ToArray())
        {
            onChange();
        }
    }

    private void UpdateInterval()
    {
        bool anyKeepAlive = _subscribers.Values.Any(v => v);
        if (anyKeepAlive)
        {
            _timer?.Dispose();
            if (_startTime == 0)
                _startTime = Environment.TickCount64;
            _timer = new Timer(Tick, null, 0, _tickIntervalMs);
        }
        else
        {
            _timer?.Dispose();
            _timer = null;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer?.Dispose();
        _subscribers.Clear();
    }

    private sealed class Unsubscriber : IDisposable
    {
        private Clock? _clock;
        private Action? _onChange;

        public Unsubscriber(Clock clock, Action onChange)
        {
            _clock = clock;
            _onChange = onChange;
        }

        public void Dispose()
        {
            if (_clock is null) return;
            _clock._subscribers.Remove(_onChange!);
            _clock.UpdateInterval();
            _clock = null;
            _onChange = null;
        }
    }
}
