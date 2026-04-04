// -----------------------------------------------------------------------
// <copyright file="InkRenderer.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) renderer.ts
// </copyright>
// -----------------------------------------------------------------------

using Ink.Net.Dom;
using static Facebook.Yoga.YGNodeLayoutAPI;

namespace Ink.Net.Rendering;

/// <summary>
/// Result of a render pass.
/// <para>Corresponds to JS <c>Result</c> type in <c>renderer.ts</c>.</para>
/// </summary>
public sealed class RenderResult
{
    /// <summary>The rendered output string.</summary>
    public string Output { get; init; } = "";

    /// <summary>Height in lines of the dynamic output.</summary>
    public int OutputHeight { get; init; }

    /// <summary>Static content output (from &lt;Static&gt; component).</summary>
    public string StaticOutput { get; init; } = "";
}

/// <summary>
/// Top-level renderer: layout → Output → string.
/// <para>1:1 port of Ink JS <c>renderer.ts</c>.</para>
/// </summary>
public static class InkRenderer
{
    /// <summary>
    /// Render a DOM tree to strings.
    /// <para>Corresponds to JS <c>renderer(node, isScreenReaderEnabled)</c>.</para>
    /// </summary>
    public static RenderResult Render(DomElement node, bool isScreenReaderEnabled = false)
    {
        if (node.YogaNode is null)
        {
            return new RenderResult();
        }

        if (isScreenReaderEnabled)
        {
            string output = NodeRenderer.RenderToScreenReaderOutput(node, skipStaticElements: true);
            int outputHeight = output == "" ? 0 : output.Split('\n').Length;

            string staticOutput = "";
            if (node.StaticNode is not null)
            {
                staticOutput = NodeRenderer.RenderToScreenReaderOutput(node.StaticNode, skipStaticElements: false);
            }

            return new RenderResult
            {
                Output = output,
                OutputHeight = outputHeight,
                StaticOutput = staticOutput.Length > 0 ? $"{staticOutput}\n" : "",
            };
        }

        var outputBuffer = new Output(
            (int)YGNodeLayoutGetWidth(node.YogaNode),
            (int)YGNodeLayoutGetHeight(node.YogaNode));

        NodeRenderer.Render(node, outputBuffer, skipStaticElements: true);

        Output? staticOutputBuffer = null;
        if (node.StaticNode?.YogaNode is not null)
        {
            staticOutputBuffer = new Output(
                (int)YGNodeLayoutGetWidth(node.StaticNode.YogaNode),
                (int)YGNodeLayoutGetHeight(node.StaticNode.YogaNode));

            NodeRenderer.Render(node.StaticNode, staticOutputBuffer, skipStaticElements: false);
        }

        var (generatedOutput, outputHeight2) = outputBuffer.Get();

        return new RenderResult
        {
            Output = generatedOutput,
            OutputHeight = outputHeight2,
            StaticOutput = staticOutputBuffer is not null
                ? $"{staticOutputBuffer.Get().Output}\n"
                : "",
        };
    }
}
