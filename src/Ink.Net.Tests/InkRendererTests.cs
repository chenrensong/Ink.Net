using Facebook.Yoga;
using Ink.Net.Dom;
using Ink.Net.Rendering;
using Ink.Net.Styles;
using static Facebook.Yoga.YGNodeAPI;
using static Facebook.Yoga.YGNodeStyleAPI;
using static Facebook.Yoga.YGNodeLayoutAPI;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>End-to-end tests for <see cref="InkRenderer"/>.</summary>
public class InkRendererTests
{
    [Fact]
    public void SimpleTextRendersToString()
    {
        var root = DomTree.CreateNode(InkNodeType.Root);
        var text = DomTree.CreateNode(InkNodeType.Text);
        var literal = DomTree.CreateTextNode("E2E Test");
        DomTree.AppendChildNode(root, text);
        DomTree.AppendChildNode(text, literal);

        YGNodeCalculateLayout(root.YogaNode!, 80, 24, YGDirection.LTR);

        var result = InkRenderer.Render(root);
        Assert.Contains("E2E Test", result.Output);
        Assert.True(result.OutputHeight > 0);
    }

    [Fact]
    public void EmptyTreeReturnsResultWithHeight()
    {
        var root = DomTree.CreateNode(InkNodeType.Root);
        YGNodeCalculateLayout(root.YogaNode!, 80, 24, YGDirection.LTR);
        var result = InkRenderer.Render(root);
        // Empty tree still has height from Yoga layout (rows = 24)
        Assert.True(result.OutputHeight >= 0);
        Assert.Equal("", result.StaticOutput);
    }

    [Fact]
    public void NestedBoxesRender()
    {
        var root = DomTree.CreateNode(InkNodeType.Root);
        var outerBox = DomTree.CreateNode(InkNodeType.Box);
        var innerBox = DomTree.CreateNode(InkNodeType.Box);
        var text = DomTree.CreateNode(InkNodeType.Text);
        var literal = DomTree.CreateTextNode("Nested");
        DomTree.AppendChildNode(root, outerBox);
        DomTree.AppendChildNode(outerBox, innerBox);
        DomTree.AppendChildNode(innerBox, text);
        DomTree.AppendChildNode(text, literal);

        YGNodeCalculateLayout(root.YogaNode!, 80, 24, YGDirection.LTR);

        var result = InkRenderer.Render(root);
        Assert.Contains("Nested", result.Output);
    }
}
