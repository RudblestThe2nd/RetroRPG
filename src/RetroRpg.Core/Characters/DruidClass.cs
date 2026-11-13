namespace RetroRpg.Core.Characters;

public class DruidClass : CharacterClass
{
    public DruidClass() : this("", "", "") { }

    protected DruidClass(string subclassId, string subclassName, string subclassFeature)
        : base("druid", "Druid", 11, "Wisdom", subclassId, subclassName, subclassFeature) { }

    public override string SignatureAction => "Doğa Büyüsü";
}

public sealed class LandDruidClass : DruidClass
{
    public LandDruidClass() : base("land-druid", "Toprak Druid'i", "Biyom büyüsü") { }
}

public sealed class MoonDruidClass : DruidClass
{
    public MoonDruidClass() : base("moon-druid", "Ay Druid'i", "Vahşi biçim") { }
}
