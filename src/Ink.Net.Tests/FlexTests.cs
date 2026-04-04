// Tests ported from flex.tsx
// Covers: flex-grow, flex-shrink, flex-basis
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Flex grow/shrink/basis tests aligned with JS flex.tsx test suite.</summary>
public class FlexTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    [Fact]
    public void GrowEqually()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 6 }, new[]
            {
                b.Box(new InkStyle { FlexGrow = 1 }, new[] { b.Text("A") }),
                b.Box(new InkStyle { FlexGrow = 1 }, new[] { b.Text("B") }),
            })
        }, Opts100);

        Assert.Equal("A  B", output);
    }

    [Fact]
    public void GrowOneElement()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 6 }, new[]
            {
                b.Box(new InkStyle { FlexGrow = 1 }, new[] { b.Text("A") }),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A    B", output);
    }

    [Fact]
    public void DoNotShrink()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 16 }, new[]
            {
                b.Box(new InkStyle { FlexShrink = 0, Width = 6 }, new[] { b.Text("A") }),
                b.Box(new InkStyle { FlexShrink = 0, Width = 6 }, new[] { b.Text("B") }),
                b.Box(new InkStyle { Width = 6 }, new[] { b.Text("C") }),
            })
        }, Opts100);

        Assert.Equal("A     B     C", output);
    }

    [Fact]
    public void ShrinkEqually()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 10 }, new[]
            {
                b.Box(new InkStyle { FlexShrink = 1, Width = 6 }, new[] { b.Text("A") }),
                b.Box(new InkStyle { FlexShrink = 1, Width = 6 }, new[] { b.Text("B") }),
                b.Text("C"),
            })
        }, Opts100);

        Assert.Equal("A    B   C", output);
    }

    [Fact]
    public void SetFlexBasisWithRowContainer()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 6 }, new[]
            {
                b.Box(new InkStyle { FlexBasis = 3 }, new[] { b.Text("A") }),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A  B", output);
    }

    [Fact]
    public void SetFlexBasisInPercentWithRowContainer()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 6 }, new[]
            {
                b.Box(new InkStyle { FlexBasis = DimensionValue.Percent(50) }, new[] { b.Text("A") }),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A  B", output);
    }

    [Fact]
    public void SetFlexBasisWithColumnContainer()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Height = 6, FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Box(new InkStyle { FlexBasis = 3 }, new[] { b.Text("A") }),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A\n\n\nB\n\n", output);
    }

    [Fact]
    public void SetFlexBasisInPercentWithColumnContainer()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Height = 6, FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Box(new InkStyle { FlexBasis = DimensionValue.Percent(50) }, new[] { b.Text("A") }),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A\n\n\nB\n\n", output);
    }
}
