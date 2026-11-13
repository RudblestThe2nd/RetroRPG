namespace RetroRpg.Core.Characters;

public class PaladinClass : CharacterClass
{
    public PaladinClass() : this("", "", "") { }

    protected PaladinClass(string subclassId, string subclassName, string subclassFeature)
        : base("paladin", "Paladin", 13, "Strength", subclassId, subclassName, subclassFeature) { }

    public override string SignatureAction => "Kutsal Darbe";
}

public sealed class DevotionPaladinClass : PaladinClass
{
    public DevotionPaladinClass() : base("devotion-paladin", "Adanmışlık Paladini", "Kutsal koruma") { }
}

public sealed class VengeancePaladinClass : PaladinClass
{
    public VengeancePaladinClass() : base("vengeance-paladin", "İntikam Paladini", "Hedef yemini") { }
}
