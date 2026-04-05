// Ported from examples/jest/jest.tsx — Jest-like test runner UI (Static + running rows + summary).
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Rendering;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Fake concurrent test run with Static completed lines and dynamic running lines.
/// </summary>
public static class JestStyleExample
{
    private sealed class TestResultRow
    {
        public string Path { get; init; } = "";
        public string Status { get; init; } = ""; // runs | pass | fail
    }

    private static readonly string[] Paths =
    [
        "tests/login.js",
        "tests/signup.js",
        "tests/forgot-password.js",
        "tests/reset-password.js",
        "tests/view-profile.js",
        "tests/edit-profile.js",
        "tests/delete-profile.js",
        "tests/posts.js",
        "tests/post.js",
        "tests/comments.js",
    ];

    public static async Task RunAsync()
    {
        var completed = new List<TestResultRow>();
        var running = new List<TestResultRow>();
        var startTime = DateTime.UtcNow;
        var gate = new object();

        TreeNode TestLine(TreeBuilder b, TestResultRow t)
        {
            var bg = t.Status switch
            {
                "runs" => "yellow",
                "pass" => "green",
                "fail" => "red",
                _ => "white",
            };

            var badge = $" {t.Status.ToUpperInvariant()} ";
            var labeled = Colorizer.Colorize(Colorizer.Colorize(badge, "black", ColorType.Foreground), bg, ColorType.Background);
            var parts = t.Path.Split('/');
            var folder = parts.Length > 1 ? parts[0] + "/" : "";
            var name = parts.Length > 1 ? parts[1] : t.Path;

            return b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Row }, new[]
            {
                b.Text(labeled),
                b.Box(new InkStyle { MarginLeft = 1 }, new[]
                {
                    b.Text(Colorizer.Dim(folder)),
                    b.Text(Colorizer.Colorize(name, "white", ColorType.Foreground)),
                }),
            });
        }

        TreeNode[] BuildUI(TreeBuilder b)
        {
            List<TestResultRow> comp;
            List<TestResultRow> run;
            lock (gate)
            {
                comp = completed.ToList();
                run = running.ToList();
            }

            var staticChildren = comp.Select(t => TestLine(b, t)).ToArray();
            var staticNode = b.Static(staticChildren);

            var col = new List<TreeNode> { staticNode };

            if (run.Count > 0)
            {
                var runningInner = run.Select(t => TestLine(b, t)).ToArray();
                col.Add(b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, MarginTop = 1 }, runningInner));
            }

            var passed = comp.Count(t => t.Status == "pass");
            var failed = comp.Count(t => t.Status == "fail");
            var elapsed = DateTime.UtcNow - startTime;
            var timeStr = $"{elapsed.TotalSeconds:F1}s";

            col.Add(SummaryBox(b, run.Count == 0 && comp.Count > 0, passed, failed, timeStr));

            return new[] { b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, col.ToArray()) };
        }

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        var instance = InkApp.Render(b => BuildUI(b));

        var concurrency = new SemaphoreSlim(4);
        var random = new Random();

        void Rerender()
        {
            try { instance.Rerender(b => BuildUI(b)); } catch { /* unmounted */ }
        }

        async Task RunOneAsync(string path)
        {
            await concurrency.WaitAsync(cts.Token);
            try
            {
                lock (gate)
                {
                    running.Add(new TestResultRow { Path = path, Status = "runs" });
                }

                Rerender();

                await Task.Delay(random.Next(100, 1000), cts.Token);

                lock (gate)
                {
                    running.RemoveAll(r => r.Path == path);
                    completed.Add(new TestResultRow
                    {
                        Path = path,
                        Status = random.NextDouble() < 0.5 ? "pass" : "fail",
                    });
                }

                Rerender();
            }
            catch (OperationCanceledException) { }
            finally
            {
                concurrency.Release();
            }
        }

        var workers = Paths.Select(p => RunOneAsync(p)).ToArray();

        try
        {
            await Task.WhenAll(workers);
            await Task.Delay(1500, cts.Token);
        }
        catch (OperationCanceledException) { }

        instance.Unmount();
    }

    private static TreeNode SummaryBox(TreeBuilder b, bool isFinished, int passed, int failed, string timeStr)
    {
        var suiteLine = new List<TreeNode>
        {
            b.Box(new InkStyle { Width = DimensionValue.Points(14) }, new[] { b.Text(Colorizer.Colorize("Test Suites:", "white", ColorType.Foreground)) }),
        };

        if (failed > 0)
            suiteLine.Add(b.Text(Colorizer.Colorize($"{failed} failed, ", "red", ColorType.Foreground)));
        if (passed > 0)
            suiteLine.Add(b.Text(Colorizer.Colorize($"{passed} passed, ", "green", ColorType.Foreground)));
        suiteLine.Add(b.Text($"{passed + failed} total"));

        var timeLine = new List<TreeNode>
        {
            b.Box(new InkStyle { Width = DimensionValue.Points(14) }, new[] { b.Text(Colorizer.Colorize("Time:", "white", ColorType.Foreground)) }),
            b.Text(timeStr),
        };

        var col = new List<TreeNode>
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Row, MarginTop = 1 }, suiteLine.ToArray()),
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Row }, timeLine.ToArray()),
        };

        if (isFinished)
        {
            col.Add(b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Row }, new[]
            {
                b.Text(Colorizer.Dim("Ran all test suites.")),
            }));
        }

        return b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, MarginTop = 1 }, col.ToArray());
    }
}
