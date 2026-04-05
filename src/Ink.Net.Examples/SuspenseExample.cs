// Ported from examples/suspense/suspense.tsx — async load + Loading fallback (no React Suspense).
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Rendering;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Shows "Loading..." then resolved text after a delay, mirroring the JS Suspense + throw promise demo.
/// </summary>
public static class SuspenseExample
{
    public static async Task RunAsync()
    {
        var loaded = false;
        var message = "Hello World";

        TreeNode[] BuildUI(TreeBuilder b) => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                loaded
                    ? b.Text(message)
                    : b.Text(Colorizer.Colorize("Loading...", "yellow", ColorType.Foreground)),
            }),
        };

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        var instance = InkApp.Render(b => BuildUI(b));

        try
        {
            await Task.Delay(500, cts.Token);
            loaded = true;
            instance.Rerender(b => BuildUI(b));
            await Task.Delay(300, cts.Token);
        }
        catch (OperationCanceledException) { }

        instance.Unmount();
    }
}
