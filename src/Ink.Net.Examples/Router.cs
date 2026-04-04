// Ported from examples/router/router.tsx
// Note: JS uses react-router; C# uses a simple state-machine approach
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Input;
using Ink.Net.Rendering;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Router example — ported from JS Ink examples/router/router.tsx.
/// Demonstrates multi-page navigation with keyboard input.
/// JS uses react-router; C# uses a simple enum-based state machine.
/// </summary>
public static class RouterExample
{
    private enum Page { Home, About }

    public static async Task RunAsync()
    {
        var currentPage = Page.Home;

        var app = InkApplication.Create(b => BuildUI(b, currentPage));

        app.Input.Register((input, key) =>
        {
            if (input == "q")
            {
                app.Lifecycle.Exit();
                return;
            }

            if (key.Return)
            {
                currentPage = currentPage == Page.Home ? Page.About : Page.Home;
                app.Rerender(b => BuildUI(b, currentPage));
            }
        });

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

    private static TreeNode[] BuildUI(TreeBuilder b, Page page)
    {
        return page switch
        {
            Page.Home => new[]
            {
                b.Box(style: new InkStyle { FlexDirection = FlexDirectionMode.Column }, children: new[]
                {
                    b.Text("Home",
                        transform: (line, _) => Colorizer.Colorize(line, "green", ColorType.Foreground)),
                    b.Text("Press Enter to go to About, or \"q\" to quit."),
                }),
            },
            Page.About => new[]
            {
                b.Box(style: new InkStyle { FlexDirection = FlexDirectionMode.Column }, children: new[]
                {
                    b.Text("About",
                        transform: (line, _) => Colorizer.Colorize(line, "blue", ColorType.Foreground)),
                    b.Text("Press Enter to go back Home, or \"q\" to quit."),
                }),
            },
            _ => new[] { b.Text("Unknown page") },
        };
    }
}
