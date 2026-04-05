// -----------------------------------------------------------------------
// <copyright file="InkApplication.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) App.tsx — Main application coordinator.
//   Integrates all subsystems: TreeBuilder, Rendering, Focus, Input, Paste,
//   Cursor, WindowSize, AppLifecycle into a single interactive application.
// </copyright>
// -----------------------------------------------------------------------

using System.Text;
using Ink.Net.Builder;
using Ink.Net.Dom;
using Ink.Net.Input;
using Ink.Net.Rendering;
using Ink.Net.Terminal;

namespace Ink.Net;

/// <summary>
/// Options for creating an <see cref="InkApplication"/>.
/// </summary>
public sealed class InkApplicationOptions
{
    /// <summary>Output writer. Default: <see cref="Console.Out"/>.</summary>
    public TextWriter? Stdout { get; init; }

    /// <summary>Error writer. Default: <see cref="Console.Error"/>.</summary>
    public TextWriter? Stderr { get; init; }

    /// <summary>Whether to exit on Ctrl+C. Default true.</summary>
    public bool ExitOnCtrlC { get; init; } = true;

    /// <summary>Terminal columns override. Default auto-detect.</summary>
    public int? Columns { get; init; }

    /// <summary>Terminal rows override. Default auto-detect.</summary>
    public int? Rows { get; init; }

    /// <summary>If true, each update outputs separately (debug mode). Default false.</summary>
    public bool Debug { get; init; }

    /// <summary>Use incremental rendering. Default false.</summary>
    public bool IncrementalRendering { get; init; }

    /// <summary>Whether the terminal supports raw mode. Default true.</summary>
    public bool IsRawModeSupported { get; init; } = true;

    /// <summary>Enable screen reader output. Default false.</summary>
    public bool IsScreenReaderEnabled { get; init; }

    /// <summary>
    /// Render the app in the terminal's alternate screen buffer.
    /// When enabled, the app renders on a separate screen, and the original
    /// terminal content is restored when the app exits.
    /// <para>Only works in interactive mode. Default false.</para>
    /// <para>Corresponds to JS <c>render({ alternateScreen: true })</c>.</para>
    /// </summary>
    public bool AlternateScreen { get; init; }

    /// <summary>
    /// Maximum frames per second for render throttling.
    /// <para>Set to 0 to disable throttling. Default 30.</para>
    /// <para>Corresponds to JS <c>render({ maxFps: 30 })</c>.</para>
    /// </summary>
    public int MaxFps { get; init; } = 30;
}

/// <summary>
/// Main interactive Ink.Net application — integrates all subsystems into a single coordinator.
/// <para>
/// C# equivalent of JS <c>App.tsx</c> + <c>ink.tsx</c>.
/// Provides a complete lifecycle for interactive terminal applications with:
/// <list type="bullet">
/// <item>Input handling (<see cref="Input"/>)</item>
/// <item>Paste handling (<see cref="Paste"/>)</item>
/// <item>Focus management (<see cref="Focus"/>)</item>
/// <item>Cursor management (<see cref="Cursor"/>)</item>
/// <item>Window size monitoring (<see cref="WindowSize"/>)</item>
/// <item>App lifecycle (<see cref="Lifecycle"/>)</item>
/// </list>
/// </para>
/// </summary>
public sealed class InkApplication : IDisposable
{
    private readonly TextWriter _stdout;
    private readonly TextWriter _stderr;
    private readonly InkApplicationOptions _options;
    private readonly TreeBuilder _builder = new();
    private LogUpdate? _logUpdate;
    private DomElement? _rootNode;
    private Func<TreeBuilder, TreeNode[]>? _buildFunc;
    private bool _disposed;
    private readonly bool _synchronize;

    // Throttle state (mirrors JS es-toolkit throttle behavior)
    private readonly System.Timers.Timer? _throttleTimer;
    private bool _hasPendingThrottledRender;
    private DateTime _lastRenderTime = DateTime.MinValue;

    // ─── Subsystems ──────────────────────────────────────────────────

    /// <summary>
    /// App lifecycle management (exit, waitUntilExit).
    /// <para>Corresponds to JS <c>useApp()</c>.</para>
    /// </summary>
    public AppLifecycle Lifecycle { get; }

