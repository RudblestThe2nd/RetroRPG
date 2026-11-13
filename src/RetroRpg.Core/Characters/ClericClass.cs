namespace RetroRpg.Core.Characters;

public class ClericClass : CharacterClass
{
    public ClericClass() : this("", "", "") { }

    protected ClericClass(string subclassId, string subclassName, string subclassFeature)
        : base("cleric", "Ruhban", 12, "Wisdom", subclassId, subclassName, subclassFeature) { }

    public override string SignatureAction => "İyileştirme";
}

public sealed class LifeClericClass : ClericClass
{
    public LifeClericClass() : base("life-cleric", "Yaşam Ruhbanı", "Güçlü iyileştirme") { }
}

public sealed class WarClericClass : ClericClass
{
    public WarClericClass() : base("war-cleric", "Savaş Ruhbanı", "Savaş kutsaması") { }
}
