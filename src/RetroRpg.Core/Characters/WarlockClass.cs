namespace RetroRpg.Core.Characters;

public class WarlockClass : CharacterClass
{
    public WarlockClass() : this("", "", "") { }

    protected WarlockClass(string subclassId, string subclassName, string subclassFeature)
        : base("warlock", "Cadı Avcısı", 10, "Charisma", subclassId, subclassName, subclassFeature) { }

    public override string SignatureAction => "Karanlık Işın";
}

public sealed class FiendWarlockClass : WarlockClass
{
    public FiendWarlockClass() : base("fiend-pact", "İblis Paktı", "Geçici can") { }
}

public sealed class GreatOldOneWarlockClass : WarlockClass
{
    public GreatOldOneWarlockClass() : base("great-old-one", "Kadim Varlık", "Zihin etkisi") { }
}
