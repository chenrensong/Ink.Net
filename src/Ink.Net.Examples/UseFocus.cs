// Ported from examples/use-focus/use-focus.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Input;
using Ink.Net.Rendering;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Focus management demo — ported from JS Ink examples/use-focus/use-focus.tsx.
/// Demonstrates Tab/Shift+Tab navigation between focusable items.
/// </summary>
public static class UseFocusExample
{
    public static async Task RunAsync()
    {
        var app = InkApplication.Create(b => BuildUI(b, null, null, null), new InkApplicationOptions
        {
            ExitOnCtrlC = true,
        });

        // Register 3 focusable items
        var focus1 = app.Focus.Add("first", new FocusOptions { AutoFocus = true });
        var focus2 = app.Focus.Add("second");
        var focus3 = app.Focus.Add("third");

        // Re-render when focus changes
        app.Focus.ActiveIdChanged += _ => app.Rerender(b => BuildUI(b, focus1, focus2, focus3));

        // Initial render with focus state
        app.Rerender(b => BuildUI(b, focus1, focus2, focus3));

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            app.Lifecycle.Exit();
        };

        // Read stdin in a loop
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

        focus1.Dispose();
        focus2.Dispose();
        focus3.Dispose();
        app.Dispose();
    }

    private static TreeNode[] BuildUI(TreeBuilder b,
        FocusRegistration? f1, FocusRegistration? f2, FocusRegistration? f3)
    {
        return new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Padding = 1 }, new[]
            {
                b.Box(new InkStyle { MarginBottom = 1 }, new[]
                {
                    b.Text("Press Tab to focus next element, Shift+Tab to focus previous element, Esc to reset focus."),
                }),
                MakeItem(b, "First", f1?.IsFocused ?? false),
                MakeItem(b, "Second", f2?.IsFocused ?? false),
                MakeItem(b, "Third", f3?.IsFocused ?? false),
            })
        };
    }

    private static TreeNode MakeItem(TreeBuilder b, string label, bool isFocused)
    {
        string text = isFocused
            ? $"{label} " + Colorizer.Colorize("(focused)", "green", ColorType.Foreground)
            : label;
        return b.Text(text);
    }
}
