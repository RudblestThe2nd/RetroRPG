using RetroRpg.Core.Game;

namespace RetroRpg.Core.Story;

// Dört finalin uygunluğu ve dinamik epilog. Hiçbiri tek bir rastgele zara bağlı değildir.
// Kalpaklı Süvari bu hesaba hiç girmez.
public static class EndingRules
{
    public const string Restore = "restore-old-roads";
    public const string Share = "share-roads";
    public const string Destroy = "destroy-fate-lens";
    public const string Watcher = "become-new-watcher";
    public const int HighTrust = 2;

    // Uzlaşmacı taş kararları: halkla ya da yerel güçlerle anlaşarak verilen kararlar.
    public static int CompromiseCount(StoryProgress story) =>
        (story.WaystoneMode("root") is "cleanse" or "bind" ? 1 : 0) +
        (story.WaystoneMode("sky") == "protect" ? 1 : 0) +
        (story.WaystoneMode("sun") == "share" ? 1 : 0);

    public static int SeizeCount(StoryProgress story) =>
        new[] { "root", "sky", "sun" }.Count(x => story.WaystoneMode(x) is "seize" or "steal");

    // Paylaştırma finalinde yalnız Yol Taşı bölgeleri sayılır. Yeşilgeçit ve mülteci güveni
    // epilog için tutulur ama bölge sayısına girmez.
    public static IReadOnlyList<string> TrustRegions { get; } = ["swamp", "mountain", "desert"];

    public static int HighTrustRegions(StoryProgress story) => TrustRegions.Count(x => story.TrustIn(x) >= HighTrust);

    public static bool CanShare(StoryProgress story) => CompromiseCount(story) >= 2 && HighTrustRegions(story) >= 2;

    // Vaelor'u ikna etmek, kurtarılan tutsağın tanıklığı veya uzlaşmacı bir yol ister.
    public static bool CanPersuadeVaelor(StoryProgress story) => story.PrisonerRescued || CompromiseCount(story) >= 2;

    public static IReadOnlyList<string> Available(StoryProgress story) =>
        StoryCatalog.Endings.Where(x => x.IsUnlocked(story)).Select(x => x.EndingId).ToArray();

    public static string Epilogue(GameSession session, string endingId)
    {
        StoryProgress story = session.Story;
        string person = story.Remembered?.Name ?? "Hatırladığın kişi";
        string rescued = story.PrisonerRescued
            ? $"{person} seninle birlikte kuleden çıkıyor."
            : $"{person} Mercek'in yankısında kalıyor; adı yalnız senin hafızanda yaşıyor.";
        string vaelor = story.VaelorState switch
        {
            VaelorState.Killed => "Vaelor'un adı haritalardan silindi.",
            VaelorState.Persuaded => "Vaelor yolları yeniden çizmek için sana yardım ediyor.",
            VaelorState.Imprisoned => "Vaelor kendi kulesinde, kendi zincirleriyle bekliyor.",
            _ => "Vaelor'un kaderi belirsiz kaldı."
        };

        string body = endingId switch
        {
            Restore => "Mercek onarıldı; bütün eski yollar geri döndü. Unutulanlar hatırlanıyor, ama eski sınır kavgaları da yollarla birlikte uyanıyor.",
            Share => "Her bölge kendi Yol Taşı'nı yönetiyor. Bataklık, dağ ve çöl yolları ortak bir mecliste açılıp kapanıyor; ticaret ve barış aynı yoldan yürüyor.",
            Destroy => "Kader Merceği paramparça oldu. Büyülü yol ağı çöktü; dünya özgür ama yolculuk zor. Bazı silinen anılar hiç geri gelmeyecek.",
            _ => $"Kuleyi ve Mercek'i ele geçirdin. Dünya seni {WatcherTone(session)} olarak anıyor."
        };
        return $"{body} {rescued} {vaelor}";
    }

    // Yeni Gözcü finalinin tonu sınıf, amaç ve geçmiş kararlarından çıkar.
    public static string WatcherTone(GameSession session)
    {
        string[] choices = session.Player.Background.Choices.Select(x => x.Id).ToArray();
        string classId = session.Player.Class.Id;
        if (choices.Contains("goal-revenge") || classId == "warlock" || SeizeCount(session.Story) >= 2)
            return "yozlaşmış bir Gözcü";
        if (choices.Contains("goal-protect") || classId is "paladin" or "cleric" or "monk")
            return "koruyucu bir Gözcü";
        return "yolların yeni hükümdarı";
    }
}
