namespace RetroRpg.Core.Characters;

public class RangerClass : CharacterClass
{
    public RangerClass() : this("", "", "") { }

    protected RangerClass(string subclassId, string subclassName, string subclassFeature)
        : base("ranger", "Korucu", 12, "Dexterity", subclassId, subclassName, subclassFeature) { }

    public override string SignatureAction => "İşaretli Atış";
    public override bool UsesDexterity => true;
}

public sealed class HunterClass : RangerClass
{
    public HunterClass() : base("hunter", "Avcı", "Av seçimi") { }

    public override int AttackBonus() => base.AttackBonus() + 1;
}

public sealed class BeastMasterClass : RangerClass
{
    public BeastMasterClass() : base("beast-master", "Canavar Ustası", "Hayvan yoldaş") { }
}
