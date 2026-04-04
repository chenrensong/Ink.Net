// -----------------------------------------------------------------------
// <copyright file="InkApp.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) ink.tsx / render.ts / render-to-string.ts
//   Main orchestration: tree build → layout → render → terminal output loop.
// </copyright>
// -----------------------------------------------------------------------

using Facebook.Yoga;
using Ink.Net.Builder;
using Ink.Net.Dom;
using Ink.Net.Rendering;
using Ink.Net.Styles;
using Ink.Net.Terminal;
using static Facebook.Yoga.YGNodeAPI;
using static Facebook.Yoga.YGNodeStyleAPI;
using static Facebook.Yoga.YGNodeLayoutAPI;

namespace Ink.Net;

/// <summary>
/// Options for <see cref="InkApp.Render"/>.
/// <para>Corresponds to JS <c>RenderOptions</c> in <c>render.ts</c>.</para>
/// </summary>
public sealed class RenderOptions
{
    /// <summary>Output writer. Default: <see cref="Console.Out"/>.</summary>
    public TextWriter? Stdout { get; init; }

    /// <summary>If true, each update outputs separately (debug mode). Default false.</summary>
    public bool Debug { get; init; }

    /// <summary>Max FPS for render throttle. Default 30.</summary>
    public int MaxFps { get; init; } = 30;

    /// <summary>Use incremental rendering. Default false.</summary>
    public bool IncrementalRendering { get; init; }

    /// <summary>Terminal columns. Default auto-detect.</summary>
    public int? Columns { get; init; }

    /// <summary>Terminal rows. Default auto-detect.</summary>
    public int? Rows { get; init; }

    /// <summary>Enable screen reader output. Default false.</summary>
    public bool IsScreenReaderEnabled { get; init; }
}

/// <summary>
/// Options for <see cref="InkApp.RenderToString"/>.
/// <para>Corresponds to JS <c>RenderToStringOptions</c>.</para>
/// </summary>
public sealed class RenderToStringOptions
{
    /// <summary>Width of the virtual terminal in columns. Default 80.</summary>
    public int Columns { get; init; } = 80;

    /// <summary>Enable screen reader output. Default false.</summary>
    public bool IsScreenReaderEnabled { get; init; }
}

/// <summary>
/// Instance handle returned by <see cref="InkApp.Render"/>.
/// Allows rerendering, clearing, and unmounting.
/// <para>Corresponds to JS <c>Instance</c> in <c>render.ts</c>.</para>
/// </summary>
public sealed class InkInstance : IDisposable
{
    private readonly InkApp _app;
    private bool _disposed;

    internal InkInstance(InkApp app) => _app = app;

    /// <summary>Re-render the UI with a new tree.</summary>
    public void Rerender(Func<TreeBuilder, TreeNode[]> buildFunc) => _app.Update(buildFunc);

    /// <summary>Clear all output from the terminal.</summary>
    public void Clear() => _app.Clear();

    /// <summary>Unmount the app and restore the terminal.</summary>
    public void Unmount() => Dispose();

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _app.Unmount();
    }
}

/// <summary>
/// Main Ink.Net application class — orchestrates tree building, layout, rendering, and terminal output.
/// <para>1:1 port of Ink JS <c>ink.tsx</c> / <c>render.ts</c> / <c>render-to-string.ts</c>.</para>
/// </summary>
public sealed class InkApp
{
    private readonly TextWriter _stdout;
    private LogUpdate? _logUpdate;
    private DomElement? _rootNode;
    private readonly RenderOptions _options;
    private readonly TreeBuilder _builder = new();
    private bool _unmounted;

    private InkApp(RenderOptions options)
    {
        _options = options;
        _stdout = options.Stdout ?? Console.Out;
    }

    // ─── Public static API ───────────────────────────────────────────

    /// <summary>
    /// Render a tree to the terminal (live).
    /// <para>Corresponds to JS <c>render(node, options)</c>.</para>
    /// </summary>
    /// <param name="buildFunc">Builder function that produces root children.</param>
    /// <param name="options">Render options.</param>
    /// <returns>An <see cref="InkInstance"/> for controlling the app.</returns>
    public static InkInstance Render(Func<TreeBuilder, TreeNode[]> buildFunc, RenderOptions? options = null)
    {
        options ??= new RenderOptions();
        var app = new InkApp(options);
        app.Initialize(buildFunc);
        return new InkInstance(app);
    }

