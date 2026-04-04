// Tests ported from flex-align-self.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Flex align-self tests aligned with JS flex-align-self.tsx test suite.</summary>
public class FlexAlignSelfTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    [Fact]
    public void RowAlignTextToCenter()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Height = 3 }, new[]
            {
                b.Box(new InkStyle { AlignSelf = AlignSelfMode.Center }, new[] { b.Text("Test") }),
            })
        }, Opts100);

        Assert.Equal("\nTest\n", output);
    }

    [Fact]
    public void RowAlignMultipleTextNodesToCenter()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Height = 3 }, new[]
            {
                b.Box(new InkStyle { AlignSelf = AlignSelfMode.Center }, new[]
                {
                    b.Text("A"),
                    b.Text("B"),
                }),
            })
        }, Opts100);

        Assert.Equal("\nAB\n", output);
    }

    [Fact]
    public void RowAlignTextToBottom()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Height = 3 }, new[]
            {
                b.Box(new InkStyle { AlignSelf = AlignSelfMode.FlexEnd }, new[] { b.Text("Test") }),
            })
        }, Opts100);

        Assert.Equal("\n\nTest", output);
    }

    [Fact]
    public void RowAlignMultipleTextNodesToBottom()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Height = 3 }, new[]
            {
                b.Box(new InkStyle { AlignSelf = AlignSelfMode.FlexEnd }, new[]
                {
                    b.Text("A"),
                    b.Text("B"),
                }),
            })
        }, Opts100);

        Assert.Equal("\n\nAB", output);
    }

    [Fact]
    public void ColumnAlignTextToCenter()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Width = 10 }, new[]
            {
                b.Box(new InkStyle { AlignSelf = AlignSelfMode.Center }, new[] { b.Text("Test") }),
            })
        }, Opts100);

        Assert.Equal("   Test", output);
    }

    [Fact]
    public void ColumnAlignTextToRight()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Width = 10 }, new[]
            {
                b.Box(new InkStyle { AlignSelf = AlignSelfMode.FlexEnd }, new[] { b.Text("Test") }),
            })
        }, Opts100);

        Assert.Equal("      Test", output);
    }

    [Fact]
    public void ColumnAlignSelfStretch()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Width = 7 }, new[]
            {
                b.Box(new InkStyle { AlignSelf = AlignSelfMode.Stretch, BorderStyle = "single" }, new[]
                {
                    b.Text("X"),
                }),
            })
        }, Opts100);

        Assert.Equal("┌─────┐\n│X    │\n└─────┘", output);
    }

    [Fact]
    public void RowAlignSelfStretch()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Height = 5 }, new[]
            {
                b.Box(new InkStyle { AlignSelf = AlignSelfMode.Stretch, BorderStyle = "single" }, new[]
                {
                    b.Text("X"),
                }),
            })
        }, Opts100);

        Assert.Equal("┌─┐\n│X│\n│ │\n│ │\n└─┘", output);
    }

    [Fact]
    public void RowAlignSelfBaseline()
    {
        // In JS: <Box alignItems="flex-end" height={3}>
        //   <Text>A<Newline />B</Text>
        //   <Box alignSelf="baseline"><Text>X</Text></Box>
        // </Box>
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { AlignItems = AlignItemsMode.FlexEnd, Height = 3 }, new[]
            {
                b.Text("A\nB"),
                b.Box(new InkStyle { AlignSelf = AlignSelfMode.Baseline }, new[] { b.Text("X") }),
            })
        }, Opts100);

        Assert.Equal("AX\nB\n", output);
    }
}
