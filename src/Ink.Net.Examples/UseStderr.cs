// Ported from examples/use-stderr/use-stderr.tsx
using Ink.Net;
using Ink.Net.Builder;

namespace Ink.Net.Examples;

/// <summary>
/// Stderr writing demo — ported from JS Ink examples/use-stderr/use-stderr.tsx.
/// Writes to stderr periodically while displaying "Hello World" on stdout.
/// </summary>
public static class UseStderrExample
{
    public static async Task RunAsync()
    {
        var instance = InkApp.Render(b => new[]
        {
            b.Text("Hello World"),
        });

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                await Task.Delay(1000, cts.Token);
                Console.Error.Write("Hello from Ink.Net to stderr\n");
            }
        }
        catch (OperationCanceledException) { }

        instance.Unmount();
    }
}
