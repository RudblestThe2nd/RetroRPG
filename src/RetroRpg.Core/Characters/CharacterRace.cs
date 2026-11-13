namespace RetroRpg.Core.Characters;

public abstract class CharacterRace
{
    protected CharacterRace(string id, string displayName, string bodyType, int movement)
    {
        Id = id;
        DisplayName = displayName;
        BodyType = bodyType;
        Movement = movement;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string BodyType { get; }
    public int Movement { get; }
    public virtual void ApplyBonuses(CharacterStats stats) { }
}

public sealed class HumanRace() : CharacterRace("human", "İnsan", "medium", 6)
{
    public override void ApplyBonuses(CharacterStats stats) => stats.Charisma += 1;
}

public sealed class ElfRace() : CharacterRace("elf", "Elf", "slim", 7)
{
    public override void ApplyBonuses(CharacterStats stats) => stats.Dexterity += 2;
}

public sealed class DwarfRace() : CharacterRace("dwarf", "Cüce", "broad", 5)
{
    public override void ApplyBonuses(CharacterStats stats) => stats.Constitution += 2;
}

public sealed class HalflingRace() : CharacterRace("halfling", "Buçukluk", "short", 5)
{
    public override void ApplyBonuses(CharacterStats stats) => stats.Dexterity += 2;
}

public sealed class GnomeRace() : CharacterRace("gnome", "Gnom", "short", 5)
{
    public override void ApplyBonuses(CharacterStats stats) => stats.Intelligence += 2;
}

public sealed class HalfElfRace() : CharacterRace("half-elf", "Yarı Elf", "medium", 6)
{
    public override void ApplyBonuses(CharacterStats stats) { stats.Charisma += 1; stats.Dexterity += 1; }
}

public sealed class DrowRace() : CharacterRace("drow", "Drow", "slim", 6)
{
    public override void ApplyBonuses(CharacterStats stats) { stats.Dexterity += 1; stats.Charisma += 1; }
}

public sealed class HalfOrcRace() : CharacterRace("half-orc", "Yarı Ork", "large", 6)
{
    public override void ApplyBonuses(CharacterStats stats) { stats.Strength += 2; stats.Constitution += 1; }
}

public sealed class TieflingRace() : CharacterRace("tiefling", "Tiefling", "horned", 6)
{
    public override void ApplyBonuses(CharacterStats stats) { stats.Charisma += 2; stats.Intelligence += 1; }
}

public sealed class DragonbornRace() : CharacterRace("dragonborn", "Dragonborn", "dragonborn", 6)
{
    public override void ApplyBonuses(CharacterStats stats) { stats.Strength += 2; stats.Charisma += 1; }
}

public sealed class GithyankiRace() : CharacterRace("githyanki", "Githyanki", "slim", 6)
{
    public override void ApplyBonuses(CharacterStats stats) { stats.Strength += 1; stats.Intelligence += 1; }
}

public static class RaceCatalog
{
    public static IReadOnlyList<CharacterRace> All { get; } =
    [
        new HumanRace(), new ElfRace(), new DwarfRace(), new HalflingRace(), new GnomeRace(),
        new HalfElfRace(), new DrowRace(), new HalfOrcRace(), new TieflingRace(),
        new DragonbornRace(), new GithyankiRace()
    ];

    public static CharacterRace Find(string id) => All.First(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
