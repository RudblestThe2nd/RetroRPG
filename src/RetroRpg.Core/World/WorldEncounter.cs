using RetroRpg.Core.Characters;
using RetroRpg.Core.Common;
using RetroRpg.Core.Entities;

namespace RetroRpg.Core.World;

// Bütün savaş dışı encounter'ların ortak tabanı. Alt sınıflar yalnız ortak davranışın
// değişen kısmını seçer; ekran kodu hepsini WorldEncounter üzerinden kullanır.
public abstract class WorldEncounter : GameEntity
{
    protected WorldEncounter(string id, string name, Biome biome, string assetFile,
        string description, BackgroundStat checkStat, int difficulty, int rewardGold) : base(name)
    {
        EncounterId = id;
        Biome = biome;
        AssetPath = $"assets/final-production-v5/scenes/encounters/non-combat/{assetFile}";
        Description = description;
        CheckStat = checkStat;
        Difficulty = difficulty;
        RewardGold = rewardGold;
    }

    public string EncounterId { get; }
    public Biome Biome { get; }
    public string AssetPath { get; }
    public string Description { get; }
    public BackgroundStat CheckStat { get; }
    public int Difficulty { get; }
    public int RewardGold { get; }
    public abstract string ActionLabel { get; }
    public abstract string SuccessText { get; }
    public virtual string LeaveLabel => "YOLUNA DEVAM ET";
    public IReadOnlyList<EncounterChoice> Choices => EncounterChoiceCatalog.For(this);

    public int CheckBonus(PlayerCharacter player) => CheckBonus(player, CheckStat);

    public static int CheckBonus(PlayerCharacter player, BackgroundStat stat) => CharacterStats.Modifier(stat switch
    {
        BackgroundStat.Strength => player.Stats.Strength,
        BackgroundStat.Dexterity => player.Stats.Dexterity,
        BackgroundStat.Constitution => player.Stats.Constitution,
        BackgroundStat.Intelligence => player.Stats.Intelligence,
        BackgroundStat.Wisdom => player.Stats.Wisdom,
        _ => player.Stats.Charisma
    });
}

public sealed class DiscoveryEncounter : WorldEncounter
{
    public DiscoveryEncounter(string id, string name, Biome biome, string assetFile, string description,
        BackgroundStat stat = BackgroundStat.Intelligence, int difficulty = 11, int rewardGold = 5)
        : base(id, name, biome, assetFile, description, stat, difficulty, rewardGold) { }
    public override string ActionLabel => "İNCELE";
    public override string SuccessText => "İncelemen işe yaradı; yararlı bir şey buldun.";
}

public sealed class TravelerEncounter : WorldEncounter
{
    public TravelerEncounter(string id, string name, Biome biome, string assetFile, string description,
        int difficulty = 10, int rewardGold = 4)
        : base(id, name, biome, assetFile, description, BackgroundStat.Charisma, difficulty, rewardGold) { }
    public override string ActionLabel => "KONUŞ / YARDIM ET";
    public override string SuccessText => "Yaklaşımın karşılık buldu; yolculuk bilgisi ve ödül kazandın.";
}

public sealed class HazardEncounter : WorldEncounter
{
    public HazardEncounter(string id, string name, Biome biome, string assetFile, string description,
        BackgroundStat stat = BackgroundStat.Dexterity, int difficulty = 12, int rewardGold = 3)
        : base(id, name, biome, assetFile, description, stat, difficulty, rewardGold) { }
    public override string ActionLabel => "AŞMAYI DENE";
    public override string SuccessText => "Tehlikeyi dikkatle aştın ve yolu güvenli hâle getirdin.";
}

