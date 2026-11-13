namespace RetroRpg.Core.Characters;

public class FighterClass : CharacterClass
{
    public FighterClass() : this("", "", "") { }

    protected FighterClass(string subclassId, string subclassName, string subclassFeature)
        : base("fighter", "Savaşçı", 13, "Strength", subclassId, subclassName, subclassFeature) { }

    public override string SignatureAction => "Güçlü Saldırı";
}

public sealed class ChampionClass : FighterClass
{
    public ChampionClass() : base("champion", "Şampiyon", "Kritik ustalığı") { }

    public override int AttackBonus() => base.AttackBonus() + 1;
}

public sealed class BattleMasterClass : FighterClass
{
    public BattleMasterClass() : base("battle-master", "Savaş Ustası", "Manevra") { }
}
