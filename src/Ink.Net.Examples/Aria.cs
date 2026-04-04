// Ported from examples/aria/aria.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Dom;
using Ink.Net.Input;
using Ink.Net.Rendering;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Aria accessibility example — ported from JS Ink examples/aria/aria.tsx.
/// Demonstrates screen reader support with aria-role, aria-state, aria-label, and aria-hidden.
/// </summary>
public static class AriaExample
{
    public static async Task RunAsync()
    {
        bool isChecked = false;

        var app = InkApplication.Create(b => BuildUI(b, isChecked), new InkApplicationOptions
        {
            IsScreenReaderEnabled = true,
        });

        app.Input.Register((input, key) =>
        {
            if (input == " ")
            {
                isChecked = !isChecked;
                app.Rerender(b => BuildUI(b, isChecked));
            }

            if (input == "q")
            {
                app.Lifecycle.Exit();
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

    private static TreeNode[] BuildUI(TreeBuilder b, bool isChecked)
    {
        return new[]
        {
            b.Box(style: new InkStyle { FlexDirection = FlexDirectionMode.Column }, children: new[]
            {
                b.Text("Press spacebar to toggle the checkbox. Press \"q\" to quit."),
                b.Text("This example is best experienced with a screen reader."),
                b.Box(style: new InkStyle { MarginTop = 1 }, children: new[]
                {
                    b.Box(
                        ariaRole: AccessibilityRole.Checkbox,
                        ariaState: new AccessibilityState { Checked = isChecked },
                        children: new[]
                        {
                            b.Text(isChecked ? "[x] Accept terms" : "[ ] Accept terms"),
                        }),
                }),
                b.Box(style: new InkStyle { MarginTop = 1 }, children: new[]
                {
                    b.Text("This text is hidden from screen readers.", ariaHidden: true),
                }),
            }),
        };
    }
}
