// Ported from examples/render-throttle/index.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Render throttle example — ported from JS Ink examples/render-throttle/index.tsx.
/// Demonstrates maxFps throttling: updates every 10ms but renders are throttled to 10fps.
/// </summary>
public static class RenderThrottleExample
{
    public static async Task RunAsync()
    {
        int count = 0;

        // Use maxFps=10 to throttle renders to ~100ms intervals
        var instance = InkApp.Render(b => new[]
        {
            b.Box(style: new InkStyle { FlexDirection = FlexDirectionMode.Column, Padding = 1 }, children: new[]
            {
                b.Text($"Counter: {count}"),
                b.Text("This updates every 10ms but renders are throttled"),
                b.Text("Press Ctrl+C to exit"),
            }),
        }, new RenderOptions
        {
            MaxFps = 10,  // Only render at 10fps (every ~100ms)
        });

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                await Task.Delay(10, cts.Token);
                count++;
                instance.Rerender(b => new[]
                {
                    b.Box(style: new InkStyle { FlexDirection = FlexDirectionMode.Column, Padding = 1 }, children: new[]
                    {
                        b.Text($"Counter: {count}"),
                        b.Text("This updates every 10ms but renders are throttled"),
                        b.Text("Press Ctrl+C to exit"),
                    }),
                });
            }
        }
        catch (OperationCanceledException) { }

        instance.Unmount();
        Console.WriteLine($"\nFinal count: {count}");
    }
}
