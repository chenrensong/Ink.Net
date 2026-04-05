// Ported from examples/concurrent-suspense/concurrent-suspense.tsx
// Cache + pending/resolved rows with periodic rerender (no React Suspense).
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Rendering;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Multiple simulated fetches with fallbacks; optional dynamic row after 2s — like JS concurrent Suspense demo.
/// </summary>
public static class ConcurrentSuspenseExample
{
    private sealed class FetchEntry
    {
        public string Status = "pending"; // pending | resolved
        public string? Data;
    }

    public static async Task RunAsync()
    {
        var cache = new Dictionary<string, FetchEntry>(StringComparer.Ordinal);
        var showMore = false;

        void StartFetch(string key, int delayMs)
        {
            if (cache.ContainsKey(key)) return;
            cache[key] = new FetchEntry();
            _ = Task.Run(async () =>
            {
                await Task.Delay(delayMs);
                if (cache.TryGetValue(key, out var e))
                {
                    e.Status = "resolved";
                    e.Data = $"Data for \"{key}\" (fetched in {delayMs}ms)";
                }
            });
        }

        StartFetch("fast", 200);
        StartFetch("medium", 800);
        StartFetch("slow", 1500);

        TreeNode[] BuildUI(TreeBuilder b)
        {
            var col = new List<TreeNode>
            {
                b.Text(Colorizer.Colorize("Concurrent Suspense Demo", "white", ColorType.Foreground) + " (C# semantic port)"),
                b.Text(Colorizer.Dim("(Multiple async rows; rerender as each fetch completes)")),
                b.Box(new InkStyle { MarginTop = 1 }),
                b.Text("Fast data (200ms):"),
                DataRow(b, cache, "fast", "Loading fast data..."),
                b.Box(new InkStyle { MarginTop = 1 }),
                b.Text("Medium data (800ms):"),
                DataRow(b, cache, "medium", "Loading medium data..."),
                b.Box(new InkStyle { MarginTop = 1 }),
                b.Text("Slow data (1500ms):"),
                DataRow(b, cache, "slow", "Loading slow data..."),
            };

            if (showMore)
            {
                col.Add(b.Box(new InkStyle { MarginTop = 1 }));
                col.Add(b.Text("Dynamically added (500ms):"));
                col.Add(DataRow(b, cache, "dynamic", "Loading dynamic data..."));
            }

            return new[] { b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, col.ToArray()) };
        }

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        var instance = InkApp.Render(b => BuildUI(b));

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(2000, cts.Token);
                showMore = true;
                StartFetch("dynamic", 500);
            }
            catch (OperationCanceledException) { }
        });

        try
        {
            var start = DateTime.UtcNow;
            while ((DateTime.UtcNow - start).TotalSeconds < 5 && !cts.Token.IsCancellationRequested)
            {
                await Task.Delay(100, cts.Token);
                instance.Rerender(b => BuildUI(b));
            }
        }
        catch (OperationCanceledException) { }

        instance.Unmount();
    }

    private static TreeNode DataRow(TreeBuilder b, Dictionary<string, FetchEntry> cache, string key, string loadingMessage)
    {
        if (!cache.TryGetValue(key, out var e) || e.Status == "pending")
        {
            return b.Box(new InkStyle { MarginLeft = 2 }, new[]
            {
                b.Text(Colorizer.Colorize(loadingMessage, "yellow", ColorType.Foreground)),
            });
        }

        return b.Box(new InkStyle { MarginLeft = 2 }, new[]
        {
            b.Text(Colorizer.Colorize(e.Data ?? "", "green", ColorType.Foreground)),
        });
    }
}
