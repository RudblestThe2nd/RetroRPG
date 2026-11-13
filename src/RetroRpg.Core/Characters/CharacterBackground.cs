using RetroRpg.Core.Characters;

namespace RetroRpg.Core.Characters;

public enum BackgroundStage
{
    Origin,
    Family,
    Childhood,
    Education,
    Profession,
    TurningPoint,
    Goal
}

public enum BackgroundStat
{
    Strength,
    Dexterity,
    Constitution,
    Intelligence,
    Wisdom,
    Charisma
}

// Karakter geçmişinin tek bir aşamasında seçilebilen sade veri nesnesi.
public sealed record BackgroundStageChoice(
    BackgroundStage Stage,
    string Id,
    string DisplayName,
    string Description,
    BackgroundStat Stat,
    int StatBonus,
    int GoldModifier)
{
    public string EffectText => $"{BackgroundCatalog.StatName(Stat)} +{StatBonus}, altın {(GoldModifier >= 0 ? "+" : "")}{GoldModifier}";

    public void Apply(CharacterStats stats)
    {
        switch (Stat)
        {
            case BackgroundStat.Strength: stats.Strength += StatBonus; break;
            case BackgroundStat.Dexterity: stats.Dexterity += StatBonus; break;
            case BackgroundStat.Constitution: stats.Constitution += StatBonus; break;
            case BackgroundStat.Intelligence: stats.Intelligence += StatBonus; break;
            case BackgroundStat.Wisdom: stats.Wisdom += StatBonus; break;
            case BackgroundStat.Charisma: stats.Charisma += StatBonus; break;
        }
    }
}

public abstract class CharacterBackground
{
    protected CharacterBackground(string id, string displayName, string description, int gold)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
        StartingGold = Math.Max(0, gold);
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public int StartingGold { get; }
    public virtual IReadOnlyList<BackgroundStageChoice> Choices => [];

    public virtual void ApplyBonuses(CharacterStats stats)
    {
        foreach (BackgroundStageChoice choice in Choices) choice.Apply(stats);
    }
}

// Eski kayıtların altı tek-parça geçmişini açabilmek için korunur.
public sealed class BackgroundChoice(string id, string name, string description, int gold)
    : CharacterBackground(id, name, description, gold);

// Yeni karakter oluşturma akışının yedi seçimini tek bir karakter geçmişinde birleştirir.
public sealed class CompositeBackground : CharacterBackground
{
    public CompositeBackground(IReadOnlyList<BackgroundStageChoice> choices)
        : base(BuildId(choices), BuildName(choices), BuildDescription(choices), 10 + choices.Sum(x => x.GoldModifier))
    {
        Choices = choices.ToArray();
    }

    public override IReadOnlyList<BackgroundStageChoice> Choices { get; }

    private static string BuildId(IReadOnlyList<BackgroundStageChoice> choices) =>
        "path/" + string.Join('/', choices.Select(x => x.Id));

    private static string BuildName(IReadOnlyList<BackgroundStageChoice> choices) =>
        choices.Count == 0 ? "Adsız Geçmiş" : $"{choices[0].DisplayName} · {choices[^1].DisplayName}";

    private static string BuildDescription(IReadOnlyList<BackgroundStageChoice> choices) =>
        string.Join(" → ", choices.Select(x => x.DisplayName));
}

public static class BackgroundCatalog
{
    private static readonly IReadOnlyList<CharacterBackground> Legacy =
    [
        new BackgroundChoice("village-child", "Köy Çocuğu", "Doğayı ve sade yaşamı tanır.", 15),
        new BackgroundChoice("city-orphan", "Şehir Yetimi", "Sokaklarda hayatta kalmayı öğrendi.", 8),
        new BackgroundChoice("noble-family", "Soylu Aile", "Eğitimli ve bağlantıları kuvvetlidir.", 40),
        new BackgroundChoice("soldier-family", "Asker Ailesi", "Disiplin ve savaş görgüsüyle yetişti.", 20),
        new BackgroundChoice("temple-student", "Tapınak Öğrencisi", "İnanç ve eski metinler konusunda eğitim aldı.", 12),
        new BackgroundChoice("wandering-merchant", "Gezgin Tüccar", "Yolları, fiyatları ve insanları tanır.", 30)
    ];

