using Ink.Net.Builder;
using Ink.Net.Dom;
using Ink.Net.Rendering;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for <see cref="TreeBuilder"/> (builder API).</summary>
public class TreeBuilderTests
{
    [Fact]
    public void BoxCreatesBoxNode()
    {
        var builder = new TreeBuilder();
        var box = builder.Box(new InkStyle { FlexGrow = 1 });
        Assert.NotNull(box);
    }

    [Fact]
    public void TextCreatesTextNode()
    {
        var builder = new TreeBuilder();
        var text = builder.Text("Hello");
        Assert.NotNull(text);
    }

    [Fact]
    public void BuildCreatesRoot()
    {
        var builder = new TreeBuilder();
        var root = builder.Build(
            [builder.Text("Test")],
            columns: 80
        );

        Assert.Equal(InkNodeType.Root, root.NodeType);
        Assert.Single(root.ChildNodes);
        Assert.NotNull(root.YogaNode);
    }

    [Fact]
    public void SpacerHasFlexGrow()
    {
        var builder = new TreeBuilder();
        var spacer = builder.Spacer();
        Assert.NotNull(spacer);
    }

    [Fact]
    public void NestedBoxesArePossible()
    {
        var builder = new TreeBuilder();
        var root = builder.Build(
            [builder.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, [
                builder.Text("Line 1"),
                builder.Text("Line 2"),
                builder.Box(new InkStyle { FlexDirection = FlexDirectionMode.Row }, [
                    builder.Text("Col A"),
                    builder.Spacer(),
                    builder.Text("Col B"),
                ]),
            ])],
            columns: 80
        );

        Assert.Equal(InkNodeType.Root, root.NodeType);
    }
}
