// Tests ported from position.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Position tests aligned with JS test suite.</summary>
public class PositionTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    [Fact]
    public void AbsolutePositionWithTopAndLeftOffsets()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 5, Height = 3 }, new[]
            {
                b.Box(new InkStyle
                {
                    Position = PositionMode.Absolute,
                    Top = DimensionValue.Points(1),
                    Left = DimensionValue.Points(2),
                }, new[]
                {
                    b.Text("X"),
                })
            })
        }, Opts100);

        Assert.Equal("\n  X\n", output);
    }

    [Fact]
    public void AbsolutePositionWithBottomAndRightOffsets()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 6, Height = 4 }, new[]
            {
                b.Box(new InkStyle
                {
                    Position = PositionMode.Absolute,
                    Bottom = DimensionValue.Points(1),
                    Right = DimensionValue.Points(1),
                }, new[]
                {
                    b.Text("X"),
                })
            })
        }, Opts100);

        Assert.Equal("\n\n    X\n", output);
    }

    [Fact]
    public void AbsolutePositionWithPercentageOffsets()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 6, Height = 4 }, new[]
            {
                b.Box(new InkStyle
                {
                    Position = PositionMode.Absolute,
                    Top = DimensionValue.Percent(50),
                    Left = DimensionValue.Percent(50),
                }, new[]
                {
                    b.Text("X"),
                })
            })
        }, Opts100);

        Assert.Equal("\n\n   X\n", output);
    }

    [Fact]
    public void RelativePositionOffsetsVisualPosition()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 5 }, new[]
            {
                b.Box(new InkStyle
                {
                    Position = PositionMode.Relative,
                    Left = DimensionValue.Points(2),
                }, new[]
                {
                    b.Text("A"),
                }),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal(" BA", output);
    }

    [Fact]
    public void StaticPositionIgnoresOffsets()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 5 }, new[]
            {
                b.Box(new InkStyle
                {
                    Position = PositionMode.Static,
                    Left = DimensionValue.Points(2),
                }, new[]
                {
                    b.Text("A"),
                }),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("AB", output);
    }
}
