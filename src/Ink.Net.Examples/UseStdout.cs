// Ported from examples/use-stdout/use-stdout.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Rendering;
using Ink.Net.Styles;
using Ink.Net.Terminal;

namespace Ink.Net.Examples;

/// <summary>
/// Stdout writing demo — ported from JS Ink examples/use-stdout/use-stdout.tsx.
/// Shows terminal dimensions and writes to stdout periodically.
/// </summary>
public static class UseStdoutExample
{
    public static async Task RunAsync()
    {
        var (columns, rows) = TerminalUtils.GetWindowSize();

        var instance = InkApp.Render(b => BuildUI(b, columns, rows));

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                await Task.Delay(1000, cts.Token);
                Console.Out.Write("Hello from Ink.Net to stdout\n");

                // Re-read in case of resize
                (columns, rows) = TerminalUtils.GetWindowSize();
                instance.Rerender(b => BuildUI(b, columns, rows));
            }
        }
        catch (OperationCanceledException) { }

        instance.Unmount();
    }

    private static TreeNode[] BuildUI(TreeBuilder b, int columns, int rows)
    {
        return new[]
        {
            b.Box(new InkStyle
            {
                FlexDirection = FlexDirectionMode.Column,
                PaddingLeft = 2, PaddingRight = 2,
                PaddingTop = 1, PaddingBottom = 1,
            }, new[]
            {
                b.Text("Terminal dimensions:"),
                b.Box(new InkStyle { MarginTop = 1 }, new[]
                {
                    b.Text($"Width: {columns}"),
                }),
                b.Text($"Height: {rows}"),
            })
        };
    }
}
