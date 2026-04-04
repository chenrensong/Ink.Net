// -----------------------------------------------------------------------
// 1:1 Port from Ink (JS) test/use-box-metrics.tsx
// Adapted for imperative TreeBuilder + BoxMetricsTracker API.
// -----------------------------------------------------------------------

using Ink.Net.Builder;
using Ink.Net.Dom;
using Ink.Net.Rendering;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for <see cref="BoxMetricsTracker"/>.</summary>
public class BoxMetricsTests
{
    [Fact]
    public void ReturnsCorrectSizeOnFirstRender()
    {
        // JS: width fills terminal (100); single-line text → height 1
        var builder = new TreeBuilder();
        var box = builder.Box(children: new[]
        {
            builder.Text("Hello"),
        });
        var root = builder.Build(new[] { box }, columns: 100);

        var boxNode = (DomElement)root.ChildNodes[0];
        var tracker = new BoxMetricsTracker(boxNode);
        var metrics = tracker.GetMetrics();

        Assert.Equal(100, metrics.Width);
        Assert.Equal(1, metrics.Height);
        Assert.True(metrics.HasMeasured);

        DomTree.CleanupYogaNode(root.YogaNode);
        tracker.Dispose();
    }

    [Fact]
    public void ReturnsCorrectPosition()
    {
        // JS: Box with marginLeft=5 in second row → left=5, top=1
        var builder = new TreeBuilder();
        var innerBox = builder.Box(
            style: new InkStyle { MarginLeft = 5 },
            children: new[] { builder.Text("pos") });

        var root = builder.Build(new[]
        {
            builder.Box(style: new InkStyle { FlexDirection = FlexDirectionMode.Column }, children: new[]
            {
                builder.Text("first line"),
                innerBox,
            }),
        }, columns: 100);

        var outerNode = (DomElement)root.ChildNodes[0];
        var innerNode = (DomElement)outerNode.ChildNodes[1];
        var tracker = new BoxMetricsTracker(innerNode);
        var metrics = tracker.GetMetrics();

        Assert.Equal(5, metrics.Left);
        Assert.Equal(1, metrics.Top);
        Assert.True(metrics.HasMeasured);

        DomTree.CleanupYogaNode(root.YogaNode);
        tracker.Dispose();
    }

    [Fact]
    public void UpdateMetricsDetectsChange()
    {
        // Build with one width, then rebuild with different width
        var builder = new TreeBuilder();
        var box1 = builder.Box(children: new[] { builder.Text("Hello") });
        var root1 = builder.Build(new[] { box1 }, columns: 100);
        var boxNode1 = (DomElement)root1.ChildNodes[0];

        var tracker = new BoxMetricsTracker(boxNode1);
        var m1 = tracker.GetMetrics();
        Assert.Equal(100, m1.Width);

        DomTree.CleanupYogaNode(root1.YogaNode);

        // Rebuild with different column count (simulating resize)
        var box2 = builder.Box(children: new[] { builder.Text("Hello") });
        var root2 = builder.Build(new[] { box2 }, columns: 60);
        var boxNode2 = (DomElement)root2.ChildNodes[0];

        tracker.Track(boxNode2);
        var m2 = tracker.GetMetrics();
        Assert.Equal(60, m2.Width);
        Assert.True(m2.HasMeasured);

        DomTree.CleanupYogaNode(root2.YogaNode);
        tracker.Dispose();
    }

    [Fact]
    public void MetricsChangedEventFires()
    {
        var builder = new TreeBuilder();
        var box = builder.Box(children: new[] { builder.Text("Hello") });
        var root = builder.Build(new[] { box }, columns: 100);
        var boxNode = (DomElement)root.ChildNodes[0];

        var tracker = new BoxMetricsTracker();
        BoxMetricsResult? receivedMetrics = null;
        tracker.MetricsChanged += m => receivedMetrics = m;

        tracker.Track(boxNode);
        tracker.UpdateMetrics();

        Assert.NotNull(receivedMetrics);
        Assert.Equal(100, receivedMetrics.Value.Width);
        Assert.True(receivedMetrics.Value.HasMeasured);

        DomTree.CleanupYogaNode(root.YogaNode);
        tracker.Dispose();
    }

    [Fact]
    public void ReturnsZerosWhenNotTracking()
    {
        var tracker = new BoxMetricsTracker();
        var metrics = tracker.GetMetrics();

        Assert.Equal(0, metrics.Width);
        Assert.Equal(0, metrics.Height);
        Assert.Equal(0, metrics.Left);
        Assert.Equal(0, metrics.Top);
        Assert.False(metrics.HasMeasured);

        tracker.Dispose();
    }

    [Fact]
    public void UntrackResetsMetrics()
    {
        var builder = new TreeBuilder();
        var box = builder.Box(
            style: new InkStyle { Width = 10 },
            children: new[] { builder.Text("1234567890") });
        var root = builder.Build(new[] { box }, columns: 100);
        var boxNode = (DomElement)root.ChildNodes[0];

        var tracker = new BoxMetricsTracker(boxNode);
        var m1 = tracker.GetMetrics();
        Assert.Equal(10, m1.Width);
        Assert.True(m1.HasMeasured);

        tracker.Untrack();
        var m2 = tracker.GetMetrics();
        Assert.Equal(0, m2.Width);
        Assert.Equal(0, m2.Height);
        Assert.False(m2.HasMeasured);

        DomTree.CleanupYogaNode(root.YogaNode);
        tracker.Dispose();
    }

    [Fact]
    public void NoChangeReturnsFalse()
    {
        var builder = new TreeBuilder();
        var box = builder.Box(children: new[] { builder.Text("Hello") });
        var root = builder.Build(new[] { box }, columns: 100);
        var boxNode = (DomElement)root.ChildNodes[0];

        var tracker = new BoxMetricsTracker(boxNode);
        // First call returns true (initial measurement)
        bool changed1 = tracker.UpdateMetrics();
        // Second call with same layout should return false
        bool changed2 = tracker.UpdateMetrics();

        Assert.False(changed2);

        DomTree.CleanupYogaNode(root.YogaNode);
        tracker.Dispose();
    }

    [Fact]
    public void HeightUpdatesWhenContentChanges()
    {
        var builder = new TreeBuilder();

        // 1 line
        var box1 = builder.Box(
            style: new InkStyle { FlexDirection = FlexDirectionMode.Column },
            children: new[] { builder.Text("short") });
        var root1 = builder.Build(new[] { box1 }, columns: 100);
        var boxNode1 = (DomElement)root1.ChildNodes[0];

        var tracker = new BoxMetricsTracker(boxNode1);
        Assert.Equal(1, tracker.GetMetrics().Height);

        DomTree.CleanupYogaNode(root1.YogaNode);

        // 3 lines
        var box2 = builder.Box(
            style: new InkStyle { FlexDirection = FlexDirectionMode.Column },
            children: new[]
            {
                builder.Text("line 1"),
                builder.Text("line 2"),
                builder.Text("line 3"),
            });
        var root2 = builder.Build(new[] { box2 }, columns: 100);
        var boxNode2 = (DomElement)root2.ChildNodes[0];

        tracker.Track(boxNode2);
        Assert.Equal(3, tracker.GetMetrics().Height);

        DomTree.CleanupYogaNode(root2.YogaNode);
        tracker.Dispose();
    }
}
