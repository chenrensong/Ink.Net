// Aligned with ink examples/alternate-screen types + gameReducer state shape.

namespace Ink.Net.SnakeGame;

public readonly record struct SnakePoint(int X, int Y);

public enum SnakeDirection
{
    Up,
    Down,
    Left,
    Right,
}

/// <summary>Mutable game state — matches the JS <c>GameState</c> object used by <c>gameReducer</c>.</summary>
public sealed class SnakeGameState
{
    public List<SnakePoint> Snake { get; set; } = new();
    public SnakePoint Food { get; set; }
    public int Score { get; set; }
    public bool GameOver { get; set; }
    public bool Won { get; set; }
    public int Frame { get; set; }
}
