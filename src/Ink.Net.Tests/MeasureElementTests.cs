// -----------------------------------------------------------------------
// 1:1 Port from Ink (JS) test/measure-element.tsx
// Adapted for imperative TreeBuilder API (no React hooks/refs).
// -----------------------------------------------------------------------

using Ink.Net.Builder;
using Ink.Net.Dom;
using Ink.Net.Rendering;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for <see cref="MeasureElement"/>.</summary>
public class MeasureElementTests
{
    [Fact]
    public void MeasureElementWidth()
    {
        // Build a box with text and measure it (100 columns → full width)
        var builder = new TreeBuilder();
        var box = builder.Box(children: new[]
        {
            builder.Text("Width: 0"),
        });
        var root = builder.Build(new[] { box }, columns: 100);

        // After layout, measure the box's inner node
        var boxNode = (DomElement)root.ChildNodes[0];
        var dims = MeasureElement.Measure(boxNode);

        Assert.Equal(100, dims.Width);
    }

    [Fact]
    public void MeasureElementHeight()
    {
        var builder = new TreeBuilder();
        var box = builder.Box(
            style: new InkStyle { FlexDirection = FlexDirectionMode.Column },
            children: new[]
            {
                builder.Text("line 1"),
                builder.Text("line 2"),
                builder.Text("line 3"),
            });
        var root = builder.Build(new[] { box }, columns: 100);

        var boxNode = (DomElement)root.ChildNodes[0];
        var dims = MeasureElement.Measure(boxNode);

        Assert.Equal(3, dims.Height);
    }

    [Fact]
    public void MeasureElementWithFixedWidth()
    {
        var builder = new TreeBuilder();
        var box = builder.Box(
            style: new InkStyle { Width = 50 },
            children: new[]
            {
                builder.Text("Content"),
            });
        var root = builder.Build(new[] { box }, columns: 100);

        var boxNode = (DomElement)root.ChildNodes[0];
        var dims = MeasureElement.Measure(boxNode);

        Assert.Equal(50, dims.Width);
    }

    [Fact]
    public void MeasureElementReturnsZeroForNullYogaNode()
    {
        // VirtualText nodes have no Yoga node
        var node = DomTree.CreateNode(InkNodeType.VirtualText);
        var dims = MeasureElement.Measure(node);

        Assert.Equal(0, dims.Width);
        Assert.Equal(0, dims.Height);
    }

    [Fact]
    public void MeasureNestedElement()
    {
        var builder = new TreeBuilder();
        var innerBox = builder.Box(
            style: new InkStyle { FlexDirection = FlexDirectionMode.Column },
            children: new[]
            {
                builder.Text("line 1"),
                builder.Text("line 2"),
                builder.Text("line 3"),
            });

        var outerBox = builder.Box(
            style: new InkStyle { FlexDirection = FlexDirectionMode.Column },
            children: new[]
            {
                innerBox,
                builder.Text("Height: ?"),
            });

        var root = builder.Build(new[] { outerBox }, columns: 100);

        // Measure the inner box
        var outerNode = (DomElement)root.ChildNodes[0];
        var innerNode = (DomElement)outerNode.ChildNodes[0];
        var dims = MeasureElement.Measure(innerNode);

        Assert.Equal(3, dims.Height);
    }

    [Fact]
    public void MeasureAfterRelayout()
    {
        // Build tree with one line, measure, then rebuild with three lines and measure again
        var builder = new TreeBuilder();

        // First layout: 1 line
        var box1 = builder.Box(
            style: new InkStyle { FlexDirection = FlexDirectionMode.Column },
            children: new[]
            {
                builder.Text("line 1"),
            });
        var root1 = builder.Build(new[] { box1 }, columns: 100);
        var boxNode1 = (DomElement)root1.ChildNodes[0];
        Assert.Equal(1, MeasureElement.Measure(boxNode1).Height);

        DomTree.CleanupYogaNode(root1.YogaNode);

        // Second layout: 3 lines
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
        Assert.Equal(3, MeasureElement.Measure(boxNode2).Height);

        DomTree.CleanupYogaNode(root2.YogaNode);
    }
}
