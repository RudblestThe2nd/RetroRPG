namespace RetroRpg.Core.Combat;

public interface IDiceRoller
{
    int Roll(int sides);
    D20Roll RollD20(int bonus = 0);
}

public sealed class DiceRoller(Random? random = null) : IDiceRoller
{
    private readonly Random _random = random ?? Random.Shared;
    public int Roll(int sides) => sides < 2 ? throw new ArgumentOutOfRangeException(nameof(sides)) : _random.Next(1, sides + 1);
    public D20Roll RollD20(int bonus = 0) => new(Roll(20), bonus);
}

public sealed class FixedDiceRoller(params int[] rolls) : IDiceRoller
{
    private readonly Queue<int> _rolls = new(rolls);
    public int Roll(int sides)
    {
        if (_rolls.Count == 0) throw new InvalidOperationException("Test zarı tükendi.");
        int value = _rolls.Dequeue();
        return value < 1 || value > sides ? throw new InvalidOperationException($"{value}, d{sides} için geçersiz.") : value;
    }
    public D20Roll RollD20(int bonus = 0) => new(Roll(20), bonus);
}
