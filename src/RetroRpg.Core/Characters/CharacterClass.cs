namespace RetroRpg.Core.Characters;

// Bütün karakter sınıflarının tabanı. FighterClass gibi ana sınıflar bundan,
// ChampionClass gibi alt sınıflar da kendi ana sınıflarından türer.
public abstract class CharacterClass
{
    protected CharacterClass(string id, string displayName, int baseHitPoints, string primaryAbility,
        string subclassId, string subclassName, string subclassFeature)
    {
        Id = id;
        DisplayName = displayName;
        BaseHitPoints = baseHitPoints;
        PrimaryAbility = primaryAbility;
        SubclassId = subclassId;
        SubclassName = subclassName;
        SubclassFeature = subclassFeature;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public int BaseHitPoints { get; }
    public string PrimaryAbility { get; }
    public string SubclassId { get; }
    public string SubclassName { get; }
    public string SubclassFeature { get; }
    public bool IsSubclass => SubclassId.Length > 0;
    public abstract string SignatureAction { get; }

    // Saldırıda Strength yerine Dexterity kullanan sınıflar true döner.
    public virtual bool UsesDexterity => false;

    // Sınıfın saldırı zarına eklediği sabit bonus. Ana sınıf ve alt sınıf override edebilir.
    public virtual int AttackBonus() => 2;
}
