// Ported from examples/cursor-ime/cursor-ime.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Input;
using Ink.Net.Styles;
using Ink.Net.Terminal;
using Ink.Net.Text;

namespace Ink.Net.Examples;

/// <summary>
/// Cursor IME demo — ported from JS Ink examples/cursor-ime/cursor-ime.tsx.
/// Demonstrates cursor positioning for text input with wide character support.
/// </summary>
public static class CursorImeExample
{
    public static async Task RunAsync()
    {
        string text = "";

        var app = InkApplication.Create(b => BuildUI(b, text), new InkApplicationOptions
        {
            ExitOnCtrlC = true,
        });

        app.Input.Register((input, key) =>
        {
            if (key.Backspace || key.Delete)
            {
                if (text.Length > 0)
                    text = text[..^1];
            }
            else if (!key.Ctrl && !key.Meta && input.Length > 0)
            {
                text += input;
            }

            // Update cursor position using string width for correct wide character support
            string prompt = "> ";
            int cursorX = StringWidthHelper.GetStringWidth(prompt + text);
            app.Cursor.SetCursorPosition(cursorX, 1);

            app.Rerender(b => BuildUI(b, text));
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

    private static TreeNode[] BuildUI(TreeBuilder b, string text)
    {
        return new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Text("Type text (Ctrl+C to exit):"),
                b.Text($"> {text}"),
            })
        };
    }
}
