// Ported from examples/box-backgrounds/box-backgrounds.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Background color examples — ported from JS Ink examples/box-backgrounds/box-backgrounds.tsx.
/// </summary>
public static class BoxBackgrounds
{
    public static void Run()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Gap = 1 }, new[]
            {
                b.Box(children: new[] { b.Text("Box Background Examples:") }),

                // 1. Standard red background
                b.Box(children: new[] { b.Text("1. Standard red background (10x3):") }),
                b.Box(new InkStyle
                    {
                        BackgroundColor = "red", Width = 10, Height = 3,
                        AlignSelf = AlignSelfMode.FlexStart
                    },
                    new[] { b.Text("Hello") }),

                // 2. Blue background with border
                b.Box(children: new[] { b.Text("2. Blue background with border (12x4):") }),
                b.Box(new InkStyle
                    {
                        BackgroundColor = "blue", BorderStyle = "round",
                        Width = 12, Height = 4, AlignSelf = AlignSelfMode.FlexStart
                    },
                    new[] { b.Text("Border") }),

                // 3. Green background with padding
                b.Box(children: new[] { b.Text("3. Green background with padding (14x4):") }),
                b.Box(new InkStyle
                    {
                        BackgroundColor = "green", Padding = 1,
                        Width = 14, Height = 4, AlignSelf = AlignSelfMode.FlexStart
                    },
                    new[] { b.Text("Padding") }),

                // 4. Yellow background with center alignment
                b.Box(children: new[] { b.Text("4. Yellow background with center alignment (16x3):") }),
                b.Box(new InkStyle
                    {
                        BackgroundColor = "yellow", Width = 16, Height = 3,
                        JustifyContent = JustifyContentMode.Center,
                        AlignSelf = AlignSelfMode.FlexStart
                    },
                    new[] { b.Text("Centered") }),

                // 5. Magenta background, column layout
                b.Box(children: new[] { b.Text("5. Magenta background, column layout (12x5):") }),
                b.Box(new InkStyle
                    {
                        BackgroundColor = "magenta", FlexDirection = FlexDirectionMode.Column,
                        Width = 12, Height = 5, AlignSelf = AlignSelfMode.FlexStart
                    },
                    new[]
                    {
                        b.Text("Line 1"),
                        b.Text("Line 2"),
                    }),

                // 6. Hex color background
                b.Box(children: new[] { b.Text("6. Hex color background #FF8800 (10x3):") }),
                b.Box(new InkStyle
                    {
                        BackgroundColor = "#FF8800", Width = 10, Height = 3,
                        AlignSelf = AlignSelfMode.FlexStart
                    },
                    new[] { b.Text("Hex") }),

                // 7. RGB background
                b.Box(children: new[] { b.Text("7. RGB background rgb(0,255,0) (10x3):") }),
                b.Box(new InkStyle
                    {
                        BackgroundColor = "rgb(0,255,0)", Width = 10, Height = 3,
                        AlignSelf = AlignSelfMode.FlexStart
                    },
                    new[] { b.Text("RGB") }),

                // 8. Text inheritance test
                b.Box(children: new[] { b.Text("8. Text inheritance test:") }),
                b.Box(new InkStyle { BackgroundColor = "cyan", AlignSelf = AlignSelfMode.FlexStart }, new[]
                {
                    b.Text("Inherited "),
                    b.Text("Override ", new InkStyle { BackgroundColor = "red" }),
                    b.Text("Back to inherited"),
                }),

                // 9. Nested background inheritance
                b.Box(children: new[] { b.Text("9. Nested background inheritance:") }),
                b.Box(new InkStyle { BackgroundColor = "blue", AlignSelf = AlignSelfMode.FlexStart }, new[]
                {
                    b.Text("Outer: "),
                    b.Box(new InkStyle { BackgroundColor = "yellow" }, new[]
                    {
                        b.Text("Inner: "),
                        b.Text("Deep", new InkStyle { BackgroundColor = "red" }),
                    }),
                }),
            })
        });

        Console.WriteLine(output);
    }
}
