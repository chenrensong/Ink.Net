// Ported from examples/justify-content/justify-content.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Flexbox justifyContent demo — ported from JS Ink examples/justify-content/justify-content.tsx.
/// </summary>
public static class JustifyContentExample
{
    public static void Run()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                MakeRow(b, JustifyContentMode.FlexStart, "flex-start"),
                MakeRow(b, JustifyContentMode.FlexEnd, "flex-end"),
                MakeRow(b, JustifyContentMode.Center, "center"),
                MakeRow(b, JustifyContentMode.SpaceAround, "space-around"),
                MakeRow(b, JustifyContentMode.SpaceBetween, "space-between"),
                MakeRow(b, JustifyContentMode.SpaceEvenly, "space-evenly"),
            })
        });

        Console.WriteLine(output);
    }

    private static TreeNode MakeRow(TreeBuilder b, JustifyContentMode jc, string label)
    {
        return b.Box(children: new[]
        {
            b.Text("["),
            b.Box(new InkStyle { JustifyContent = jc, Width = 20, Height = 1 }, new[]
            {
                b.Text("X"),
                b.Text("Y"),
            }),
            b.Text($"] {label}"),
        });
    }
}
