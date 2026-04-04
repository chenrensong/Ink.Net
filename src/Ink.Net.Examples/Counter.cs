// Ported from examples/counter/counter.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Rendering;

namespace Ink.Net.Examples;

/// <summary>
/// Live counter example — ported from JS Ink examples/counter/counter.tsx.
/// Demonstrates live rendering with periodic rerender.
/// </summary>
public static class Counter
{
    public static async Task RunAsync()
    {
        var counter = 0;

        var instance = InkApp.Render(b => new[]
        {
            b.Text($"{counter} tests passed",
                transform: (line, _) => Colorizer.Colorize(line, "green", ColorType.Foreground))
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
                await Task.Delay(100, cts.Token);
                counter++;
                instance.Rerender(b => new[]
                {
                    b.Text($"{counter} tests passed",
                        transform: (line, _) => Colorizer.Colorize(line, "green", ColorType.Foreground))
                });
            }
        }
        catch (OperationCanceledException) { }

        instance.Unmount();
        Console.WriteLine($"\nFinal count: {counter}");
    }
}