public static class EncounterCatalog
{
    public static IReadOnlyList<WorldEncounter> All { get; } =
    [
        H("collapsed-signposts", "Yıkılmış Yön Tabelaları", Biome.Plains, "01-collapsed-signposts-v1.png", "Fırtına tabelaları devirmiş; yollar farklı yönlere ayrılıyor.", BackgroundStat.Wisdom, 9),
        H("safe-risky-crossroads", "Güvenli ve Riskli Yol", Biome.Plains, "02-safe-risky-crossroads-v1.png", "Aydınlık ana yol ile karanlık kestirme arasında seçim yapmalısın.", BackgroundStat.Wisdom, 10),
        H("broken-bridge", "Kırık Köprü", Biome.Plains, "03-broken-bridge-v1.png", "Köprünün orta kısmı çökmüş; geçiş yolu bulman gerekiyor.", BackgroundStat.Dexterity, 11),
        T("traveling-merchant", "Gezgin Tüccar", Biome.Plains, "04-traveling-merchant-v1.png", "Gezgin bir tüccar yol kenarında mola vermiş."),
        T("wounded-messenger", "Yaralı Haberci", Biome.Plains, "05-wounded-messenger-v1.png", "Yorgun haberci yardım istiyor ve taşıdığı haber acil görünüyor."),
        T("lost-pilgrim", "Kaybolmuş Hacı", Biome.Plains, "06-lost-pilgrim-v1.png", "Bir hacı uzaktaki tapınağa giden yolu bulamıyor."),
        D("abandoned-wagon", "Terk Edilmiş Araba", Biome.Plains, "07-abandoned-wagon-v1.png", "Hasarlı araba aceleyle bırakılmış; izler çalılıklara gidiyor."),
        T("road-toll", "Yol Geçiş Ücreti", Biome.Plains, "08-road-toll-v1.png", "Köprü görevlileri geçiş için ücret veya ikna bekliyor."),
        H("sudden-storm", "Ani Fırtına", Biome.Plains, "09-sudden-storm-v1.png", "Şiddetli fırtına yolu kapatmadan sığınak bulmalısın.", BackgroundStat.Constitution, 11),
        D("roadside-shrine", "Yol Kenarı Sunağı", Biome.Plains, "10-roadside-shrine-v1.png", "Eski sunakta yolcuların bıraktığı küçük adaklar var.", BackgroundStat.Wisdom),
        D("rare-herb", "Nadir Şifalı Bitki", Biome.Forest, "11-rare-medicinal-herb-v1.png", "Güneş ışığı nadir bir şifalı bitkiyi aydınlatıyor.", BackgroundStat.Wisdom),
        H("trapped-animal", "Tuzağa Yakalanmış Hayvan", Biome.Forest, "12-trapped-animal-v1.png", "Bir geyik halat tuzağından kurtulmaya çalışıyor.", BackgroundStat.Dexterity, 10),
        D("hunter-camp", "Terk Edilmiş Avcı Kampı", Biome.Forest, "13-abandoned-hunter-camp-v1.png", "Soğuk kamp ateşi ve taze izler avcının yakınlarda olduğunu düşündürüyor."),
        T("woodcutter-dispute", "Oduncuların Anlaşmazlığı", Biome.Forest, "14-woodcutter-boundary-dispute-v1.png", "İki oduncu grubu ağaç sınırı konusunda tartışıyor.", 11),
        D("fairy-ring", "Peri Çemberi", Biome.Forest, "15-fairy-ring-v1.png", "Ay ışığındaki mantar çemberi davetkâr ama açıklanamaz görünüyor.", BackgroundStat.Wisdom, 12),
        D("whispering-well", "Fısıldayan Kuyu", Biome.Forest, "16-whispering-well-v1.png", "Rüzgâr yokken kuyunun çıkrığı hareket ediyor."),
        D("forgotten-grave", "Unutulmuş Mezar", Biome.Forest, "17-forgotten-grave-v1.png", "İsimsiz mezarın toprağı yakın zamanda bozulmuş."),
        H("forest-fire", "Orman Yangını Dumanı", Biome.Forest, "18-forest-fire-smoke-v1.png", "Duman ana yolu kapatıyor; dereye inen yol daha güvenli olabilir.", BackgroundStat.Constitution, 12),
        H("giant-beehive", "Büyük Arı Kovanı", Biome.Forest, "19-giant-beehive-v1.png", "Dev kovandan bal almak dikkatli bir yaklaşım gerektiriyor.", BackgroundStat.Dexterity, 11),
        D("hidden-grove", "Gizli Koruluk", Biome.Forest, "20-hidden-grove-v1.png", "Köklerin ardında el değmemiş bir koruluk açılıyor.", BackgroundStat.Wisdom),
        D("swamp-lights", "Bataklık Işıkları", Biome.Swamp, "21-swamp-lights-v1.png", "Soluk ışıklar güvenli geçitten uzaklaşıp sise giriyor.", BackgroundStat.Wisdom, 12),
        H("sinking-chest", "Çamura Gömülen Sandık", Biome.Swamp, "22-sinking-chest-v1.png", "Sandık batmadan önce sağlam bir halat bağlamak gerekiyor.", BackgroundStat.Strength, 12),
        T("swamp-healer", "Bataklık Şifacısı", Biome.Swamp, "23-swamp-healer-hut-v1.png", "Şifacının kulübesi karanlık bataklıkta güvenli bir ışık yayıyor."),
        H("poison-fog", "Zehirli Sis", Biome.Swamp, "24-poisonous-fog-v1.png", "Zehirli sis ana geçidi kaplamış; yüksek kök yolu daha dar.", BackgroundStat.Constitution, 13),
        H("lily-crossing", "Dev Nilüfer Geçidi", Biome.Swamp, "25-giant-lily-pad-crossing-v1.png", "Bazı nilüferler ağırlığını taşıyabilir, bazıları yırtılmış.", BackgroundStat.Dexterity, 11),
        D("unknown-fruit", "Bilinmeyen Tropik Meyveler", Biome.Tropical, "26-unknown-tropical-fruit-v1.png", "Renkli meyvelerin hangisinin güvenli olduğu belirsiz.", BackgroundStat.Wisdom),
        T("fisherman", "Kıyıya Vurmuş Balıkçı", Biome.Tropical, "27-washed-up-fisherman-v1.png", "Fırtınadan kurtulan balıkçı su ve yardım istiyor."),
        D("tidal-cave", "Gelgit Mağarası", Biome.Tropical, "28-tidal-cave-v1.png", "Mağara düşük gelgitte açılmış; su yakında geri dönecek."),
        H("tropical-flood", "Ani Tropik Sel", Biome.Tropical, "29-sudden-tropical-flood-v1.png", "Yükselen sel suyunu düşmüş ağacın üzerinden geçebilirsin.", BackgroundStat.Dexterity, 12),
        D("vine-altar", "Sarmaşıklarla Kaplı Sunak", Biome.Tropical, "30-vine-covered-altar-v1.png", "Kökler eski sunağın hareketli bir bölümünü saklıyor."),
        H("avalanche", "Çığ Tehlikesi", Biome.Snow, "31-avalanche-danger-v1.png", "Çatlayan kar tabakası dar geçidin üzerinde asılı duruyor.", BackgroundStat.Wisdom, 13),
        T("mountain-monk", "Dağ Keşişi", Biome.Mountain, "32-mountain-monk-v1.png", "Keşiş soğuk geçitte sıcak çay ve öğüt sunuyor."),
        H("rope-bridge", "Hasarlı Halat Köprü", Biome.Mountain, "33-damaged-rope-bridge-v1.png", "Köprünün destek halatı yıpranmış ve tahtaları eksik.", BackgroundStat.Dexterity, 13),
        T("freezing-traveler", "Donmak Üzere Olan Gezgin", Biome.Snow, "34-freezing-traveler-v1.png", "Gezginin ateşi sönmek üzere; hızlı yardım gerekiyor."),
        D("hot-spring", "Doğal Sıcak Su Kaynağı", Biome.Snow, "35-natural-hot-spring-v1.png", "Buzlar arasındaki sıcak kaynak güvenli bir dinlenme alanı sunuyor.", BackgroundStat.Wisdom, 9),
        H("mine-elevator", "Terk Edilmiş Maden Asansörü", Biome.Underground, "36-abandoned-mine-elevator-v1.png", "Eski asansör çalışabilir ama destek kirişi çatlamış.", BackgroundStat.Intelligence, 12),
        D("eagle-nest", "Kartal Yuvası", Biome.Mountain, "37-eagle-nest-v1.png", "Yuvada parlak bir eşya var; kartal gökyüzünde dönüyor.", BackgroundStat.Wisdom, 12),
        H("frozen-lake", "Çatlayan Donmuş Göl", Biome.Snow, "38-cracking-frozen-lake-v1.png", "İnce buz ana yolu kaplıyor ve çatlaklar büyüyor.", BackgroundStat.Dexterity, 13),
        D("echo-cave", "Yankılı Mağara", Biome.Underground, "39-echoing-cave-v1.png", "Yankılar birden fazla tünelden geliyor; doğru yolu seçmelisin.", BackgroundStat.Wisdom),
        H("blocked-pass", "Karla Kapanmış Geçit", Biome.Snow, "40-snow-blocked-pass-v1.png", "Ana geçit kapanmış; dar çıkıntı veya eski tünel kalmış.", BackgroundStat.Strength, 12),
        H("sandstorm", "Kum Fırtınası", Biome.Desert, "41-sandstorm-v1.png", "Kum duvarı yaklaşırken kaya sığınağına ulaşmalısın.", BackgroundStat.Constitution, 12),
        D("buried-obelisk", "Kuma Gömülmüş Dikilitaş", Biome.Desert, "42-buried-obelisk-v1.png", "Dikilitaşın altında taş bir giriş açığa çıkmış."),
        T("caravan-wheel", "Kırılmış Kervan Tekeri", Biome.Desert, "43-broken-caravan-wheel-v1.png", "Yolcular kırılan tekeri onarmak için yardım arıyor."),
        D("dried-well", "Kurumuş Kuyu", Biome.Desert, "44-dried-well-v1.png", "Kuyu kurumuş ama gevşek taşın altında nem var."),
        D("mirage-oasis", "Serap Vahası", Biome.Desert, "45-mirage-oasis-v1.png", "Vaha gerçek olabilir; sıcak hava görüntüyü bozuyor.", BackgroundStat.Wisdom, 12),
        D("fossil-field", "Fosil Alanı", Biome.Desert, "46-fossil-field-v1.png", "Kayalardan dev fosiller çıkıyor; alan son derece kırılgan."),
        H("ash-rain", "Kül Yağmuru", Biome.Volcanic, "47-ash-rain-v1.png", "Kül görüşü azaltıyor; taş sığınak yakınında.", BackgroundStat.Constitution, 13),
        H("lava-channel", "Yolu Kesen Lav Kanalı", Biome.Volcanic, "48-lava-channel-v1.png", "Yeni lav akıntısı yolu kesmiş; soğuk taşlar geçiş sağlayabilir.", BackgroundStat.Dexterity, 14),
        D("corrupted-tree", "Fısıldayan Yozlaşmış Ağaç", Biome.Corrupted, "49-whispering-corrupted-tree-v1.png", "Yozlaşmış ağaç çevresindeki her şeyi kendine çekiyor.", BackgroundStat.Wisdom, 14),
        D("town-gate", "Görülen Kasaba Kapısı", Biome.Plains, "50-town-gate-sighted-v1.png", "Uzun yolun ardından açık kasaba kapısı göründü.", BackgroundStat.Wisdom, 8)
    ];

    public static WorldEncounter Pick(Biome biome, int seed)
    {
        WorldEncounter[] matches = All.Where(x => x.Biome == biome).ToArray();
        if (matches.Length == 0) matches = All.ToArray();
        return matches[Math.Abs(seed == int.MinValue ? 0 : seed) % matches.Length];
    }

    private static DiscoveryEncounter D(string id, string name, Biome biome, string file, string text,
        BackgroundStat stat = BackgroundStat.Intelligence, int difficulty = 11) =>
        new(id, name, biome, file, text, stat, difficulty);
    private static TravelerEncounter T(string id, string name, Biome biome, string file, string text, int difficulty = 10) =>
        new(id, name, biome, file, text, difficulty);
    private static HazardEncounter H(string id, string name, Biome biome, string file, string text,
        BackgroundStat stat, int difficulty) => new(id, name, biome, file, text, stat, difficulty);
}
