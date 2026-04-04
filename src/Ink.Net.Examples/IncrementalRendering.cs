// Ported from examples/incremental-rendering/incremental-rendering.tsx
// Simplified version using imperative API (no React hooks).
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Rendering;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Incremental rendering demo — ported from JS Ink examples/incremental-rendering/incremental-rendering.tsx.
/// Shows live updates with progress bars and a service list.
/// </summary>
public static class IncrementalRendering
{
    private static readonly string[] Services = new[]
    {
        "Server Authentication Module - JWT & OAuth2",
        "Database Connection Pool - PostgreSQL cluster",
        "API Gateway Service - Rate limiting & routing",
        "User Profile Manager - Redis cache layer",
        "Payment Processing Engine - Stripe integration",
        "Email Notification Queue - SendGrid delivery",
        "File Storage Handler - S3 + CDN",
        "Search Indexer Service - Elasticsearch",
        "Metrics Aggregation Pipeline - Prometheus",
        "WebSocket Connection Manager - Chat & Notify",
    };

    public static async Task RunAsync()
    {
        int counter = 0;
        int progress1 = 0, progress2 = 0, progress3 = 0;
        int selectedIndex = 0;
        var rng = new Random();

        TreeNode[] BuildUI(TreeBuilder b)
        {
            string MakeBar(int pct)
            {
                int filled = pct / 5;
                int empty = 20 - filled;
                return new string('█', filled) + new string('░', empty);
            }

            var serviceNodes = new List<TreeNode>();
            for (int i = 0; i < Services.Length; i++)
            {
                string prefix = i == selectedIndex ? "> " : "  ";
                serviceNodes.Add(b.Text($"{prefix}{Services[i]}"));
            }

            return new[]
            {
                b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
                {
                    // Header
                    b.Box(new InkStyle
                    {
                        BorderStyle = "round", PaddingLeft = 2, PaddingRight = 2,
                        PaddingTop = 1, PaddingBottom = 1,
                        FlexDirection = FlexDirectionMode.Column
                    }, new[]
                    {
                        b.Text("Incremental Rendering Demo"),
                        b.Text($"Time: {DateTime.Now:HH:mm:ss} | Updates: {counter}"),
                        b.Text($"Progress 1: {MakeBar(progress1)} {progress1}%"),
                        b.Text($"Progress 2: {MakeBar(progress2)} {progress2}%"),
                        b.Text($"Progress 3: {MakeBar(progress3)} {progress3}%"),
                    }),

                    // Services
                    b.Box(new InkStyle
                    {
                        BorderStyle = "single", MarginTop = 1,
                        PaddingLeft = 2, PaddingRight = 2,
                        PaddingTop = 1, PaddingBottom = 1,
                        FlexDirection = FlexDirectionMode.Column
                    }, serviceNodes.ToArray()),

                    // Footer
                    b.Box(new InkStyle { BorderStyle = "round", MarginTop = 1, PaddingLeft = 2, PaddingRight = 2 }, new[]
                    {
                        b.Text($"Selected: {Services[selectedIndex]}"),
                    }),
                })
            };
        }

        var instance = InkApp.Render(b => BuildUI(b));

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                await Task.Delay(100, cts.Token);
                counter++;
                progress1 = (progress1 + 1) % 101;
                progress2 = (progress2 + 2) % 101;
                progress3 = (progress3 + 3) % 101;
                selectedIndex = counter % Services.Length;

                instance.Rerender(b => BuildUI(b));
            }
        }
        catch (OperationCanceledException) { }

        instance.Unmount();
        Console.WriteLine($"\nFinal updates: {counter}");
    }
}
