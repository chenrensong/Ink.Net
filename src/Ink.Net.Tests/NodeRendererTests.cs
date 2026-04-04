using Facebook.Yoga;
using Ink.Net.Dom;
using Ink.Net.Rendering;
using Ink.Net.Styles;
using static Facebook.Yoga.YGNodeAPI;
using static Facebook.Yoga.YGNodeStyleAPI;
using static Facebook.Yoga.YGNodeLayoutAPI;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for <see cref="NodeRenderer"/> (full DOM → string rendering).</summary>
public class NodeRendererTests
{
    [Fact]
    public void TextNodeRendersContent()
    {
        var root = DomTree.CreateNode(InkNodeType.Root);
        var text = DomTree.CreateNode(InkNodeType.Text);
        var literal = DomTree.CreateTextNode("Hello");
        DomTree.AppendChildNode(root, text);
        DomTree.AppendChildNode(text, literal);

        YGNodeCalculateLayout(root.YogaNode!, 80, 24, YGDirection.LTR);

        var output = new Output(
            (int)YGNodeLayoutGetWidth(root.YogaNode!),
            (int)YGNodeLayoutGetHeight(root.YogaNode!));
        NodeRenderer.Render(root, output, skipStaticElements: true);
        var (str, _) = output.Get();

        Assert.Contains("Hello", str);
    }

    [Fact]
    public void BoxWithChildTextRendersContent()
    {
        var root = DomTree.CreateNode(InkNodeType.Root);
        var box = DomTree.CreateNode(InkNodeType.Box);
        var text = DomTree.CreateNode(InkNodeType.Text);
        var literal = DomTree.CreateTextNode("BoxChild");
        DomTree.AppendChildNode(root, box);
        DomTree.AppendChildNode(box, text);
        DomTree.AppendChildNode(text, literal);

        YGNodeCalculateLayout(root.YogaNode!, 80, 24, YGDirection.LTR);

        var output = new Output(
            (int)YGNodeLayoutGetWidth(root.YogaNode!),
            (int)YGNodeLayoutGetHeight(root.YogaNode!));
        NodeRenderer.Render(root, output, skipStaticElements: true);
        var (str, _) = output.Get();

        Assert.Contains("BoxChild", str);
    }

    [Fact]
    public void ScreenReaderOutputReturnsText()
    {
        var root = DomTree.CreateNode(InkNodeType.Root);
        var text = DomTree.CreateNode(InkNodeType.Text);
        var literal = DomTree.CreateTextNode("SR Output");
        DomTree.AppendChildNode(root, text);
        DomTree.AppendChildNode(text, literal);

        YGNodeCalculateLayout(root.YogaNode!, 80, 24, YGDirection.LTR);

        string sr = NodeRenderer.RenderToScreenReaderOutput(root);
        Assert.Contains("SR Output", sr);
    }

    [Fact]
    public void DisplayNoneHidesNode()
    {
        var root = DomTree.CreateNode(InkNodeType.Root);
        var text = DomTree.CreateNode(InkNodeType.Text);
        text.Style = new InkStyle { Display = DisplayMode.None };
        StyleApplier.Apply(text.YogaNode!, text.Style);

        var literal = DomTree.CreateTextNode("Hidden");
        DomTree.AppendChildNode(root, text);
        DomTree.AppendChildNode(text, literal);

        YGNodeCalculateLayout(root.YogaNode!, 80, 24, YGDirection.LTR);

        var output = new Output(80, 24);
        NodeRenderer.Render(root, output, skipStaticElements: true);
        var (str, _) = output.Get();

        Assert.DoesNotContain("Hidden", str);
    }
}
