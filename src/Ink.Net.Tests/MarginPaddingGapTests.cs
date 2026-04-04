// Tests ported from margin.tsx, padding.tsx, gap.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Margin, padding, and gap tests aligned with JS test suite.</summary>
public class MarginPaddingGapTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    // ── margin.tsx ──────────────────────────────────────────────────

    [Fact]
    public void Margin()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Margin = 2 }, new[]
            {
                b.Text("X"),
            })
        }, Opts100);

        Assert.Equal("\n\n  X\n\n", output);
    }

    [Fact]
    public void MarginX()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[]
            {
                b.Box(new InkStyle { MarginX = 2 }, new[] { b.Text("X") }),
                b.Text("Y"),
            })
        }, Opts100);

        Assert.Equal("  X  Y", output);
    }

    [Fact]
    public void MarginY()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { MarginY = 2 }, new[]
            {
                b.Text("X"),
            })
        }, Opts100);

        Assert.Equal("\n\nX\n\n", output);
    }

    [Fact]
    public void MarginTop()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { MarginTop = 2 }, new[]
            {
                b.Text("X"),
            })
        }, Opts100);

        Assert.Equal("\n\nX", output);
    }

    [Fact]
    public void MarginBottom()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { MarginBottom = 2 }, new[]
            {
                b.Text("X"),
            })
        }, Opts100);

        Assert.Equal("X\n\n", output);
    }

    [Fact]
    public void MarginLeft()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { MarginLeft = 2 }, new[]
            {
                b.Text("X"),
            })
        }, Opts100);

        Assert.Equal("  X", output);
    }

    [Fact]
    public void MarginRight()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[]
            {
                b.Box(new InkStyle { MarginRight = 2 }, new[] { b.Text("X") }),
                b.Text("Y"),
            })
        }, Opts100);

        Assert.Equal("X  Y", output);
    }

    [Fact]
    public void NestedMargin()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Margin = 2 }, new[]
            {
                b.Box(new InkStyle { Margin = 2 }, new[]
                {
                    b.Text("X"),
                })
            })
        }, Opts100);

        Assert.Equal("\n\n\n\n    X\n\n\n\n", output);
    }

    [Fact]
    public void MarginWithMultilineString()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Margin = 2 }, new[]
            {
                b.Text("A\nB"),
            })
        }, Opts100);

        Assert.Equal("\n\n  A\n  B\n\n", output);
    }

    // ── padding.tsx ─────────────────────────────────────────────────

    [Fact]
    public void Padding()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Padding = 2 }, new[]
            {
                b.Text("X"),
            })
        }, Opts100);

        Assert.Equal("\n\n  X\n\n", output);
    }

    [Fact]
    public void PaddingX()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[]
            {
                b.Box(new InkStyle { PaddingX = 2 }, new[] { b.Text("X") }),
                b.Text("Y"),
            })
        }, Opts100);

        Assert.Equal("  X  Y", output);
    }

    [Fact]
    public void PaddingY()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { PaddingY = 2 }, new[]
            {
                b.Text("X"),
            })
        }, Opts100);

        Assert.Equal("\n\nX\n\n", output);
    }

    [Fact]
    public void PaddingTop()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { PaddingTop = 2 }, new[]
            {
                b.Text("X"),
            })
        }, Opts100);

        Assert.Equal("\n\nX", output);
    }

    [Fact]
    public void PaddingBottom()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { PaddingBottom = 2 }, new[]
            {
                b.Text("X"),
            })
        }, Opts100);

        Assert.Equal("X\n\n", output);
    }

    [Fact]
    public void PaddingLeft()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { PaddingLeft = 2 }, new[]
            {
                b.Text("X"),
            })
        }, Opts100);

        Assert.Equal("  X", output);
    }

    [Fact]
    public void PaddingRight()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[]
            {
                b.Box(new InkStyle { PaddingRight = 2 }, new[] { b.Text("X") }),
                b.Text("Y"),
            })
        }, Opts100);

        Assert.Equal("X  Y", output);
    }

    [Fact]
    public void NestedPadding()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Padding = 2 }, new[]
            {
                b.Box(new InkStyle { Padding = 2 }, new[]
                {
                    b.Text("X"),
                })
            })
        }, Opts100);

        Assert.Equal("\n\n\n\n    X\n\n\n\n", output);
    }

    [Fact]
    public void PaddingWithMultilineString()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Padding = 2 }, new[]
            {
                b.Text("A\nB"),
            })
        }, Opts100);

        Assert.Equal("\n\n  A\n  B\n\n", output);
    }

    // ── gap.tsx ──────────────────────────────────────────────────────

    [Fact]
    public void Gap()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Gap = 1, Width = 3, FlexWrap = FlexWrapMode.Wrap }, new[]
            {
                b.Text("A"),
                b.Text("B"),
                b.Text("C"),
            })
        }, Opts100);

        Assert.Equal("A B\n\nC", output);
    }

    [Fact]
    public void ColumnGap()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Gap = 1 }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A B", output);
    }

    [Fact]
    public void RowGap()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Gap = 1 }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A\n\nB", output);
    }
}
