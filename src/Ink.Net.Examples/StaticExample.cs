// Ported from examples/static/static.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Rendering;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Static (accumulated items) demo — ported from JS Ink examples/static/static.tsx.
/// Shows tests completing one by one with a running count.
/// </summary>
public static class StaticExample
{
    public static async Task RunAsync()
    {
        var tests = new List<string>();

        var instance = InkApp.Render(b => BuildUI(b, tests));

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        try
        {
            for (int i = 0; i < 10 && !cts.Token.IsCancellationRequested; i++)
            {
                await Task.Delay(100, cts.Token);
                tests.Add($"Test #{i + 1}");
                instance.Rerender(b => BuildUI(b, tests));
            }

            // Keep showing final result for a moment
            await Task.Delay(500, cts.Token);
        }
        catch (OperationCanceledException) { }

        instance.Unmount();
    }

    private static TreeNode[] BuildUI(TreeBuilder b, List<string> tests)
    {
        var children = new List<TreeNode>();

        // Completed tests
        foreach (var test in tests)
        {
            children.Add(b.Text($"✔ {test}", new InkStyle { Color = "green" }));
        }

        // Footer
        children.Add(b.Box(new InkStyle { MarginTop = 1 }, new[]
        {
            b.Text(Colorizer.Dim($"Completed tests: {tests.Count}")),
        }));

        return new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, children.ToArray())
        };
    }
}
