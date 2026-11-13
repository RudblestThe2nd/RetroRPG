using RetroRpg.Core.Characters;

namespace RetroRpg.Core.World;

// Muhafız konuşmalarının ortak tabanı. Alt sınıflar yalnız kullanılan niteliği
// ve kimlik belgesi davranışını değiştirir; ekran hepsini aynı türden kullanır.
public abstract class AccessApproach(string id, string label, string description, BackgroundStat stat)
{
    public string Id { get; } = id;
    public string Label { get; } = label;
    public string Description { get; } = description;
    public BackgroundStat Stat { get; } = stat;
    public virtual bool CanAttempt(PlayerCharacter player, LocationService service) => true;
    public virtual bool AutomaticSuccess(PlayerCharacter player, LocationService service) => false;
    public virtual int Bonus(PlayerCharacter player) => WorldEncounter.CheckBonus(player, Stat);
}

public sealed class PersuasionApproach()
    : AccessApproach("persuasion", "İKNA ET", "Neden içeri alınman gerektiğini sakin biçimde anlat.", BackgroundStat.Charisma);

public sealed class IntimidationApproach()
    : AccessApproach("intimidation", "GÖZDAĞI VER", "Fiziksel gücünü ve kararlılığını göster.", BackgroundStat.Strength);

public sealed class CredentialApproach()
    : AccessApproach("credential", "KİMLİĞİNİ GÖSTER", "Soyluluk mührünü veya nöbetçi rozetini göster.", BackgroundStat.Charisma)
{
    public override bool CanAttempt(PlayerCharacter player, LocationService service)
    {
        string[] choices = player.Background.Choices.Select(x => x.Id).ToArray();
        bool noble = choices.Contains("family-nobles") || player.Background.Id == "noble-family";
        bool guard = choices.Contains("profession-guard") || player.Background.Id == "soldier-family";
        return noble || (service == LocationService.Barracks && guard);
    }

    public override bool AutomaticSuccess(PlayerCharacter player, LocationService service) => CanAttempt(player, service);
}

public static class AccessApproachCatalog
{
    public static IReadOnlyList<AccessApproach> All { get; } =
        [new PersuasionApproach(), new IntimidationApproach(), new CredentialApproach()];

    public static int Difficulty(LocationService service) => service == LocationService.WarRoom ? 15 : 12;
}