    /// <summary>
    /// Render a tree to a string synchronously (no terminal output).
    /// <para>Corresponds to JS <c>renderToString(node, options)</c>.</para>
    /// </summary>
    /// <param name="buildFunc">Builder function that produces root children.</param>
    /// <param name="options">Options (columns).</param>
    /// <returns>The rendered string.</returns>
    public static string RenderToString(Func<TreeBuilder, TreeNode[]> buildFunc, RenderToStringOptions? options = null)
    {
        options ??= new RenderToStringOptions();
        int columns = options.Columns;

        var builder = new TreeBuilder();
        var children = buildFunc(builder);
        // Pass null for rows → auto-height (content-determined), same as JS renderToString
        var root = builder.Build(children, columns, rows: null);

        var result = InkRenderer.Render(root, isScreenReaderEnabled: options.IsScreenReaderEnabled);

        // Cleanup Yoga nodes
        DomTree.CleanupYogaNode(root.YogaNode);

        // Combine static and dynamic output (same as JS renderToString)
        string staticOutput = result.StaticOutput;
        if (staticOutput.EndsWith('\n'))
            staticOutput = staticOutput[..^1];

        if (!string.IsNullOrEmpty(staticOutput) && !string.IsNullOrEmpty(result.Output))
            return staticOutput + "\n" + result.Output;

        return string.IsNullOrEmpty(staticOutput) ? result.Output : staticOutput;
    }

    /// <summary>
    /// Convenience overload: render a single tree node to string.
    /// </summary>
    public static string RenderToString(Func<TreeBuilder, TreeNode> buildFunc, RenderToStringOptions? options = null)
    {
        return RenderToString(b => new[] { buildFunc(b) }, options);
    }

    // ─── Internal ────────────────────────────────────────────────────

    private void Initialize(Func<TreeBuilder, TreeNode[]> buildFunc)
    {
        var (columns, rows) = GetDimensions();

        _logUpdate = LogUpdate.Create(_stdout, new LogUpdateOptions
        {
            ShowCursor = false,
            Incremental = _options.IncrementalRendering,
        });

        var children = buildFunc(_builder);
        _rootNode = _builder.Build(children, columns, rows);

        // Perform initial render
        DoRender();
    }

    internal void Update(Func<TreeBuilder, TreeNode[]> buildFunc)
    {
        if (_unmounted) return;

        var (columns, rows) = GetDimensions();
        var children = buildFunc(_builder);

        // Cleanup old tree
        if (_rootNode?.YogaNode is not null)
        {
            DomTree.CleanupYogaNode(_rootNode.YogaNode);
        }

        _rootNode = _builder.Build(children, columns, rows);
        DoRender();
    }

    private void DoRender()
    {
        if (_rootNode is null || _logUpdate is null) return;

        var result = InkRenderer.Render(_rootNode, _options.IsScreenReaderEnabled);

        if (!string.IsNullOrEmpty(result.StaticOutput))
        {
            _logUpdate.Clear();
            _stdout.Write(result.StaticOutput);
            _logUpdate.Reset();
        }

        if (_options.Debug)
        {
            _stdout.Write(result.Output + "\n");
        }
        else
        {
            _logUpdate.Render(result.Output);
        }
    }

    internal void Clear()
    {
        _logUpdate?.Clear();
    }

    internal void Unmount()
    {
        if (_unmounted) return;
        _unmounted = true;

        _logUpdate?.Done();

        // Cleanup Yoga nodes
        if (_rootNode?.YogaNode is not null)
        {
            DomTree.CleanupYogaNode(_rootNode.YogaNode);
        }

        _rootNode = null;
    }

    private (int Columns, int Rows) GetDimensions()
    {
        int columns = _options.Columns ?? TerminalUtils.GetWindowSize().Columns;
        int rows = _options.Rows ?? TerminalUtils.GetWindowSize().Rows;
        return (columns, rows);
    }
}
