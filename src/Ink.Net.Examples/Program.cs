// Ink.Net Examples — ported from JS Ink examples
// Usage: dotnet run -- <example-name>
// Available: borders, backgrounds, justify-content, table, counter, render-to-string

using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Rendering;
using Ink.Net.Styles;

var exampleName = args.Length > 0 ? args[0] : "help";

switch (exampleName.ToLowerInvariant())
{
    case "borders":
        Examples.Borders();
        break;
    case "backgrounds":
        Examples.Backgrounds();
        break;
    case "justify-content":
        Examples.JustifyContent();
        break;
    case "table":
        Examples.Table();
        break;
    case "counter":
        await Examples.CounterAsync();
        break;
    case "render-to-string":
        Examples.RenderToStringDemo();
        break;
    default:
        Console.WriteLine("Ink.Net Examples");
        Console.WriteLine("================");
        Console.WriteLine("Usage: dotnet run -- <example>");
        Console.WriteLine();
        Console.WriteLine("Available examples:");
        Console.WriteLine("  borders           - Border styles showcase");
        Console.WriteLine("  backgrounds       - Background color examples");
        Console.WriteLine("  justify-content   - Flexbox justifyContent demo");
        Console.WriteLine("  table             - Table layout with percentage widths");
        Console.WriteLine("  counter           - Live counter (interactive, Ctrl+C to exit)");
        Console.WriteLine("  render-to-string  - Render UI to string (no terminal)");
        break;
}

/// <summary>
/// Example implementations ported from JS Ink examples.
/// </summary>
static class Examples
{
    // ═══════════════════════════════════════════════════════════════════
    //  borders — ported from examples/borders/borders.tsx
    // ═══════════════════════════════════════════════════════════════════
    public static void Borders()
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

    // ═══════════════════════════════════════════════════════════════════
    //  backgrounds — ported from examples/box-backgrounds/box-backgrounds.tsx
    // ═══════════════════════════════════════════════════════════════════
    public static void Backgrounds()
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

