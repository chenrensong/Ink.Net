// Ported from examples/chat/chat.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Input;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Chat app demo — ported from JS Ink examples/chat/chat.tsx.
/// Type a message and press Enter to send. Press Ctrl+C to exit.
/// </summary>
public static class ChatExample
{
    public static async Task RunAsync()
    {
        string currentInput = "";
        var messages = new List<string>();

        var app = InkApplication.Create(b => BuildUI(b, messages, currentInput), new InkApplicationOptions
        {
            ExitOnCtrlC = true,
        });

        app.Input.Register((input, key) =>
        {
            if (key.Return)
            {
                if (currentInput.Length > 0)
                {
                    messages.Add($"User: {currentInput}");
                    currentInput = "";
                }
            }
            else if (key.Backspace || key.Delete)
            {
                if (currentInput.Length > 0)
                    currentInput = currentInput[..^1];
            }
            else if (!key.Ctrl && !key.Meta && input.Length > 0)
            {
                currentInput += input;
            }

            app.Rerender(b => BuildUI(b, messages, currentInput));
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

    private static TreeNode[] BuildUI(TreeBuilder b, List<string> messages, string currentInput)
    {
        var children = new List<TreeNode>();

        // Messages
        foreach (var msg in messages)
        {
            children.Add(b.Text(msg));
        }

        return new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Padding = 1 }, new[]
            {
                b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column },
                    children.ToArray()),
                b.Box(new InkStyle { MarginTop = 1 }, new[]
                {
                    b.Text($"Enter your message: {currentInput}"),
                }),
            })
        };
    }
}
