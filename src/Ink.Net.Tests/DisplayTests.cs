// Tests ported from display.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Display mode tests aligned with JS test suite.</summary>
public class DisplayTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    [Fact]
    public void DisplayFlex()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Display = DisplayMode.Flex }, new[]
            {
                b.Text("X"),
            })
        }, Opts100);

        Assert.Equal("X", output);
    }

    [Fact]
    public void DisplayNone()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Box(new InkStyle { Display = DisplayMode.None }, new[]
                {
                    b.Text("Kitty!"),
                }),
                b.Text("Doggo"),
            })
        }, Opts100);

        Assert.Equal("Doggo", output);
    }
}