                // 4. Hex color background
                b.Box(children: new[] { b.Text("4. Hex color background #FF8800 (10x3):") }),
                b.Box(new InkStyle
                    {
                        BackgroundColor = "#FF8800", Width = 10, Height = 3,
                        AlignSelf = AlignSelfMode.FlexStart
                    },
                    new[] { b.Text("Hex") }),
            })
        });

        Console.WriteLine(output);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  justify-content — ported from examples/justify-content/justify-content.tsx
    // ═══════════════════════════════════════════════════════════════════
    public static void JustifyContent()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                MakeJcRow(b, JustifyContentMode.FlexStart, "flex-start"),
                MakeJcRow(b, JustifyContentMode.FlexEnd, "flex-end"),
                MakeJcRow(b, JustifyContentMode.Center, "center"),
                MakeJcRow(b, JustifyContentMode.SpaceAround, "space-around"),
                MakeJcRow(b, JustifyContentMode.SpaceBetween, "space-between"),
                MakeJcRow(b, JustifyContentMode.SpaceEvenly, "space-evenly"),
            })
        });

        Console.WriteLine(output);
    }

    private static TreeNode MakeJcRow(TreeBuilder b, JustifyContentMode jc, string label)
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

    // ═══════════════════════════════════════════════════════════════════
    //  table — ported from examples/table/table.tsx
    // ═══════════════════════════════════════════════════════════════════
    public static void Table()
    {
        var users = new[]
        {
            (Id: 0, Name: "Alice", Email: "alice@example.com"),
            (Id: 1, Name: "Bob", Email: "bob@example.com"),
            (Id: 2, Name: "Charlie", Email: "charlie@example.com"),
            (Id: 3, Name: "Diana", Email: "diana@example.com"),
            (Id: 4, Name: "Eve", Email: "eve@example.com"),
        };

        var output = InkApp.RenderToString(b =>
        {
            var rows = new List<TreeNode>();

            // Header
            rows.Add(b.Box(children: new[]
            {
                b.Box(new InkStyle { Width = DimensionValue.Percent(10) }, new[] { b.Text("ID") }),
                b.Box(new InkStyle { Width = DimensionValue.Percent(50) }, new[] { b.Text("Name") }),
                b.Box(new InkStyle { Width = DimensionValue.Percent(40) }, new[] { b.Text("Email") }),
            }));

            // Data rows
            foreach (var user in users)
            {
                rows.Add(b.Box(children: new[]
                {
                    b.Box(new InkStyle { Width = DimensionValue.Percent(10) },
                        new[] { b.Text(user.Id.ToString()) }),
                    b.Box(new InkStyle { Width = DimensionValue.Percent(50) },
                        new[] { b.Text(user.Name) }),
                    b.Box(new InkStyle { Width = DimensionValue.Percent(40) },
                        new[] { b.Text(user.Email) }),
                }));
            }

            return new[]
            {
                b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Width = 80 },
                    rows.ToArray())
            };
        });

        Console.WriteLine(output);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  counter — ported from examples/counter/counter.tsx
    //  Uses live render with periodic rerender
    // ═══════════════════════════════════════════════════════════════════
    public static async Task CounterAsync()
    {
        var counter = 0;

        var instance = InkApp.Render(b => new[]
        {
            b.Text($"{counter} tests passed",
                transform: (line, _) => Colorizer.Colorize(line, "green", ColorType.Foreground))
        });

        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                await Task.Delay(100, cts.Token);
                counter++;
                instance.Rerender(b => new[]
                {
                    b.Text($"{counter} tests passed",
                        transform: (line, _) => Colorizer.Colorize(line, "green", ColorType.Foreground))
                });
            }
        }
        catch (OperationCanceledException) { }

        instance.Unmount();
        Console.WriteLine($"\nFinal count: {counter}");
    }

    // ═══════════════════════════════════════════════════════════════════
    //  render-to-string — demonstrates string rendering (no terminal)
    // ═══════════════════════════════════════════════════════════════════
    public static void RenderToStringDemo()
    {
        Console.WriteLine("=== RenderToString Examples ===\n");

        // Simple text
        var simple = InkApp.RenderToString(b => b.Text("Hello, Ink.Net!"));
        Console.WriteLine($"1. Simple text:\n{simple}\n");

        // Box with text
        var boxed = InkApp.RenderToString(b =>
            b.Box(new InkStyle { BorderStyle = "round", Padding = 1 },
                new[] { b.Text("Boxed content") }));
        Console.WriteLine($"2. Bordered box:\n{boxed}\n");

        // Flex layout
        var flex = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 40 }, new[]
            {
                b.Box(new InkStyle { FlexGrow = 1 }, new[] { b.Text("Left") }),
                b.Box(new InkStyle { FlexGrow = 1, JustifyContent = JustifyContentMode.FlexEnd },
                    new[] { b.Text("Right") }),
            })
        }, new RenderToStringOptions { Columns = 40 });
        Console.WriteLine($"3. Flex layout (40 cols):\n{flex}\n");

        // Column layout with wrapping
        var column = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Width = 30 }, new[]
            {
                b.Text("Line 1: Hello"),
                b.Text("Line 2: World"),
                b.Text("Line 3: Ink.Net is awesome!"),
            })
        }, new RenderToStringOptions { Columns = 30 });
        Console.WriteLine($"4. Column layout:\n{column}\n");

        // Nested boxes
        var nested = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle
            {
                FlexDirection = FlexDirectionMode.Column,
                BorderStyle = "single",
                Padding = 1,
                Width = 40
            }, new[]
            {
                b.Text("Outer Box"),
                b.Box(new InkStyle
                {
                    BorderStyle = "round",
                    MarginTop = 1,
                    Padding = 1
                }, new[]
                {
                    b.Text("Inner Box"),
                }),
            })
        }, new RenderToStringOptions { Columns = 40 });
        Console.WriteLine($"5. Nested boxes:\n{nested}\n");
    }
}
