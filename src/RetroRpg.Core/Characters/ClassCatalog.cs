namespace RetroRpg.Core.Characters;

public static class ClassCatalog
{
    public static IReadOnlyList<CharacterClass> All { get; } =
    [
        new BarbarianClass(), new BardClass(), new ClericClass(), new DruidClass(),
        new FighterClass(), new MonkClass(), new PaladinClass(), new RangerClass(),
        new RogueClass(), new SorcererClass(), new WarlockClass(), new WizardClass()
    ];

    public static IReadOnlyList<CharacterClass> Subclasses { get; } =
    [
        new BerserkerClass(), new TotemWarriorClass(), new LoreBardClass(), new ValorBardClass(),
        new LifeClericClass(), new WarClericClass(), new LandDruidClass(), new MoonDruidClass(),
        new ChampionClass(), new BattleMasterClass(), new OpenHandMonkClass(), new ShadowMonkClass(),
        new DevotionPaladinClass(), new VengeancePaladinClass(), new HunterClass(), new BeastMasterClass(),
        new ThiefClass(), new AssassinClass(), new WildMagicSorcererClass(), new DraconicSorcererClass(),
        new FiendWarlockClass(), new GreatOldOneWarlockClass(), new EvocationWizardClass(), new AbjurationWizardClass()
    ];

    public static CharacterClass Find(string id) => All.First(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<CharacterClass> SubclassesOf(string classId) =>
        Subclasses.Where(x => x.Id.Equals(classId, StringComparison.OrdinalIgnoreCase)).ToArray();

    public static CharacterClass FindSubclass(string subclassId) =>
        Subclasses.First(x => x.SubclassId.Equals(subclassId, StringComparison.OrdinalIgnoreCase));
}
