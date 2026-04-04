// -----------------------------------------------------------------------
// <copyright file="BoxMetrics.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) hooks/use-box-metrics.ts
//   Provides reactive measurement of box element layout metrics.
// </copyright>
// -----------------------------------------------------------------------

using Ink.Net.Dom;
using static Facebook.Yoga.YGNodeLayoutAPI;

namespace Ink.Net.Rendering;

/// <summary>
/// Layout metrics of a box element. All positions are relative to the element's parent.
/// <para>Corresponds to JS <c>BoxMetrics</c> type.</para>
/// </summary>
public readonly record struct BoxMetricsData(int Width, int Height, int Left, int Top)
{
    /// <summary>Empty metrics (all zeros).</summary>
    public static readonly BoxMetricsData Empty = new(0, 0, 0, 0);
}

/// <summary>
/// Result of measuring a box element, including whether it has been measured.
/// <para>Corresponds to JS <c>UseBoxMetricsResult</c>.</para>
/// </summary>
public readonly record struct BoxMetricsResult(int Width, int Height, int Left, int Top, bool HasMeasured);

/// <summary>
/// Measures and monitors the layout metrics of a DOM element.
/// <para>
/// Corresponds to JS <c>useBoxMetrics(ref)</c> hook.
/// Unlike <c>measureElement</c> which is a one-shot measurement,
/// BoxMetrics monitors for changes and fires events when metrics change.
/// </para>
/// </summary>
public sealed class BoxMetricsTracker : IDisposable
{
    private DomElement? _element;
    private BoxMetricsData _currentMetrics = BoxMetricsData.Empty;
    private bool _hasMeasured;
    private bool _disposed;

    /// <summary>
    /// Create a new metrics tracker for the given element.
    /// </summary>
    public BoxMetricsTracker(DomElement? element = null)
    {
        if (element is not null)
            Track(element);
    }

    /// <summary>
    /// Start tracking a specific element.
    /// </summary>
    public void Track(DomElement element)
    {
        _element = element;
        UpdateMetrics();
    }

    /// <summary>
    /// Stop tracking the current element.
    /// </summary>
    public void Untrack()
    {
        _element = null;
        _currentMetrics = BoxMetricsData.Empty;
        _hasMeasured = false;
    }

    /// <summary>
    /// Get the current metrics.
    /// </summary>
    public BoxMetricsResult GetMetrics()
    {
        UpdateMetrics();
        return new BoxMetricsResult(
            _currentMetrics.Width,
            _currentMetrics.Height,
            _currentMetrics.Left,
            _currentMetrics.Top,
            _hasMeasured);
    }

    /// <summary>
    /// Update metrics from the Yoga layout.
    /// Call this after layout calculation to refresh values.
    /// </summary>
    public bool UpdateMetrics()
    {
        if (_element?.YogaNode is null)
        {
            if (_hasMeasured)
            {
                _currentMetrics = BoxMetricsData.Empty;
                _hasMeasured = false;
                MetricsChanged?.Invoke(new BoxMetricsResult(0, 0, 0, 0, false));
                return true;
            }
            return false;
        }

        var yogaNode = _element.YogaNode;
        var newMetrics = new BoxMetricsData(
            (int)YGNodeLayoutGetWidth(yogaNode),
            (int)YGNodeLayoutGetHeight(yogaNode),
            (int)YGNodeLayoutGetLeft(yogaNode),
            (int)YGNodeLayoutGetTop(yogaNode));

        bool changed = newMetrics != _currentMetrics || !_hasMeasured;

        if (changed)
        {
            _currentMetrics = newMetrics;
            _hasMeasured = true;
            MetricsChanged?.Invoke(new BoxMetricsResult(
                newMetrics.Width, newMetrics.Height,
                newMetrics.Left, newMetrics.Top, true));
        }

        return changed;
    }

    /// <summary>
    /// Event fired when metrics change.
    /// <para>Corresponds to JS resize/layout listener pattern in <c>useBoxMetrics</c>.</para>
    /// </summary>
    public event Action<BoxMetricsResult>? MetricsChanged;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _element = null;
    }
}
