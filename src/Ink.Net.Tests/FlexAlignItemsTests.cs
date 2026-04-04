// Tests ported from flex-align-items.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Flex align-items tests aligned with JS flex-align-items.tsx test suite.</summary>
public class FlexAlignItemsTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    [Fact]
    public void RowAlignTextToCenter()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { AlignItems = AlignItemsMode.Center, Height = 3 }, new[]
            {
                b.Text("Test"),
            })
        }, Opts100);

        Assert.Equal("\nTest\n", output);
    }

    [Fact]
    public void RowAlignMultipleTextNodesToCenter()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { AlignItems = AlignItemsMode.Center, Height = 3 }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("\nAB\n", output);
    }

    [Fact]
    public void RowAlignTextToBottom()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { AlignItems = AlignItemsMode.FlexEnd, Height = 3 }, new[]
            {
                b.Text("Test"),
            })
        }, Opts100);

        Assert.Equal("\n\nTest", output);
    }

    [Fact]
    public void RowAlignMultipleTextNodesToBottom()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { AlignItems = AlignItemsMode.FlexEnd, Height = 3 }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("\n\nAB", output);
    }

    [Fact]
    public void ColumnAlignTextToCenter()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.Center, Width = 10 }, new[]
            {
                b.Text("Test"),
            })
        }, Opts100);

        Assert.Equal("   Test", output);
    }

    [Fact]
    public void ColumnAlignTextToRight()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexEnd, Width = 10 }, new[]
            {
                b.Text("Test"),
            })
        }, Opts100);

        Assert.Equal("      Test", output);
    }

    [Fact]
    public void RowAlignItemsStretch()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { AlignItems = AlignItemsMode.Stretch, Height = 5 }, new[]
            {
                b.Box(new InkStyle { BorderStyle = "single" }, new[] { b.Text("X") }),
            })
        }, Opts100);

        Assert.Equal("┌─┐\n│X│\n│ │\n│ │\n└─┘", output);
    }

    [Fact]
    public void RowDefaultAlignItemsStretchesChildren()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Height = 5 }, new[]
            {
                b.Box(new InkStyle { BorderStyle = "single" }, new[] { b.Text("X") }),
            })
        }, Opts100);

        Assert.Equal("┌─┐\n│X│\n│ │\n│ │\n└─┘", output);
    }

    [Fact]
    public void RowAlignTextToBaseline()
    {
        // In JS: <Text>A<Newline />B</Text><Text>X</Text>
        // In C#: We use column layout with A\nB as one text, X as another
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { AlignItems = AlignItemsMode.Baseline, Height = 3 }, new[]
            {
                b.Text("A\nB"),
                b.Text("X"),
            })
        }, Opts100);

        Assert.Equal("A\nBX\n", output);
    }
}
