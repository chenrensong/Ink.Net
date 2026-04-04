// Tests ported from flex-justify-content.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Flex justify-content tests aligned with JS flex-justify-content.tsx test suite.</summary>
public class FlexJustifyContentTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    [Fact]
    public void RowAlignTextToCenter()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { JustifyContent = JustifyContentMode.Center, Width = 10 }, new[]
            {
                b.Text("Test"),
            })
        }, Opts100);

        Assert.Equal("   Test", output);
    }

    [Fact]
    public void RowAlignMultipleTextNodesToCenter()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { JustifyContent = JustifyContentMode.Center, Width = 10 }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("    AB", output);
    }

    [Fact]
    public void RowAlignTextToRight()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { JustifyContent = JustifyContentMode.FlexEnd, Width = 10 }, new[]
            {
                b.Text("Test"),
            })
        }, Opts100);

        Assert.Equal("      Test", output);
    }

    [Fact]
    public void RowAlignMultipleTextNodesToRight()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { JustifyContent = JustifyContentMode.FlexEnd, Width = 10 }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("        AB", output);
    }

    [Fact]
    public void RowAlignTwoTextNodesOnTheEdges()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { JustifyContent = JustifyContentMode.SpaceBetween, Width = 4 }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A  B", output);
    }

    [Fact]
    public void RowSpaceEvenlyTwoTextNodes()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { JustifyContent = JustifyContentMode.SpaceEvenly, Width = 10 }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("  A   B", output);
    }

    [Fact]
    public void ColumnAlignTextToCenter()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle
            {
                FlexDirection = FlexDirectionMode.Column,
                JustifyContent = JustifyContentMode.Center,
                Height = 3,
            }, new[]
            {
                b.Text("Test"),
            })
        }, Opts100);

        Assert.Equal("\nTest\n", output);
    }

    [Fact]
    public void ColumnAlignTextToBottom()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle
            {
                FlexDirection = FlexDirectionMode.Column,
                JustifyContent = JustifyContentMode.FlexEnd,
                Height = 3,
            }, new[]
            {
                b.Text("Test"),
            })
        }, Opts100);

        Assert.Equal("\n\nTest", output);
    }

    [Fact]
    public void ColumnAlignTwoTextNodesOnTheEdges()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle
            {
                FlexDirection = FlexDirectionMode.Column,
                JustifyContent = JustifyContentMode.SpaceBetween,
                Height = 4,
            }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A\n\n\nB", output);
    }
}