    public static IReadOnlyList<BackgroundStage> Stages { get; } =
    [
        BackgroundStage.Origin, BackgroundStage.Family, BackgroundStage.Childhood,
        BackgroundStage.Education, BackgroundStage.Profession, BackgroundStage.TurningPoint,
        BackgroundStage.Goal
    ];

    private static readonly IReadOnlyDictionary<BackgroundStage, IReadOnlyList<BackgroundStageChoice>> StageOptions =
        new Dictionary<BackgroundStage, IReadOnlyList<BackgroundStageChoice>>
        {
            [BackgroundStage.Origin] =
            [
                C(BackgroundStage.Origin, "origin-village", "Kırsal Köken", "Tarlalar ve orman yolları arasında doğdun.", BackgroundStat.Constitution, 1, 2),
                C(BackgroundStage.Origin, "origin-city", "Şehir Kökeni", "Kalabalığın ve dar sokakların dilini öğrendin.", BackgroundStat.Charisma, 1, 4),
                C(BackgroundStage.Origin, "origin-frontier", "Sınır Boyu", "Tehlikenin eksik olmadığı uzak bir karakolda büyüdün.", BackgroundStat.Strength, 1, 0),
                C(BackgroundStage.Origin, "origin-coast", "Kıyı Halkı", "Rüzgârı, akıntıyı ve liman insanlarını tanırsın.", BackgroundStat.Dexterity, 1, 3),
                C(BackgroundStage.Origin, "origin-mountain", "Dağ Yurdu", "Sert iklim sana direnç kazandırdı.", BackgroundStat.Constitution, 1, -1),
                C(BackgroundStage.Origin, "origin-temple", "Tapınak Bölgesi", "Çanlar ve eski öğretiler arasında doğdun.", BackgroundStat.Wisdom, 1, 1)
            ],
            [BackgroundStage.Family] =
            [
                C(BackgroundStage.Family, "family-farmers", "Çiftçi Ailesi", "Emek, sabır ve dayanışma öğrendin.", BackgroundStat.Constitution, 1, 1),
                C(BackgroundStage.Family, "family-artisans", "Zanaatkâr Ailesi", "Aletleri ve ince işçiliği tanırsın.", BackgroundStat.Intelligence, 1, 3),
                C(BackgroundStage.Family, "family-merchants", "Tüccar Ailesi", "Pazarlık ve hesap evin gündelik diliydi.", BackgroundStat.Charisma, 1, 8),
                C(BackgroundStage.Family, "family-soldiers", "Asker Ailesi", "Düzen ve silah bilgisiyle yetiştin.", BackgroundStat.Strength, 1, 2),
                C(BackgroundStage.Family, "family-nobles", "Soylu Aile", "Eğitim ve bağlantılar sana kapılar açtı.", BackgroundStat.Charisma, 1, 15),
                C(BackgroundStage.Family, "family-wanderers", "Göçebe Aile", "Yolculuk senin için ev kadar doğaldır.", BackgroundStat.Dexterity, 1, 0)
            ],
            [BackgroundStage.Childhood] =
            [
                C(BackgroundStage.Childhood, "childhood-woods", "Ormanlarda", "Patikaları ve hayvan izlerini okuyarak büyüdün.", BackgroundStat.Wisdom, 1, 0),
                C(BackgroundStage.Childhood, "childhood-streets", "Sokaklarda", "Tehlikeyi erken fark etmeyi öğrendin.", BackgroundStat.Dexterity, 1, 2),
                C(BackgroundStage.Childhood, "childhood-books", "Kitaplarla", "Eski hikâyeler merakını besledi.", BackgroundStat.Intelligence, 1, -2),
                C(BackgroundStage.Childhood, "childhood-workshop", "Atölyede", "Sabırla üretmeyi ve onarmayı öğrendin.", BackgroundStat.Intelligence, 1, 3),
                C(BackgroundStage.Childhood, "childhood-barracks", "Kışlada", "Disiplin ve dayanıklılık hayatına erken girdi.", BackgroundStat.Strength, 1, 1),
                C(BackgroundStage.Childhood, "childhood-caravan", "Kervanda", "Farklı dilleri ve insanları gözlemledin.", BackgroundStat.Charisma, 1, 4)
            ],
            [BackgroundStage.Education] =
            [
                C(BackgroundStage.Education, "education-self", "Kendi Kendine", "Deneyerek ve yanılarak öğrendin.", BackgroundStat.Wisdom, 1, 0),
                C(BackgroundStage.Education, "education-guild", "Lonca Eğitimi", "Bir ustanın yanında meslek disiplini kazandın.", BackgroundStat.Dexterity, 1, 2),
                C(BackgroundStage.Education, "education-temple", "Tapınak Eğitimi", "İnanç, tarih ve şifa bilgisi aldın.", BackgroundStat.Wisdom, 1, -1),
                C(BackgroundStage.Education, "education-military", "Askerî Eğitim", "Talimler seni daha güçlü yaptı.", BackgroundStat.Strength, 1, 0),
                C(BackgroundStage.Education, "education-academy", "Akademi", "Kuram ve kadim diller üzerine çalıştın.", BackgroundStat.Intelligence, 1, -5),
                C(BackgroundStage.Education, "education-court", "Saray Eğitimi", "Hitabet ve görgü kurallarını öğrendin.", BackgroundStat.Charisma, 1, 5)
            ],
            [BackgroundStage.Profession] =
            [
                C(BackgroundStage.Profession, "profession-hunter", "Avcı", "İz sürüp avlanarak geçimini sağladın.", BackgroundStat.Wisdom, 1, 4),
                C(BackgroundStage.Profession, "profession-guard", "Muhafız", "İnsanları ve kapıları korudun.", BackgroundStat.Strength, 1, 6),
                C(BackgroundStage.Profession, "profession-healer", "Şifacı", "Otlar ve yaralar konusunda deneyim kazandın.", BackgroundStat.Wisdom, 1, 3),
                C(BackgroundStage.Profession, "profession-trader", "Tüccar", "Mal alıp satarak sermaye biriktirdin.", BackgroundStat.Charisma, 1, 12),
                C(BackgroundStage.Profession, "profession-scout", "İzci", "Tehlikeli yolları herkesten önce gördün.", BackgroundStat.Dexterity, 1, 5),
                C(BackgroundStage.Profession, "profession-scribe", "Kâtip", "Kayıtlar ve anlaşmalarla çalıştın.", BackgroundStat.Intelligence, 1, 5)
            ],
            [BackgroundStage.TurningPoint] =
            [
                C(BackgroundStage.TurningPoint, "turning-raid", "Baskından Kurtuldun", "Yurdunun yıkılışı seni yola çıkardı.", BackgroundStat.Constitution, 1, -5),
                C(BackgroundStage.TurningPoint, "turning-exile", "Sürgün Edildin", "Geride bıraktıkların seni hâlâ takip ediyor.", BackgroundStat.Wisdom, 1, -3),
                C(BackgroundStage.TurningPoint, "turning-relic", "Bir Kalıntı Buldun", "Açıklayamadığın bir nesne kaderini değiştirdi.", BackgroundStat.Intelligence, 1, 5),
                C(BackgroundStage.TurningPoint, "turning-debt", "Borçlandın", "Ödenmesi gereken büyük bir borcun var.", BackgroundStat.Charisma, 1, 10),
                C(BackgroundStage.TurningPoint, "turning-oath", "Yemin Ettin", "Bir söz artık bütün kararlarına yön veriyor.", BackgroundStat.Strength, 1, 0),
                C(BackgroundStage.TurningPoint, "turning-loss", "Birini Kaybettin", "Kayıp, dikkatini ve iradeni keskinleştirdi.", BackgroundStat.Dexterity, 1, -2)
            ],
            [BackgroundStage.Goal] =
            [
                C(BackgroundStage.Goal, "goal-glory", "Şan Kazanmak", "Adının büyük zaferlerle anılmasını istiyorsun.", BackgroundStat.Strength, 1, 0),
                C(BackgroundStage.Goal, "goal-wealth", "Servet Edinmek", "Hiç kimseye muhtaç olmamak istiyorsun.", BackgroundStat.Charisma, 1, 8),
                C(BackgroundStage.Goal, "goal-knowledge", "Bilgiyi Bulmak", "Dünyanın sakladığı gerçeğin peşindesin.", BackgroundStat.Intelligence, 1, -2),
                C(BackgroundStage.Goal, "goal-revenge", "İntikam Almak", "Geçmişteki bir hesabı kapatacaksın.", BackgroundStat.Dexterity, 1, 0),
                C(BackgroundStage.Goal, "goal-protect", "Birilerini Korumak", "Gücünü başkalarının güvenliği için kullanacaksın.", BackgroundStat.Constitution, 1, -1),
                C(BackgroundStage.Goal, "goal-freedom", "Özgür Olmak", "Kaderini yalnız kendin belirlemek istiyorsun.", BackgroundStat.Wisdom, 1, 2)
            ]
        };

