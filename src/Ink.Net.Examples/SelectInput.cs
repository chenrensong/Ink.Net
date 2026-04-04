// Ported from examples/select-input/select-input.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Input;
using Ink.Net.Rendering;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Select input demo — ported from JS Ink examples/select-input/select-input.tsx.
/// Use Up/Down arrows to select a color. Press Ctrl+C to exit.
/// </summary>
public static class SelectInputExample
{
    private static readonly string[] Items = { "Red", "Green", "Blue", "Yellow", "Magenta", "Cyan" };

    public static async Task RunAsync()
    {
        int selectedIndex = 0;

        var app = InkApplication.Create(b => BuildUI(b, selectedIndex), new InkApplicationOptions
        {
            ExitOnCtrlC = true,
        });

        app.Input.Register((input, key) =>
        {
            if (key.UpArrow)
            {
                selectedIndex = selectedIndex == 0 ? Items.Length - 1 : selectedIndex - 1;
            }

            if (key.DownArrow)
            {
                selectedIndex = selectedIndex == Items.Length - 1 ? 0 : selectedIndex + 1;
            }

            app.Rerender(b => BuildUI(b, selectedIndex));
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

    private static TreeNode[] BuildUI(TreeBuilder b, int selectedIndex)
    {
        var items = new List<TreeNode>();
        items.Add(b.Text("Select a color:"));

        for (int i = 0; i < Items.Length; i++)
        {
            bool isSelected = i == selectedIndex;
            string label = isSelected ? $"> {Items[i]}" : $"  {Items[i]}";
            string text = isSelected
                ? Colorizer.Colorize(label, "blue", ColorType.Foreground)
                : label;
            items.Add(b.Text(text));
        }

        return new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, items.ToArray())
        };
    }
}