    /// <summary>
    /// Keyboard input handler.
    /// <para>Corresponds to JS <c>useInput()</c>.</para>
    /// </summary>
    public Input.InputHandler Input { get; }

    /// <summary>
    /// Clipboard paste handler.
    /// <para>Corresponds to JS <c>usePaste()</c>.</para>
    /// </summary>
    public PasteHandler Paste { get; }

    /// <summary>
    /// Focus management system.
    /// <para>Corresponds to JS <c>useFocus()</c> / <c>useFocusManager()</c>.</para>
    /// </summary>
    public FocusManager Focus { get; }

    /// <summary>
    /// Terminal cursor management.
    /// <para>Corresponds to JS <c>useCursor()</c>.</para>
    /// </summary>
    public CursorManager Cursor { get; }

    /// <summary>
    /// Terminal window size monitoring.
    /// <para>Corresponds to JS <c>useWindowSize()</c>.</para>
    /// </summary>
    public Terminal.WindowSizeMonitor WindowSize { get; }

    /// <summary>
    /// Stdin provider for accessing the input stream and raw mode.
    /// <para>Corresponds to JS <c>useStdin()</c>.</para>
    /// </summary>
    public StdinProvider Stdin { get; }

    /// <summary>
    /// Stdout provider for writing to stdout while preserving Ink output.
    /// <para>Corresponds to JS <c>useStdout()</c>.</para>
    /// </summary>
    public StdoutProvider Stdout { get; }

    /// <summary>
    /// Stderr provider for writing to stderr while preserving Ink output.
    /// <para>Corresponds to JS <c>useStderr()</c>.</para>
    /// </summary>
    public StderrProvider Stderr { get; }

    /// <summary>
    /// Alternate screen manager for rendering in the terminal's alternate screen buffer.
    /// <para>Corresponds to JS <c>render({ alternateScreen: true })</c>.</para>
    /// </summary>
    public AlternateScreen Screen { get; }

    /// <summary>
    /// Whether screen reader support is enabled.
    /// <para>Corresponds to JS <c>useIsScreenReaderEnabled()</c>.</para>
    /// </summary>
    public bool IsScreenReaderEnabled { get; }

    // ─── Constructor ─────────────────────────────────────────────────

    private InkApplication(InkApplicationOptions options)
    {
        _options = options;
        _stdout = options.Stdout ?? Console.Out;
        _stderr = options.Stderr ?? Console.Error;
        _synchronize = SynchronizedWrite.ShouldSynchronize(Console.IsOutputRedirected);

        // Initialize throttle timer (disabled initially, started in Start())
        System.Timers.Timer? timer = null;
        if (!_options.Debug && !options.IsScreenReaderEnabled && options.MaxFps > 0)
        {
            int renderThrottleMs = Math.Max(1, (int)Math.Ceiling(1000.0 / options.MaxFps));
            timer = new System.Timers.Timer(renderThrottleMs);
            timer.AutoReset = false; // One-shot timer for throttle
        }
        _throttleTimer = timer;

        IsScreenReaderEnabled = options.IsScreenReaderEnabled;
        Lifecycle = new AppLifecycle();
        Input = new Input.InputHandler { ExitOnCtrlC = options.ExitOnCtrlC };
        Paste = new PasteHandler(_stdout);
        Focus = new FocusManager();
        Cursor = new CursorManager();
        WindowSize = new Terminal.WindowSizeMonitor();
        Stdin = new StdinProvider(isRawModeSupported: options.IsRawModeSupported);
        Stdout = new StdoutProvider(_stdout);
        Stderr = new StderrProvider(_stderr);
        Screen = new AlternateScreen(_stdout);

        // Wire up subsystem events (same as JS App.tsx)
        Input.CtrlCPressed += OnCtrlC;
        Input.PasteReceived += text => Paste.HandlePaste(text);
        Input.RawInput += rawInput => Focus.HandleInput(rawInput);
        Lifecycle.Exiting += OnExit;
        WindowSize.Resized += OnResize;

        // Wire StdoutProvider write to LogUpdate clear/restore
        Stdout.WriteRequested += OnWriteToStdout;
        Stderr.WriteRequested += OnWriteToStderr;
    }

