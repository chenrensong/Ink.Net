// Tests ported from width-height.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Width and height tests aligned with JS width-height.tsx test suite.</summary>
public class WidthHeightTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    // ── Width ───────────────────────────────────────────────────────────

    [Fact]
    public void SetWidth()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[]
            {
                b.Box(new InkStyle { Width = 5 }, new[] { b.Text("A") }),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A    B", output);
    }

    [Fact]
    public void SetWidthInPercent()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 10 }, new[]
            {
                b.Box(new InkStyle { Width = DimensionValue.Percent(50) }, new[] { b.Text("A") }),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A    B", output);
    }

    [Fact]
    public void SetMinWidth()
    {
        // Smaller content - minWidth forces wider box
        var smallerOutput = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[]
            {
                b.Box(new InkStyle { MinWidth = 5 }, new[] { b.Text("A") }),
                b.Text("B"),
            })
        }, Opts100);
        Assert.Equal("A    B", smallerOutput);

        // Larger content - minWidth has no effect
        var largerOutput = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[]
            {
                b.Box(new InkStyle { MinWidth = 2 }, new[] { b.Text("AAAAA") }),
                b.Text("B"),
            })
        }, Opts100);
        Assert.Equal("AAAAAB", largerOutput);
    }

    // ── Height ──────────────────────────────────────────────────────────

    [Fact]
    public void SetHeight()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Height = 4 }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("AB\n\n\n", output);
    }

    [Fact]
    public void SetHeightInPercent()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Height = 6, FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Box(new InkStyle { Height = DimensionValue.Percent(50) }, new[] { b.Text("A") }),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A\n\n\nB\n\n", output);
    }

    [Fact]
    public void CutTextOverSetHeight()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Height = 2 }, new[]
            {
                b.Text("AAAABBBBCCCC"),
            })
        }, new RenderToStringOptions { Columns = 4 });

        Assert.Equal("AAAA\nBBBB", output);
    }

    [Fact]
    public void SetMinHeight()
    {
        // Smaller content - minHeight forces taller box
        var smallerOutput = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { MinHeight = 4 }, new[]
            {
                b.Text("A"),
            })
        }, Opts100);
        Assert.Equal("A\n\n\n", smallerOutput);

        // Larger content
        var largerOutput = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { MinHeight = 2 }, new[]
            {
                b.Box(new InkStyle { Height = 4 }, new[] { b.Text("A") }),
            })
        }, Opts100);
        Assert.Equal("A\n\n\n", largerOutput);
    }

    [Fact]
    public void SetMinHeightInPercent()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Height = 6, FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Box(new InkStyle { MinHeight = DimensionValue.Percent(50) }, new[] { b.Text("A") }),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A\n\n\nB\n\n", output);
    }

    // ── MaxWidth ────────────────────────────────────────────────────────

    [Fact]
    public void SetMaxWidth()
    {
        // Constrained
        var constrainedOutput = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[]
            {
                b.Box(new InkStyle { MaxWidth = 3 }, new[] { b.Text("AAAAA") }),
                b.Text("B"),
            })
        }, new RenderToStringOptions { Columns = 10 });
        Assert.Equal("AAAB\nAA", constrainedOutput);

        // Unconstrained
        var unconstrainedOutput = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[]
            {
                b.Box(new InkStyle { MaxWidth = 10 }, new[] { b.Text("AAA") }),
                b.Text("B"),
            })
        }, Opts100);
        Assert.Equal("AAAB", unconstrainedOutput);
    }

    // ── MaxHeight ───────────────────────────────────────────────────────

    [Fact]
    public void SetMaxHeight()
    {
        // Constrained
        var constrainedOutput = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { MaxHeight = 2 }, new[]
            {
                b.Box(new InkStyle { Height = 4 }, new[] { b.Text("A") }),
            })
        }, Opts100);
        Assert.Equal("A\n", constrainedOutput);

        // Unconstrained
        var unconstrainedOutput = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { MaxHeight = 4 }, new[]
            {
                b.Text("A"),
            })
        }, Opts100);
        Assert.Equal("A", unconstrainedOutput);
    }

    [Fact]
    public void SetMaxHeightInPercent()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Height = 6, FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Box(new InkStyle { MaxHeight = DimensionValue.Percent(50) }, new[]
                {
                    b.Box(new InkStyle { Height = 6 }, new[] { b.Text("A") }),
                }),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A\n\n\nB\n\n", output);
    }

    // ── Aspect ratio ───────────────────────────────────────────────────

    [Fact]
    public void SetAspectRatioWithWidth()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Box(new InkStyle { Width = 8, AspectRatio = 2, BorderStyle = "single" }, new[]
                {
                    b.Text("X"),
                }),
                b.Text("Y"),
            })
        }, Opts100);

        Assert.Equal("┌──────┐\n│X     │\n│      │\n└──────┘\nY", output);
    }

    [Fact]
    public void SetAspectRatioWithHeight()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Box(new InkStyle { Height = 3, AspectRatio = 2, BorderStyle = "single" }, new[]
                {
                    b.Text("X"),
                }),
                b.Text("Y"),
            })
        }, Opts100);

        Assert.Equal("┌────┐\n│X   │\n└────┘\nY", output);
    }

    [Fact]
    public void SetAspectRatioWithWidthAndHeight()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Box(new InkStyle { Width = 8, Height = 3, AspectRatio = 2, BorderStyle = "single" }, new[]
                {
                    b.Text("X"),
                }),
                b.Text("Y"),
            })
        }, Opts100);

        Assert.Equal("┌────┐\n│X   │\n└────┘\nY", output);
    }

    [Fact]
    public void SetAspectRatioWithMaxHeightConstraint()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Box(new InkStyle { Width = 10, MaxHeight = 3, AspectRatio = 2, BorderStyle = "single" }, new[]
                {
                    b.Text("X"),
                }),
                b.Text("Y"),
            })
        }, Opts100);

        Assert.Equal("┌────┐\n│X   │\n└────┘\nY", output);
    }
}
