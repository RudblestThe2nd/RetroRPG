namespace RetroRpg.Core.Characters;

public class RogueClass : CharacterClass
{
    public RogueClass() : this("", "", "") { }

    protected RogueClass(string subclassId, string subclassName, string subclassFeature)
        : base("rogue", "Düzenbaz", 10, "Dexterity", subclassId, subclassName, subclassFeature) { }

    public override string SignatureAction => "Gizli Saldırı";
    public override bool UsesDexterity => true;
}

public sealed class ThiefClass : RogueClass
{
    public ThiefClass() : base("thief", "Hırsız", "Hızlı eşya") { }
}

public sealed class AssassinClass : RogueClass
{
    public AssassinClass() : base("assassin", "Suikastçı", "İlk vuruş") { }

    public override int AttackBonus() => base.AttackBonus() + 1;
}
