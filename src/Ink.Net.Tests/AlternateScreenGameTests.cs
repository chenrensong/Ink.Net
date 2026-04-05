// -----------------------------------------------------------------------
// Aligned with ink/test/alternate-screen-example.tsx (gameReducer unit tests).
// -----------------------------------------------------------------------

using Ink.Net.SnakeGame;
using Xunit;

namespace Ink.Net.Tests;

public class AlternateScreenGameTests
{
    private static SnakePoint FailFood(IReadOnlyList<SnakePoint> _) =>
        throw new InvalidOperationException("randomFood must not be invoked when the snake does not eat.");

    /// <summary>Snake can move into the tail cell when the tail segment moves away (same as JS test).</summary>
    [Fact]
    public void Tick_MoveIntoTailCellWhenTailMovesAway_IsNotGameOver()
    {
        var state = new SnakeGameState
        {
            Snake =
            [
                new SnakePoint(2, 1),
                new SnakePoint(1, 1),
                new SnakePoint(1, 2),
                new SnakePoint(2, 2),
            ],
            Food = new SnakePoint(0, 0),
            Score = 3,
            GameOver = false,
            Won = false,
            Frame = 10,
        };

        var next = SnakeGameEngine.Tick(state, SnakeDirection.Down, FailFood);

        Assert.False(next.GameOver);
        Assert.False(next.Won);
        Assert.Equal(3, next.Score);
        Assert.Equal(11, next.Frame);
        Assert.Equal(4, next.Snake.Count);
        Assert.Equal(new SnakePoint(2, 2), next.Snake[0]);
        Assert.Equal(new SnakePoint(2, 1), next.Snake[1]);
        Assert.Equal(new SnakePoint(1, 1), next.Snake[2]);
        Assert.Equal(new SnakePoint(1, 2), next.Snake[3]);
    }

    [Fact]
    public void Tick_WhenAlreadyGameOver_ReturnsSameStateShape()
    {
        var state = new SnakeGameState
        {
            Snake = [new SnakePoint(0, 0)],
            Food = new SnakePoint(5, 5),
            Score = 0,
            GameOver = true,
            Won = false,
            Frame = 1,
        };

        var next = SnakeGameEngine.Tick(state, SnakeDirection.Right, FailFood);

        Assert.True(next.GameOver);
        Assert.Same(state, next);
    }
}
