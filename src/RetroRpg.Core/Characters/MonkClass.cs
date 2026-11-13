namespace RetroRpg.Core.Characters;

public class MonkClass : CharacterClass
{
    public MonkClass() : this("", "", "") { }

    protected MonkClass(string subclassId, string subclassName, string subclassFeature)
        : base("monk", "Keşiş", 11, "Dexterity", subclassId, subclassName, subclassFeature) { }

    public override string SignatureAction => "Seri Darbe";
    public override bool UsesDexterity => true;
}

public sealed class OpenHandMonkClass : MonkClass
{
    public OpenHandMonkClass() : base("open-hand", "Açık El", "Sersemletme") { }
}

public sealed class ShadowMonkClass : MonkClass
{
    public ShadowMonkClass() : base("shadow-monk", "Gölge Keşişi", "Gölge adımı") { }
}
