// -----------------------------------------------------------------------
// AlternateScreen.cs — 1:1 port of examples/alternate-screen/alternate-screen.tsx
// A snake game rendered in the terminal's alternate screen buffer.
// -----------------------------------------------------------------------

using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Input;
using Ink.Net.Rendering;
using Ink.Net.SnakeGame;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Snake game running on the alternate screen buffer.
/// <para>Aligned with JS <c>examples/alternate-screen/alternate-screen.tsx</c>.</para>
/// </summary>
public static class AlternateScreenExample
{
    private const string HeadChar = "OO";
    private const string BodyChar = "**";
    private const string FoodChar = "##";
    private const string EmptyCell = "  ";
    private const int TickMs = 150;

    private static readonly string[] RainbowColors = { "Red", "Yellow", "Green", "Cyan", "Blue", "Magenta" };
    private static readonly string BorderH = new string('─', SnakeGameEngine.BoardWidth * 2);
    private static readonly string BorderTop = $"┌{BorderH}┐";
    private static readonly string BorderBottom = $"└{BorderH}┘";

    private static readonly Random Rng = new();

    private static SnakePoint RandomPosition(IReadOnlyList<SnakePoint> exclude)
    {
        while (true)
        {
            var p = new SnakePoint(Rng.Next(SnakeGameEngine.BoardWidth), Rng.Next(SnakeGameEngine.BoardHeight));
            bool excluded = false;
            foreach (var seg in exclude)
            {
                if (seg.X == p.X && seg.Y == p.Y) { excluded = true; break; }
            }

            if (!excluded) return p;
        }
    }

    private static SnakeGameState CreateInitialState()
    {
        var snake = new List<SnakePoint> { new(10, 7), new(9, 7), new(8, 7) };
        return new SnakeGameState
        {
            Snake = snake,
            Food = RandomPosition(snake),
            Score = 0,
            GameOver = false,
            Won = false,
            Frame = 0,
        };
    }

    private static string BuildBoard(IReadOnlyList<SnakePoint> snake, SnakePoint food)
    {
        var headKey = $"{snake[0].X},{snake[0].Y}";
        var snakeSet = new HashSet<string>();
        foreach (var seg in snake) snakeSet.Add($"{seg.X},{seg.Y}");

        var rows = new List<string> { BorderTop };
        for (int y = 0; y < SnakeGameEngine.BoardHeight; y++)
        {
            var row = "│";
            for (int x = 0; x < SnakeGameEngine.BoardWidth; x++)
            {
                var key = $"{x},{y}";
                if (key == headKey) row += HeadChar;
                else if (snakeSet.Contains(key)) row += BodyChar;
                else if (food.X == x && food.Y == y) row += FoodChar;
                else row += EmptyCell;
            }

            row += "│";
            rows.Add(row);
        }

        rows.Add(BorderBottom);
        return string.Join("\n", rows);
    }

    public static async Task RunAsync()
    {
        var state = CreateInitialState();
        var direction = SnakeDirection.Right;

        var app = InkApplication.Create(b => BuildTree(b, state), new InkApplicationOptions
        {
            AlternateScreen = true,
            ExitOnCtrlC = false,
        });

        app.Input.Register((input, key) =>
        {
            if (input == "q")
            {
                app.Lifecycle.Exit();
                return;
            }

            if (state.GameOver && input == "r")
            {
                state = CreateInitialState();
                direction = SnakeDirection.Right;
                app.Rerender(b => BuildTree(b, state));
                return;
            }

            if (state.GameOver) return;

            if (key.UpArrow && direction != SnakeDirection.Down) direction = SnakeDirection.Up;
            else if (key.DownArrow && direction != SnakeDirection.Up) direction = SnakeDirection.Down;
            else if (key.LeftArrow && direction != SnakeDirection.Right) direction = SnakeDirection.Left;
            else if (key.RightArrow && direction != SnakeDirection.Left) direction = SnakeDirection.Right;
        });

        _ = Task.Run(async () =>
        {
            while (!app.Lifecycle.HasExited)
            {
                await Task.Delay(TickMs);
                if (app.Lifecycle.HasExited) break;

                state = SnakeGameEngine.Tick(state, direction, RandomPosition);
                app.Rerender(b => BuildTree(b, state));
            }
        });

        await app.WaitUntilExit();
        app.Dispose();
    }

    private static string Bold(string text) => $"\x1b[1m{text}\x1b[22m";

    private static TreeNode[] BuildTree(TreeBuilder b, SnakeGameState game)
    {
        var titleColor = RainbowColors[game.Frame % RainbowColors.Length];
        var board = BuildBoard(game.Snake, game.Food);
        int columns = 100;
        try { columns = Console.WindowWidth; } catch { /* ignore */ }
        int boardWidthChars = SnakeGameEngine.BoardWidth * 2 + 2;
        int marginLeft = Math.Max((columns - boardWidthChars) / 2, 0);

        var children = new List<TreeNode>();

        children.Add(b.Box(new InkStyle { JustifyContent = JustifyContentMode.Center }, new[]
        {
            b.Text(Bold(Colorizer.Colorize("Snake Game", titleColor, ColorType.Foreground)))
        }));

        children.Add(b.Box(new InkStyle { JustifyContent = JustifyContentMode.Center, MarginTop = 1 }, new[]
        {
            b.Text(Bold(Colorizer.Colorize($"Score: {game.Score}", "Yellow", ColorType.Foreground)))
        }));

        children.Add(b.Box(new InkStyle { MarginLeft = marginLeft, MarginTop = 1 }, new[]
        {
            b.Text(board)
        }));

        if (game.GameOver)
        {
            var label = game.Won ? "You Win!" : "Game Over!";
            children.Add(b.Box(new InkStyle { JustifyContent = JustifyContentMode.Center, MarginTop = 1 }, new[]
            {
                b.Text(Bold(Colorizer.Colorize(label, "Red", ColorType.Foreground))),
                b.Text(Colorizer.Dim(" r: restart | q: quit"))
            }));
        }
        else
        {
            children.Add(b.Box(new InkStyle { JustifyContent = JustifyContentMode.Center, MarginTop = 1 }, new[]
            {
                b.Text(Colorizer.Dim("Arrow keys: move | Eat ## to grow | q: quit"))
            }));
        }

        return children.ToArray();
    }
}
