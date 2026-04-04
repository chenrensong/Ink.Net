// -----------------------------------------------------------------------
// <copyright file="AppLifecycle.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) use-app.ts + AppContext.ts
//   Provides app lifecycle management: exit, waitUntilExit, waitUntilRenderFlush.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net;

/// <summary>
/// Application lifecycle manager.
/// <para>
/// C# equivalent of the JS <c>useApp()</c> hook and <c>AppContext</c>.
/// Provides methods to exit the app and wait for completion.
/// </para>
/// </summary>
public sealed class AppLifecycle : IDisposable
{
    private readonly TaskCompletionSource<object?> _exitTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _renderFlushTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _exited;
    private bool _disposed;

    /// <summary>
    /// Raised when <see cref="Exit"/> is called.
    /// </summary>
    public event Action<object?>? Exiting;

    /// <summary>
    /// Whether the app has exited.
    /// </summary>
    public bool HasExited => _exited;

    /// <summary>
    /// Exit (unmount) the whole Ink app.
    /// <para>
    /// - <c>Exit()</c> — resolves <see cref="WaitUntilExit"/> with <c>null</c>.
    /// - <c>Exit(new Exception("…"))</c> — makes <see cref="WaitUntilExit"/> throw.
    /// - <c>Exit(value)</c> — resolves <see cref="WaitUntilExit"/> with <c>value</c>.
    /// </para>
    /// <para>Corresponds to JS <c>AppContext.exit(errorOrResult)</c>.</para>
    /// </summary>
    public void Exit(object? errorOrResult = null)
    {
        if (_exited) return;
        _exited = true;

        Exiting?.Invoke(errorOrResult);

        if (errorOrResult is Exception ex)
        {
            _exitTcs.TrySetException(ex);
        }
        else
        {
            _exitTcs.TrySetResult(errorOrResult);
        }

        // Also complete the render flush
        _renderFlushTcs.TrySetResult();
    }

    /// <summary>
    /// Returns a task that completes when the app exits.
    /// <para>Corresponds to JS <c>waitUntilExit()</c>.</para>
    /// </summary>
    public Task<object?> WaitUntilExit() => _exitTcs.Task;

    /// <summary>
    /// Returns a task that settles after pending render output is flushed.
    /// <para>Corresponds to JS <c>waitUntilRenderFlush()</c>.</para>
    /// </summary>
    public Task WaitUntilRenderFlush() => _renderFlushTcs.Task;

    /// <summary>
    /// Signal that the render has been flushed (called by the rendering system).
    /// </summary>
    public void NotifyRenderFlushed()
    {
        _renderFlushTcs.TrySetResult();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (!_exited)
        {
            Exit();
        }
    }
}
