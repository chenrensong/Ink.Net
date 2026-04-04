// Ported from examples/terminal-resize/terminal-resize.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Rendering;
using Ink.Net.Styles;
using Ink.Net.Terminal;

namespace Ink.Net.Examples;

/// <summary>
/// Terminal resize demo — ported from JS Ink examples/terminal-resize/terminal-resize.tsx.
/// Shows current terminal dimensions. Resize the terminal to see values update.
/// </summary>
public static class TerminalResizeExample
{
    public static async Task RunAsync()
    {
        var app = InkApplication.Create(b => BuildUI(b), new InkApplicationOptions
        {
            ExitOnCtrlC = true,
        });

        // Re-render on window resize
        app.WindowSize.Resized += _ => app.Rerender(b => BuildUI(b));

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            app.Lifecycle.Exit();
        };

        _ = Task.Run(async () =>
        {
            try
            {
                char[] buf = new char[256];
                while (!app.Lifecycle.HasExited)
                {
                    int n = await Console.In.ReadAsync(buf, 0, buf.Length);
                    if (n > 0) app.HandleInput(new string(buf, 0, n));
                }
            }
            catch { }
        });

        try { await app.WaitUntilExit(); } catch { }
        app.Dispose();
    }

    private static TreeNode[] BuildUI(TreeBuilder b)
    {
        var (columns, rows) = TerminalUtils.GetWindowSize();

        return new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Padding = 1 }, new[]
            {
                b.Text(Colorizer.Colorize("Terminal Size", "cyan", ColorType.Foreground)),
                b.Text($"Columns: {columns}"),
                b.Text($"Rows: {rows}"),
                b.Box(new InkStyle { MarginTop = 1 }, new[]
                {
                    b.Text(Colorizer.Dim("Resize your terminal to see the values update. Press Ctrl+C to exit.")),
                }),
            })
        };
    }
}
