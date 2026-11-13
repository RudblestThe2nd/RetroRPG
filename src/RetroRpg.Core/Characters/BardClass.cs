namespace RetroRpg.Core.Characters;

public class BardClass : CharacterClass
{
    public BardClass() : this("", "", "") { }

    protected BardClass(string subclassId, string subclassName, string subclassFeature)
        : base("bard", "Ozan", 10, "Charisma", subclassId, subclassName, subclassFeature) { }

    public override string SignatureAction => "İlham";
}

public sealed class LoreBardClass : BardClass
{
    public LoreBardClass() : base("lore-bard", "Bilgi Ozanı", "Ek beceri") { }
}

public sealed class ValorBardClass : BardClass
{
    public ValorBardClass() : base("valor-bard", "Cesaret Ozanı", "Savaş ilhamı") { }
}
