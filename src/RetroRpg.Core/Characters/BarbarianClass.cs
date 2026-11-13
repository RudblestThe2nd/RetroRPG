namespace RetroRpg.Core.Characters;

public class BarbarianClass : CharacterClass
{
    public BarbarianClass() : this("", "", "") { }

    protected BarbarianClass(string subclassId, string subclassName, string subclassFeature)
        : base("barbarian", "Barbar", 14, "Strength", subclassId, subclassName, subclassFeature) { }

    public override string SignatureAction => "Öfke";
}

public sealed class BerserkerClass : BarbarianClass
{
    public BerserkerClass() : base("berserker", "Vahşi Savaşçı", "Öfke hasarı") { }

    public override int AttackBonus() => base.AttackBonus() + 1;
}

public sealed class TotemWarriorClass : BarbarianClass
{
    public TotemWarriorClass() : base("totem-warrior", "Totem Savaşçısı", "Doğa direnci") { }
}