    // Eski API ve kayıt uyumluluğu.
    public static IReadOnlyList<CharacterBackground> All => Legacy;

    public static IReadOnlyList<BackgroundStageChoice> OptionsFor(BackgroundStage stage) => StageOptions[stage];

    public static IReadOnlyList<BackgroundStageChoice> DefaultPath() =>
        Stages.Select(stage => OptionsFor(stage)[0]).ToArray();

    public static CompositeBackground Create(IEnumerable<BackgroundStageChoice> selections)
    {
        Dictionary<BackgroundStage, BackgroundStageChoice> byStage = selections.ToDictionary(x => x.Stage);
        if (Stages.Any(stage => !byStage.ContainsKey(stage)))
            throw new ArgumentException("Karakter geçmişinin yedi aşaması da seçilmelidir.", nameof(selections));
        return new CompositeBackground(Stages.Select(stage => byStage[stage]).ToArray());
    }

    public static CharacterBackground Find(string id)
    {
        if (!id.StartsWith("path/", StringComparison.OrdinalIgnoreCase))
            return Legacy.First(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        string[] ids = id["path/".Length..].Split('/', StringSplitOptions.RemoveEmptyEntries);
        BackgroundStageChoice[] choices = ids
            .Select(choiceId => StageOptions.Values.SelectMany(x => x)
                .First(x => x.Id.Equals(choiceId, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        return Create(choices);
    }

    public static string StageName(BackgroundStage stage) => stage switch
    {
        BackgroundStage.Origin => "Köken",
        BackgroundStage.Family => "Aile",
        BackgroundStage.Childhood => "Çocukluk",
        BackgroundStage.Education => "Eğitim",
        BackgroundStage.Profession => "Meslek",
        BackgroundStage.TurningPoint => "Dönüm Noktası",
        _ => "Amaç"
    };

    public static string StatName(BackgroundStat stat) => stat switch
    {
        BackgroundStat.Strength => "Güç",
        BackgroundStat.Dexterity => "Çeviklik",
        BackgroundStat.Constitution => "Dayanıklılık",
        BackgroundStat.Intelligence => "Zekâ",
        BackgroundStat.Wisdom => "Bilgelik",
        _ => "Karizma"
    };

    private static BackgroundStageChoice C(BackgroundStage stage, string id, string name, string description,
        BackgroundStat stat, int statBonus, int goldModifier) =>
        new(stage, id, name, description, stat, statBonus, goldModifier);
}
