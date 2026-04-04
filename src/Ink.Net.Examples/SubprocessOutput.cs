// Ported from examples/subprocess-output/subprocess-output.tsx
using System.Diagnostics;
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Subprocess output demo — ported from JS Ink examples/subprocess-output/subprocess-output.tsx.
/// Runs a subprocess and displays the last 5 lines of output.
/// </summary>
public static class SubprocessOutputExample
{
    public static async Task RunAsync()
    {
        string output = "";

        var instance = InkApp.Render(b => BuildUI(b, output));

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        try
        {
            // Run "dotnet --info" as the subprocess (cross-platform equivalent of npm)
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--info",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process != null)
            {
                var allOutput = new List<string>();

                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                    {
                        allOutput.Add(e.Data);
                        // Keep last 5 lines
                        output = string.Join('\n', allOutput.TakeLast(5));
                        instance.Rerender(b => BuildUI(b, output));
                    }
                };

                process.BeginOutputReadLine();
                await process.WaitForExitAsync(cts.Token);
            }
            else
            {
                output = "Failed to start subprocess";
                instance.Rerender(b => BuildUI(b, output));
            }

            // Wait a bit so user can see the final output
            await Task.Delay(2000, cts.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            output = $"Error: {ex.Message}";
            instance.Rerender(b => BuildUI(b, output));
            try { await Task.Delay(2000, cts.Token); } catch { }
        }

        instance.Unmount();
    }

    private static TreeNode[] BuildUI(TreeBuilder b, string output)
    {
        return new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Padding = 1 }, new[]
            {
                b.Text("Command output:"),
                b.Box(new InkStyle { MarginTop = 1 }, new[]
                {
                    b.Text(string.IsNullOrEmpty(output) ? "(waiting...)" : output),
                }),
            })
        };
    }
}
