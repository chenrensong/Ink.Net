// Ported from examples/use-transition/use-transition.tsx
// Semantic port: immediate query vs deferred filter + "pending" flag (no React useTransition API).
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Rendering;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Demo: typing updates the search line immediately while the result list catches up (simulated work).
/// </summary>
public static class UseTransitionExample
{
    private static readonly string[] Fruit = ["Apple", "Banana", "Cherry", "Date", "Elderberry"];

    public static async Task RunAsync()
    {
        var query = "";
        var deferredQuery = "";
        var pending = false;
        var workVersion = 0;

        string BuildQueryDisplay() => string.IsNullOrEmpty(query) ? "(type something)" : query;

        TreeNode[] BuildUI(TreeBuilder b)
        {
            var searchRow = new List<TreeNode>
            {
                b.Text("Search: "),
                b.Text(Colorizer.Colorize(BuildQueryDisplay(), "cyan", ColorType.Foreground)),
            };
            if (pending)
                searchRow.Add(b.Text(Colorizer.Colorize(" (updating...)", "yellow", ColorType.Foreground)));

            return new[]
            {
                b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
                {
                    b.Text(Colorizer.Colorize("useTransition Demo", "cyan", ColorType.Foreground) + " (C# semantic port)"),
                    b.Text(Colorizer.Dim("(Type to search — line stays responsive while list updates)")),
                    b.Box(new InkStyle { MarginTop = 1 }),
                    b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Row }, searchRow.ToArray()),
                    b.Box(new InkStyle { MarginTop = 1, FlexDirection = FlexDirectionMode.Column },
                        BuildResultChildren(b, deferredQuery, pending).ToArray()),
                    b.Box(new InkStyle { MarginTop = 1 }, new[]
                    {
                        b.Text(Colorizer.Dim("Press Ctrl+C to exit")),
                    }),
                }),
            };
        }

        var app = InkApplication.Create(b => BuildUI(b), new InkApplicationOptions { ExitOnCtrlC = true });

        void scheduleDeferred(string targetDeferred, int myVersion)
        {
            pending = true;
            app.Rerender(b => BuildUI(b));
            _ = Task.Run(async () =>
            {
                var start = DateTime.UtcNow;
                while ((DateTime.UtcNow - start).TotalMilliseconds < 100)
                    await Task.Yield();

                if (myVersion != workVersion) return;

                deferredQuery = targetDeferred;
                pending = false;
                if (!app.Lifecycle.HasExited)
                    app.Rerender(b => BuildUI(b));
            });
        }

        app.Input.Register((input, key) =>
        {
            if (key.Ctrl && input == "c")
                return;

            if (key.Backspace || key.Delete)
            {
                if (query.Length > 0)
                    query = query[..^1];
                workVersion++;
                scheduleDeferred(query, workVersion);
                return;
            }

            if (!string.IsNullOrEmpty(input) && !key.Ctrl && !key.Meta)
            {
                query += input;
                workVersion++;
                scheduleDeferred(query, workVersion);
            }
        });

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            app.Lifecycle.Exit();
        };

        _ = Task.Run(async () =>
        {
            try
            {
                var buf = new char[256];
                while (!app.Lifecycle.HasExited)
                {
                    int n = await Console.In.ReadAsync(buf, 0, buf.Length);
                    if (n > 0) app.HandleInput(new string(buf, 0, n));
                }
            }
            catch { /* stdin closed */ }
        });

        try { await app.WaitUntilExit(); } catch { }
        app.Dispose();
    }

    private static List<TreeNode> BuildResultChildren(TreeBuilder b, string filter, bool pending)
    {
        var items = GenerateItems(filter);
        var list = new List<TreeNode>
        {
            b.Text(Colorizer.Colorize(
                $"Results {(string.IsNullOrEmpty(filter) ? "(showing first 10)" : $"for \"{filter}\"")}:",
                "white",
                ColorType.Foreground)),
        };

        if (items.Count == 0)
        {
            list.Add(b.Text(Colorizer.Dim(" No items found")));
            return list;
        }

        foreach (var item in items)
            list.Add(b.Text(pending ? Colorizer.Dim(item) : item));

        return list;
    }

    private static List<string> GenerateItems(string filter)
    {
        var all = new List<string>(200);
        for (int i = 0; i < 200; i++)
            all.Add($"Item {i + 1}: {Fruit[i % Fruit.Length]}");

        if (string.IsNullOrEmpty(filter))
            return all.Take(10).ToList();

        return all
            .Where(item => item.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();
    }
}
