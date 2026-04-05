// Pure tick logic — aligned with ink examples/alternate-screen gameReducer (TS).

namespace Ink.Net.SnakeGame;

public static class SnakeGameEngine
{
    public const int BoardWidth = 20;
    public const int BoardHeight = 15;

    private static readonly Dictionary<SnakeDirection, SnakePoint> Offsets = new()
    {
        { SnakeDirection.Up, new SnakePoint(0, -1) },
        { SnakeDirection.Down, new SnakePoint(0, 1) },
        { SnakeDirection.Left, new SnakePoint(-1, 0) },
        { SnakeDirection.Right, new SnakePoint(1, 0) },
    };

    /// <summary>
    /// Advance one tick in <paramref name="direction"/>.
    /// <para>When the snake eats food, <paramref name="randomFood"/> selects the next cell (excluding the new body).</para>
    /// </summary>
    public static SnakeGameState Tick(
        SnakeGameState state,
        SnakeDirection direction,
        Func<IReadOnlyList<SnakePoint>, SnakePoint> randomFood)
    {
        if (state.GameOver) return state;

        var head = state.Snake[0];
        var offset = Offsets[direction];
        var newHead = new SnakePoint(head.X + offset.X, head.Y + offset.Y);

        if (newHead.X < 0 || newHead.X >= BoardWidth || newHead.Y < 0 || newHead.Y >= BoardHeight)
        {
            return CloneWith(state, gameOver: true, won: false, snake: state.Snake, food: state.Food, score: state.Score, frame: state.Frame);
        }

        bool ateFood = newHead.X == state.Food.X && newHead.Y == state.Food.Y;

        var checkSegments = ateFood ? state.Snake : state.Snake.GetRange(0, state.Snake.Count - 1);
        foreach (var seg in checkSegments)
        {
            if (seg.X == newHead.X && seg.Y == newHead.Y)
            {
                return CloneWith(state, gameOver: true, won: false, snake: state.Snake, food: state.Food, score: state.Score, frame: state.Frame);
            }
        }

        var newSnake = new List<SnakePoint> { newHead };
        newSnake.AddRange(state.Snake);
        if (!ateFood) newSnake.RemoveAt(newSnake.Count - 1);

        if (ateFood && newSnake.Count == BoardWidth * BoardHeight)
        {
            return new SnakeGameState
            {
                Snake = newSnake,
                Food = state.Food,
                Score = state.Score + 1,
                GameOver = true,
                Won = true,
                Frame = state.Frame + 1,
            };
        }

        return new SnakeGameState
        {
            Snake = newSnake,
            Food = ateFood ? randomFood(newSnake) : state.Food,
            Score = state.Score + (ateFood ? 1 : 0),
            GameOver = false,
            Won = false,
            Frame = state.Frame + 1,
        };
    }

    private static SnakeGameState CloneWith(
        SnakeGameState state,
        bool gameOver,
        bool won,
        List<SnakePoint> snake,
        SnakePoint food,
        int score,
        int frame) =>
        new()
        {
            Snake = snake,
            Food = food,
            Score = score,
            GameOver = gameOver,
            Won = won,
            Frame = frame,
        };
}
