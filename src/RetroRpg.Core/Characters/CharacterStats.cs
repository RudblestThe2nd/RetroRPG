namespace RetroRpg.Core.Characters;

public sealed class CharacterStats
{
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;

    public static int Modifier(int score) => (int)Math.Floor((score - 10) / 2d);
    public int InitiativeBonus => Modifier(Dexterity);
    public int MaxHealth => Math.Max(1, 10 + Modifier(Constitution));
}
