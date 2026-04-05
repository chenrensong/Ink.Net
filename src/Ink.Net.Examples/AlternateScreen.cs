// -----------------------------------------------------------------------
// AlternateScreen.cs — 1:1 port of examples/alternate-screen/alternate-screen.tsx
// A snake game rendered in the terminal's alternate screen buffer.
// -----------------------------------------------------------------------

using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Dom;
using Ink.Net.Input;
using Ink.Net.Rendering;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Snake game running on the alternate screen buffer.
/// <para>Aligned with JS <c>examples/alternate-screen/alternate-screen.tsx</c>.</para>
/// </summary>
public static class AlternateScreenExample
{
    // ─── Constants ────────────────────────────────────────────────
    private const string HeadChar = "OO"; // Console-safe snake head
    private const string BodyChar = "**"; // Console-safe body
    private const string FoodChar = "##"; // Console-safe food
    private const string EmptyCell = "  ";
    private const int TickMs = 150;
    private const int BoardWidth = 20;
    private const int BoardHeight = 15;

    private static readonly string[] RainbowColors = { "Red", "Yellow", "Green", "Cyan", "Blue", "Magenta" };
    private static readonly string BorderH = new string('─', BoardWidth * 2);
    private static readonly string BorderTop = $"┌{BorderH}┐";
    private static readonly string BorderBottom = $"└{BorderH}┘";

    // ─── Types ────────────────────────────────────────────────────

    private record struct Point(int X, int Y);

    private enum Direction { Up, Down, Left, Right }

    private sealed class GameState
    {
        public List<Point> Snake { get; set; } = new();
        public Point Food { get; set; }
        public int Score { get; set; }
        public bool GameOver { get; set; }
        public bool Won { get; set; }
        public int Frame { get; set; }
    }

    // ─── Game Logic ───────────────────────────────────────────────

    private static readonly Dictionary<Direction, Direction> Opposites = new()
    {
        { Direction.Up, Direction.Down },
        { Direction.Down, Direction.Up },
        { Direction.Left, Direction.Right },
        { Direction.Right, Direction.Left },
    };

    private static readonly Dictionary<Direction, Point> Offsets = new()
    {
        { Direction.Up, new Point(0, -1) },
        { Direction.Down, new Point(0, 1) },
        { Direction.Left, new Point(-1, 0) },
        { Direction.Right, new Point(1, 0) },
    };

    private static readonly Random Rng = new();

    private static Point RandomPosition(IReadOnlyList<Point> exclude)
    {
        while (true)
        {
            var p = new Point(Rng.Next(BoardWidth), Rng.Next(BoardHeight));
            bool excluded = false;
            foreach (var seg in exclude)
            {
                if (seg.X == p.X && seg.Y == p.Y) { excluded = true; break; }
            }
            if (!excluded) return p;
        }
    }

    private static GameState CreateInitialState()
    {
        var snake = new List<Point> { new(10, 7), new(9, 7), new(8, 7) };
        return new GameState
        {
            Snake = snake,
            Food = RandomPosition(snake),
            Score = 0,
            GameOver = false,
            Won = false,
            Frame = 0,
        };
    }

    /// <summary>
    /// Game reducer.
    /// <para>Aligned with JS <c>gameReducer</c>.</para>
    /// </summary>
    private static GameState GameReducer(GameState state, Direction direction)
    {
        if (state.GameOver) return state;

        var head = state.Snake[0];
        var offset = Offsets[direction];
        var newHead = new Point(head.X + offset.X, head.Y + offset.Y);

        // Wall collision
        if (newHead.X < 0 || newHead.X >= BoardWidth || newHead.Y < 0 || newHead.Y >= BoardHeight)
        {
            return new GameState
            {
                Snake = state.Snake,
                Food = state.Food,
                Score = state.Score,
                GameOver = true,
                Won = false,
                Frame = state.Frame,
            };
        }

        bool ateFood = newHead.X == state.Food.X && newHead.Y == state.Food.Y;

        // Self collision — check against all segments except the last (which will move away),
        // unless we ate food (in which case the tail doesn't move)
        var checkSegments = ateFood ? state.Snake : state.Snake.GetRange(0, state.Snake.Count - 1);
        foreach (var seg in checkSegments)
        {
            if (seg.X == newHead.X && seg.Y == newHead.Y)
            {
                return new GameState
                {
                    Snake = state.Snake,
                    Food = state.Food,
                    Score = state.Score,
                    GameOver = true,
                    Won = false,
                    Frame = state.Frame,
                };
            }
        }

        var newSnake = new List<Point> { newHead };
        newSnake.AddRange(state.Snake);
        if (!ateFood) newSnake.RemoveAt(newSnake.Count - 1);

        // Win condition: filled the board
        if (ateFood && newSnake.Count == BoardWidth * BoardHeight)
        {
            return new GameState
            {
                Snake = newSnake,
                Food = state.Food,
                Score = state.Score + 1,
                GameOver = true,
                Won = true,
                Frame = state.Frame + 1,
            };
        }

        return new GameState
        {
            Snake = newSnake,
            Food = ateFood ? RandomPosition(newSnake) : state.Food,
            Score = state.Score + (ateFood ? 1 : 0),
            GameOver = false,
            Won = false,
            Frame = state.Frame + 1,
        };
    }