    // ─── Static factory ──────────────────────────────────────────────

    /// <summary>
    /// Create and start an interactive Ink application.
    /// <para>Corresponds to JS <c>render(element, options)</c> with interactive features.</para>
    /// </summary>
    /// <param name="buildFunc">Builder function that produces root children.</param>
    /// <param name="options">Application options.</param>
    /// <returns>The running <see cref="InkApplication"/>.</returns>
    public static InkApplication Create(Func<TreeBuilder, TreeNode[]> buildFunc, InkApplicationOptions? options = null)
    {
        options ??= new InkApplicationOptions();
        var app = new InkApplication(options);
        app.Start(buildFunc);
        return app;
    }

    // ─── Public API ──────────────────────────────────────────────────

    /// <summary>
    /// Re-render the UI with the current or a new build function.
    /// </summary>
    public void Rerender(Func<TreeBuilder, TreeNode[]>? buildFunc = null)
    {
        if (_disposed) return;

        if (buildFunc != null)
            _buildFunc = buildFunc;

        // Use throttle if enabled, otherwise render immediately
        if (_throttleTimer is null)
        {
            DoRender();
            return;
        }

        // Leading+trailing throttle behavior (like es-toolkit)
        var now = DateTime.UtcNow;
        var msSinceLastRender = (now - _lastRenderTime).TotalMilliseconds;
        var throttleMs = _throttleTimer.Interval;

        if (msSinceLastRender >= throttleMs)
        {
            // Enough time has passed, render immediately (leading edge)
            _lastRenderTime = now;
            DoRender();
        }
        else
        {
            // Too soon, mark as pending (trailing edge will handle it)
            _hasPendingThrottledRender = true;
        }
    }

    /// <summary>
    /// Feed raw input data from stdin.
    /// <para>Call this with data read from <c>Console.In</c> or a raw stdin stream.</para>
    /// </summary>
    public void HandleInput(string data)
    {
        if (_disposed) return;
        Input.HandleData(data);
    }

    /// <summary>
    /// Wait for the application to exit.
    /// </summary>
    public Task<object?> WaitUntilExit() => Lifecycle.WaitUntilExit();

    /// <summary>
    /// Clear all output from the terminal.
    /// </summary>
    public void Clear()
    {
        _logUpdate?.Clear();
    }

    // ─── Internal ────────────────────────────────────────────────────

    private void Start(Func<TreeBuilder, TreeNode[]> buildFunc)
    {
        _buildFunc = buildFunc;

        var (columns, rows) = GetDimensions();

        _logUpdate = LogUpdate.Create(_stdout, new LogUpdateOptions
        {
            ShowCursor = false,
            Incremental = _options.IncrementalRendering,
            Synchronize = _synchronize,
        });

        // Enter alternate screen if requested (same as JS ink.tsx constructor)
        if (AlternateScreen.ShouldEnable(_options.AlternateScreen, interactive: true))
        {
            Screen.Enter();
        }

        // Start window size monitoring
        WindowSize.Start();

        // Set up render throttle (same as JS ink.tsx)
        // Unthrottled if debug or screen reader mode
        bool unthrottled = _options.Debug || IsScreenReaderEnabled;
        int maxFps = _options.MaxFps;
        int renderThrottleMs = maxFps > 0 ? Math.Max(1, (int)Math.Ceiling(1000.0 / maxFps)) : 0;

        if (unthrottled || renderThrottleMs <= 0)
        {
            _throttleTimer?.Dispose();
        }
        else
        {
            // Set up throttle timer with leading+trailing behavior (like es-toolkit)
            _throttleTimer!.Interval = renderThrottleMs;
            _throttleTimer!.Elapsed += (s, e) => OnThrottledRender();
            _throttleTimer!.Start();
        }

        // Initial render
        DoRender();
    }

    private void OnThrottledRender()
    {
        if (_disposed || _throttleTimer is null) return;

        // Only render if there's a pending render
        if (_hasPendingThrottledRender)
        {
            _hasPendingThrottledRender = false;
            _lastRenderTime = DateTime.UtcNow;
            DoRender();
        }

        // Re-arm timer for next trailing edge
        _throttleTimer.Stop();
        _throttleTimer.Start();
    }

