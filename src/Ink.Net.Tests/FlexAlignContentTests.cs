// Tests ported from flex-align-content.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Flex align-content tests aligned with JS flex-align-content.tsx test suite.</summary>
public class FlexAlignContentTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    private static string RenderWithAlignContent(AlignContentMode alignContent)
    {
        return InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle
            {
                Width = 2,
                Height = 6,
                FlexWrap = FlexWrapMode.Wrap,
                AlignContent = alignContent,
            }, new[]
            {
                b.Text("A"),
                b.Text("B"),
                b.Text("C"),
                b.Text("D"),
            })
        }, Opts100);
    }

    [Fact]
    public void AlignContentFlexStart()
    {
        Assert.Equal("AB\nCD\n\n\n\n", RenderWithAlignContent(AlignContentMode.FlexStart));
    }

    [Fact]
    public void AlignContentCenter()
    {
        Assert.Equal("\n\nAB\nCD\n\n", RenderWithAlignContent(AlignContentMode.Center));
    }

    [Fact]
    public void AlignContentFlexEnd()
    {
        Assert.Equal("\n\n\n\nAB\nCD", RenderWithAlignContent(AlignContentMode.FlexEnd));
    }

    [Fact]
    public void AlignContentSpaceBetween()
    {
        Assert.Equal("AB\n\n\n\n\nCD", RenderWithAlignContent(AlignContentMode.SpaceBetween));
    }

    [Fact]
    public void AlignContentSpaceAround()
    {
        Assert.Equal("\nAB\n\n\nCD\n", RenderWithAlignContent(AlignContentMode.SpaceAround));
    }

    [Fact]
    public void AlignContentSpaceEvenly()
    {
        Assert.Equal("\nAB\n\nCD\n\n", RenderWithAlignContent(AlignContentMode.SpaceEvenly));
    }

    [Fact]
    public void AlignContentStretch()
    {
        Assert.Equal("AB\n\n\nCD\n\n", RenderWithAlignContent(AlignContentMode.Stretch));
    }

    [Fact]
    public void AlignContentDefaultsToFlexStart()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 2, Height = 6, FlexWrap = FlexWrapMode.Wrap }, new[]
            {
                b.Text("A"),
                b.Text("B"),
                b.Text("C"),
                b.Text("D"),
            })
        }, Opts100);

        Assert.Equal("AB\nCD\n\n\n\n", output);
    }

    [Fact]
    public void AlignContentDoesNotAddExtraSpacingWhenNoFreeCrossAxisSpace()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle
            {
                Width = 2,
                Height = 2,
                FlexWrap = FlexWrapMode.Wrap,
                AlignContent = AlignContentMode.Center,
            }, new[]
            {
                b.Text("A"),
                b.Text("B"),
                b.Text("C"),
                b.Text("D"),
            })
        }, Opts100);

        Assert.Equal("AB\nCD", output);
    }
}
