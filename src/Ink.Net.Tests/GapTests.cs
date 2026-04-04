// Tests ported from gap.tsx (1:1 file mapping)
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Gap tests — 1:1 port of JS test/gap.tsx.</summary>
public class GapTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    [Fact]
    public void Gap()
    {
        // gap=1, width=3, flexWrap="wrap" → "A B\n\nC"
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
        // gap=1, default flexDirection=row → "A B"
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
        // flexDirection=column, gap=1 → "A\n\nB"
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