    // ─── Board Rendering ──────────────────────────────────────────

    private static string BuildBoard(IReadOnlyList<Point> snake, Point food)
    {
        var headKey = $"{snake[0].X},{snake[0].Y}";
        var snakeSet = new HashSet<string>();
        foreach (var seg in snake) snakeSet.Add($"{seg.X},{seg.Y}");

        var rows = new List<string> { BorderTop };
        for (int y = 0; y < BoardHeight; y++)
        {
            var row = "│";
            for (int x = 0; x < BoardWidth; x++)
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

    // ─── Main ─────────────────────────────────────────────────────

    public static async Task RunAsync()
    {
        var state = CreateInitialState();
        var direction = Direction.Right;

        var app = InkApplication.Create(b => BuildTree(b, state, direction), new InkApplicationOptions
        {
            AlternateScreen = true,
            ExitOnCtrlC = false,
        });

        // Input handler
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
                direction = Direction.Right;
                app.Rerender(b => BuildTree(b, state, direction));
                return;
            }

            if (state.GameOver) return;

            if (key.UpArrow && direction != Direction.Down) direction = Direction.Up;
            else if (key.DownArrow && direction != Direction.Up) direction = Direction.Down;
            else if (key.LeftArrow && direction != Direction.Right) direction = Direction.Left;
            else if (key.RightArrow && direction != Direction.Left) direction = Direction.Right;
        });

        // Game loop
        _ = Task.Run(async () =>
        {
            while (!app.Lifecycle.HasExited)
            {
                await Task.Delay(TickMs);
                if (app.Lifecycle.HasExited) break;

                state = GameReducer(state, direction);
                app.Rerender(b => BuildTree(b, state, direction));
            }
        });

        await app.WaitUntilExit();
        app.Dispose();
    }

    /// <summary>Helper to wrap text with ANSI bold escape codes.</summary>
    private static string Bold(string text) => $"\x1b[1m{text}\x1b[22m";

    private static TreeNode[] BuildTree(TreeBuilder b, GameState game, Direction dir)
    {
        var titleColor = RainbowColors[game.Frame % RainbowColors.Length];
        var board = BuildBoard(game.Snake, game.Food);
        int columns = 100;
        try { columns = Console.WindowWidth; } catch { /* ignore */ }
        int boardWidthChars = BoardWidth * 2 + 2;
        int marginLeft = Math.Max((columns - boardWidthChars) / 2, 0);

        var children = new List<TreeNode>();

        // Title
        children.Add(b.Box(new InkStyle { JustifyContent = JustifyContentMode.Center }, new[]
        {
            b.Text(Bold(Colorizer.Colorize("Snake Game", titleColor, ColorType.Foreground)))
        }));

        // Score
        children.Add(b.Box(new InkStyle { JustifyContent = JustifyContentMode.Center, MarginTop = 1 }, new[]
        {
            b.Text(Bold(Colorizer.Colorize($"Score: {game.Score}", "Yellow", ColorType.Foreground)))
        }));

        // Board
        children.Add(b.Box(new InkStyle { MarginLeft = marginLeft, MarginTop = 1 }, new[]
        {
            b.Text(board)
        }));

        // Footer
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
