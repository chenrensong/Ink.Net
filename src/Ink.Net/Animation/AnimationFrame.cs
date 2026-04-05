// -----------------------------------------------------------------------
// <copyright file="AnimationFrame.cs" company="Ink.Net">
//   Animation frame callbacks, similar to useAnimationFrame hook.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Animation;

/// <summary>
/// Provides animation frame callbacks, similar to useAnimationFrame hook.
/// Subscribes to a <see cref="Clock"/> with keepAlive=true.
/// </summary>
public sealed class AnimationFrame : IDisposable
{
    private readonly Clock _clock;
    private IDisposable? _subscription;
    private Action<long>? _callback;
    private bool _active;

    /// <summary>
    /// Initializes a new <see cref="AnimationFrame"/> bound to the specified clock.
    /// </summary>
    /// <param name="clock">The clock to subscribe to.</param>
    public AnimationFrame(Clock clock)
    {
        _clock = clock;
    }

    /// <summary>
    /// Start the animation loop with the given callback.
    /// The callback receives elapsed time in milliseconds.
    /// </summary>
    /// <param name="callback">Callback invoked each frame with elapsed milliseconds.</param>
    public void Start(Action<long> callback)
    {
        if (_active) Stop();
        _callback = callback;
        _active = true;
        _subscription = _clock.Subscribe(() => _callback?.Invoke(_clock.Now()), keepAlive: true);
    }

    /// <summary>Stop the animation loop.</summary>
    public void Stop()
    {
        _active = false;
        _subscription?.Dispose();
        _subscription = null;
        _callback = null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Stop();
    }
}