    private void FlushThrottledRender()
    {
        if (_throttleTimer is null || _disposed) return;

        // Flush any pending throttled render
        _throttleTimer.Stop();
        if (_hasPendingThrottledRender)
        {
            _hasPendingThrottledRender = false;
            DoRender();
        }
        _throttleTimer.Start();
    }

    // ─── Write handlers (for useStdout / useStderr) ──────────────────

    private string _lastOutput = "";

    private void OnWriteToStdout(string data)
    {
        if (_disposed) return;

        if (_options.Debug)
        {
            _stdout.Write(data + _lastOutput);
            return;
        }

        if (_synchronize) _stdout.Write(SynchronizedWrite.Bsu);
        _logUpdate?.Clear();
        _stdout.Write(data);
        RestoreLastOutput();
        if (_synchronize) _stdout.Write(SynchronizedWrite.Esu);
    }

    private void OnWriteToStderr(string data)
    {
        if (_disposed) return;

        if (_options.Debug)
        {
            _stderr.Write(data);
            _stdout.Write(_lastOutput);
            return;
        }

        if (_synchronize) _stdout.Write(SynchronizedWrite.Bsu);
        _logUpdate?.Clear();
        _stderr.Write(data);
        RestoreLastOutput();
        if (_synchronize) _stdout.Write(SynchronizedWrite.Esu);
    }

    private void RestoreLastOutput()
    {
        if (_logUpdate is null) return;

        // Re-render last output to restore the Ink display
        _logUpdate.SetCursorPosition(Cursor.Position);
        if (!string.IsNullOrEmpty(_lastOutput))
        {
            _logUpdate.Render(_lastOutput + "\n");
        }
    }

    private void DoRender()
    {
        if (_buildFunc == null || _logUpdate == null) return;

        var (columns, rows) = GetDimensions();

        // Cleanup old tree
        if (_rootNode?.YogaNode is not null)
        {
            DomTree.CleanupYogaNode(_rootNode.YogaNode);
        }

        var children = _buildFunc(_builder);
        _rootNode = _builder.Build(children, columns, rows);

        var result = InkRenderer.Render(_rootNode, IsScreenReaderEnabled);

        if (!string.IsNullOrEmpty(result.StaticOutput))
        {
            _logUpdate.Clear();
            _stdout.Write(result.StaticOutput);
            _logUpdate.Reset();
        }

        _lastOutput = result.Output;

        if (_options.Debug)
        {
            _stdout.Write(result.Output + "\n");
        }
        else
        {
            _logUpdate.Render(result.Output);
        }

        // Notify render flush
        Lifecycle.NotifyRenderFlushed();
    }

    private void OnCtrlC()
    {
        Lifecycle.Exit();
    }

    private void OnExit(object? _)
    {
        Dispose();
    }

    private void OnResize(Terminal.WindowSize newSize)
    {
        // Re-render with new dimensions - flush immediately for resize
        if (_throttleTimer is null || !_hasPendingThrottledRender)
        {
            DoRender();
        }
    }

    private (int Columns, int Rows) GetDimensions()
    {
        int columns = _options.Columns ?? WindowSize.Size.Columns;
        int rows = _options.Rows ?? WindowSize.Size.Rows;
        return (columns, rows);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Flush any pending throttled render before exit (like JS ink.tsx)
        if (_throttleTimer is not null)
        {
            _throttleTimer.Stop();
            if (_hasPendingThrottledRender)
            {
                _hasPendingThrottledRender = false;
                DoRender();
            }
            _throttleTimer.Dispose();
        }

        WindowSize.Dispose();
        Input.Dispose();
        Paste.Dispose();
        Lifecycle.Dispose();

        _logUpdate?.Done();

        // Exit alternate screen buffer if active (same as JS ink.tsx finishUnmount)
        Screen.Dispose();

        if (_rootNode?.YogaNode is not null)
        {
            DomTree.CleanupYogaNode(_rootNode.YogaNode);
        }

        _rootNode = null;
    }
}
