// Ported from examples/borders/borders.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Border styles showcase — ported from JS Ink examples/borders/borders.tsx.
/// </summary>
public static class Borders
{
    public static void Run()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Padding = 2 }, new[]
            {
                // Row 1: single, double, round, bold
                b.Box(children: new[]
                {
                    b.Box(new InkStyle { BorderStyle = "single", MarginRight = 2 },
                        new[] { b.Text("single") }),
                    b.Box(new InkStyle { BorderStyle = "double", MarginRight = 2 },
                        new[] { b.Text("double") }),
                    b.Box(new InkStyle { BorderStyle = "round", MarginRight = 2 },
                        new[] { b.Text("round") }),
                    b.Box(new InkStyle { BorderStyle = "bold" },
                        new[] { b.Text("bold") }),
                }),
                // Row 2: singleDouble, doubleSingle, classic
                b.Box(new InkStyle { MarginTop = 1 }, new[]
                {
                    b.Box(new InkStyle { BorderStyle = "singleDouble", MarginRight = 2 },
                        new[] { b.Text("singleDouble") }),
                    b.Box(new InkStyle { BorderStyle = "doubleSingle", MarginRight = 2 },
                        new[] { b.Text("doubleSingle") }),
                    b.Box(new InkStyle { BorderStyle = "classic" },
                        new[] { b.Text("classic") }),
                }),
            })
        });

        Console.WriteLine(output);
    }
}
