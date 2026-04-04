// Ported from examples/use-input/use-input.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Input;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Robot face movement demo — ported from JS Ink examples/use-input/use-input.tsx.
/// Use arrow keys to move the face. Press "q" to exit.
/// </summary>
public static class UseInputExample
{
    public static async Task RunAsync()
    {
        int x = 1, y = 1;

        var app = InkApplication.Create(b => BuildUI(b, x, y), new InkApplicationOptions
        {
            ExitOnCtrlC = true,
        });

        app.Input.Register((input, key) =>
        {
            if (input == "q")
            {
                app.Lifecycle.Exit();
                return;
            }

            if (key.LeftArrow) x = Math.Max(1, x - 1);
            if (key.RightArrow) x = Math.Min(20, x + 1);
            if (key.UpArrow) y = Math.Max(1, y - 1);
            if (key.DownArrow) y = Math.Min(10, y + 1);

            app.Rerender(b => BuildUI(b, x, y));
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

    private static TreeNode[] BuildUI(TreeBuilder b, int x, int y)
    {
        return new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Text("Use arrow keys to move the face. Press \"q\" to exit."),
                b.Box(new InkStyle { Height = 12, PaddingLeft = x, PaddingTop = y }, new[]
                {
                    b.Text("^_^"),
                }),
            })
        };
    }
}
