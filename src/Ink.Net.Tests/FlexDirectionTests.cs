// Tests ported from flex-direction.tsx (1:1 file mapping)
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Flex direction tests — 1:1 port of JS test/flex-direction.tsx.</summary>
public class FlexDirectionTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    [Fact]
    public void DirectionRow()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Row }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("AB", output);
    }

    [Fact]
    public void DirectionRowReverse()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.RowReverse, Width = 4 }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("  BA", output);
    }

    [Fact]
    public void DirectionColumn()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A\nB", output);
    }

    [Fact]
    public void DirectionColumnReverse()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.ColumnReverse, Height = 4 }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("\n\nB\nA", output);
    }

    [Fact]
    public void DontSquashTextNodesWhenColumnDirection()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        }, Opts100);

        Assert.Equal("A\nB", output);
    }
}
