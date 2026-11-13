namespace RetroRpg.Core.Characters;

public class SorcererClass : CharacterClass
{
    public SorcererClass() : this("", "", "") { }

    protected SorcererClass(string subclassId, string subclassName, string subclassFeature)
        : base("sorcerer", "Büyücü", 9, "Charisma", subclassId, subclassName, subclassFeature) { }

    public override string SignatureAction => "Doğuştan Büyü";
}

public sealed class WildMagicSorcererClass : SorcererClass
{
    public WildMagicSorcererClass() : base("wild-magic", "Vahşi Büyü", "Rastlantısal büyü") { }
}

public sealed class DraconicSorcererClass : SorcererClass
{
    public DraconicSorcererClass() : base("draconic-bloodline", "Ejder Soyu", "Element direnci") { }
}
