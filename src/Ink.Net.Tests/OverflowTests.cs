// Tests ported from overflow.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Overflow tests aligned with JS test suite.</summary>
public class OverflowTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    [Fact]
    public void OverflowX_SingleTextNodeInBox()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 6, OverflowX = OverflowMode.Hidden }, new[]
            {
                b.Box(new InkStyle { Width = 16, FlexShrink = 0 }, new[]
                {
                    b.Text("Hello World"),
                })
            })
        }, Opts100);

        Assert.Equal("Hello", output);
    }

    [Fact]
    public void OverflowX_MultipleBoxes()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 6, OverflowX = OverflowMode.Hidden }, new[]
            {
                b.Box(new InkStyle { Width = 6, FlexShrink = 0 }, new[]
                {
                    b.Text("Hello "),
                }),
                b.Box(new InkStyle { Width = 6, FlexShrink = 0 }, new[]
                {
                    b.Text("World"),
                })
            })
        }, Opts100);

        Assert.Equal("Hello", output);
    }

    [Fact]
    public void OverflowX_BoxAfterRightEdge()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 6, OverflowX = OverflowMode.Hidden }, new[]
            {
                b.Box(new InkStyle { MarginLeft = 6, Width = 6, FlexShrink = 0 }, new[]
                {
                    b.Text("Hello"),
                })
            })
        }, Opts100);

        Assert.Equal("", output);
    }

    [Fact]
    public void OverflowX_BoxIntersectingRightEdge()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 6, OverflowX = OverflowMode.Hidden }, new[]
            {
                b.Box(new InkStyle { MarginLeft = 3, Width = 6, FlexShrink = 0 }, new[]
                {
                    b.Text("Hello"),
                })
            })
        }, Opts100);

        Assert.Equal("   Hel", output);
    }

    [Fact]
    public void OverflowY_SingleTextNode()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Height = 1, OverflowY = OverflowMode.Hidden }, new[]
            {
                b.Text("Hello\nWorld"),
            })
        }, Opts100);

        Assert.Equal("Hello", output);
    }

    [Fact]
    public void OverflowY_MultipleBoxes()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle
            {
                Height = 2,
                OverflowY = OverflowMode.Hidden,
                FlexDirection = FlexDirectionMode.Column,
            }, new[]
            {
                b.Box(new InkStyle { FlexShrink = 0 }, new[] { b.Text("Line #1") }),
                b.Box(new InkStyle { FlexShrink = 0 }, new[] { b.Text("Line #2") }),
                b.Box(new InkStyle { FlexShrink = 0 }, new[] { b.Text("Line #3") }),
                b.Box(new InkStyle { FlexShrink = 0 }, new[] { b.Text("Line #4") }),
            })
        }, Opts100);

        Assert.Equal("Line #1\nLine #2", output);
    }

    [Fact]
    public void OverflowY_BoxIntersectingBottomEdge()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Height = 1, OverflowY = OverflowMode.Hidden }, new[]
            {
                b.Box(new InkStyle { Height = 2, FlexShrink = 0 }, new[]
                {
                    b.Text("Hello\nWorld"),
                })
            })
        }, Opts100);

        Assert.Equal("Hello", output);
    }

    [Fact]
    public void Overflow_SingleTextNode()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { PaddingBottom = 1 }, new[]
            {
                b.Box(new InkStyle { Width = 6, Height = 1, Overflow = OverflowMode.Hidden }, new[]
                {
                    b.Box(new InkStyle { Width = 12, Height = 2, FlexShrink = 0 }, new[]
                    {
                        b.Text("Hello\nWorld"),
                    })
                })
            })
        }, Opts100);

        Assert.Equal("Hello\n", output);
    }

    [Fact]
    public void Overflow_MultipleBoxes()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { PaddingBottom = 1 }, new[]
            {
                b.Box(new InkStyle { Width = 4, Height = 1, Overflow = OverflowMode.Hidden }, new[]
                {
                    b.Box(new InkStyle { Width = 2, Height = 2, FlexShrink = 0 }, new[]
                    {
                        b.Text("TL\nBL"),
                    }),
                    b.Box(new InkStyle { Width = 2, Height = 2, FlexShrink = 0 }, new[]
                    {
                        b.Text("TR\nBR"),
                    }),
                })
            })
        }, Opts100);

        Assert.Equal("TLTR\n", output);
    }
}
