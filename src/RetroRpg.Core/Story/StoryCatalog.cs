using RetroRpg.Core.Characters;
using RetroRpg.Core.Common;
using RetroRpg.Core.Game;
using RetroRpg.Core.Items;
using RetroRpg.Core.World;

namespace RetroRpg.Core.Story;

// Kader Yolları: Göz Kulesi — bütün görevler ve hikâye olayları kararlı kimliklerle burada tanımlanır.
// Olayların sırası önemlidir: aynı hexte birden fazla olay varsa listede önce gelen oynar.
public static class StoryCatalog
{
    public const string PrologueId = "prologue-broken-caravan";
    public const string MainOne = "main-01-silinen-yol";
    public const string MainTwo = "main-02-uc-yol-tasi";
    public const string RootQuest = "waystone-root";
    public const string SkyQuest = "waystone-sky";
    public const string SunQuest = "waystone-sun";
    public const string ArchiveMessageQuest = "main-archive-message";
    public const string MainThree = "main-03-goz-kulesi";
    public const string RefugeesQuest = "side-forgotten-refugees";
    public const string PatrolQuest = "side-erased-patrol";
    public const string BishopQuest = "main-eye-bishop";
    public const string FateLensQuest = "main-fate-lens";
    public const string ScriptedDefeatQuest = "main-scripted-defeat";
    public const string ReturnQuest = "main-fatebreaker-return";
    public const string FinalChoiceQuest = "main-final-choice";

    public static readonly HexCoordinate Greenford = new(0, 0);
    public static readonly HexCoordinate CaravanWreck = new(1, 0);
    public static readonly HexCoordinate Redburg = new(2, -1);
    public static readonly HexCoordinate RootSite = new(-4, -1);
    public static readonly HexCoordinate SkySite = new(4, -3);
    public static readonly HexCoordinate SunSite = new(3, 3);
    public static readonly HexCoordinate PatrolSite = HexMap.PurpleCorridor[0];
    public static readonly HexCoordinate BridgeSite = HexMap.PurpleCorridor[1];

    private const string VaelorFirstIdle = "characters/story-boss-sprites-v1/sprites/vaelor/vaelor-bas-haritaci-idle-v1.png";
    private const string VaelorFirstAttack = "characters/story-boss-sprites-v1/sprites/vaelor/vaelor-bas-haritaci-attack-v1.png";
    private const string VaelorFinalIdle = "characters/story-boss-sprites-v1/sprites/vaelor/vaelor-kader-gozu-idle-v1.png";
    private const string VaelorFinalAttack = "characters/story-boss-sprites-v1/sprites/vaelor/vaelor-kader-gozu-attack-v1.png";
    private const string BishopIdle = "characters/boss-sprites-v1/sprites/corrupted/eye-bishop/corrupted-eye-bishop-idle-v1.png";
    private const string BishopAttack = "characters/boss-sprites-v1/sprites/corrupted/eye-bishop/corrupted-eye-bishop-attack-v1.png";
    private const string GuardianIdle = "characters/boss-sprites-v1/sprites/mountain/stormclaw-matriarch/mountain-stormclaw-matriarch-idle-v1.png";
    private const string GuardianAttack = "characters/boss-sprites-v1/sprites/mountain/stormclaw-matriarch/mountain-stormclaw-matriarch-attack-v1.png";

    public static QuestItem BlackMap() => new("Kara Harita Parçası", "story:black-map");
    public static QuestItem RootShard() => new("Kök Taşı Parçası", "story:waystone-root");
    public static QuestItem SkyShard() => new("Gök Taşı Parçası", "story:waystone-sky");
    public static QuestItem SunShard() => new("Güneş Taşı Parçası", "story:waystone-sun");
    private static Consumable Potion() => new("İyileştirme İksiri", 10, .5, 6);

    public static IReadOnlyList<Quest> CreateQuests() =>
    [
        new MainQuest(MainOne, "Silinen Yol",
            "Kara Harita Parçası, haritada duran ama herkesin unuttuğu bir yolu gösteriyor. Parçanın kaynağını bul.",
            new QuestObjective("archive-alternative", "Alternatif kaynak",
                "Arşiv kapandı. Kızılburç'ta göz işaretli subayı ya da anı levhasını bul.", Redburg),
            new QuestObjective("greenford-witness", "Han tanıkları", "Yeşilgeçit hanında tanıkları dinle: ÇEVREYİ ARA, İÇERİ GİR, HAN.", Greenford),
            new QuestObjective("caravan-wreck", "Kervan enkazı", "Doğudaki kervan enkazında (1,0) ÇEVREYİ ARA.", CaravanWreck),
            new QuestObjective("redburg-archive", "Kızılburç arşivi", "Kızılburç Kalesi'ne (2,-1) git ve arşive gir.", Redburg)),
        new MainQuest(MainTwo, "Üç Yol Taşı",
            "Bataklık, dağ ve çöldeki Yol Taşları'nı istediğin sırayla uyandır. Kuleye giden kapıyı ancak üçü birlikte açar.", null,
            new QuestObjective("root", "Kök Taşı", "Bataklıktaki Kök Taşı'nı uyandır."),
            new QuestObjective("sky", "Gök Taşı", "Dağlardaki Gök Taşı'nı uyandır."),
            new QuestObjective("sun", "Güneş Taşı", "Çöldeki Güneş Taşı'nı uyandır.")),
        new WaystoneQuest(RootQuest, "root", "Kök Taşı",
            "Bataklık köylüleri, yaşlı cadı ve tapınak ruhları batmış tapınaktaki taş için çekişiyor.",
            new QuestObjective("site", "Batmış tapınak", "Bataklığın merkezindeki batmış tapınağa git (-4,-1).", RootSite)),
        new WaystoneQuest(SkyQuest, "sky", "Gök Taşı",
            "Keşişler taşı uyandırmanın çığ getireceğinden korkuyor; yozlaşmış bir muhafız taşı kendine istiyor.",
            new QuestObjective("site", "Zirve manastırı", "Dağlardaki zirve manastırına çık (4,-3).", SkySite)),
        new WaystoneQuest(SunQuest, "sun", "Güneş Taşı",
            "Altın Liman ve Kızıl Vaha, eski gözlemevindeki taşı kendi ticaret yolları için istiyor.",
            new QuestObjective("site", "Çöl gözlemevi", "Çöldeki eski gözlemevine ulaş (3,3).", SunSite)),
        new MainQuest(ArchiveMessageQuest, "Kırık Arşivin Mesajı",
            "Kızılburç'taki göz mühürlü levha, iki taş uyanınca gizli metnini gösterecek.", null,
            new QuestObjective("second-stone", "İkinci taş", "İkinci Yol Taşı'nı uyandır.")),
        new MainQuest(MainThree, "Göz Kulesi",
            "Üç taşın açtığı güney kapısından mor bölgeye gir ve Göz Kulesi'ne ulaş.", null,
            new QuestObjective("gate", "Güney kapısı", "Mor bölgenin güney kapısından (-5,-3) gir.", HexMap.PurpleGate),
            new QuestObjective("tower", "Kule kapısı", "Göz Kulesi'nin (-3,-5) kapısına ulaş.", HexMap.EyeTower)),
        new SideQuest(RefugeesQuest, "Adını Unutanlar",
            "Kapının ardındaki mülteciler kendi adlarını birer birer unutuyor.",
            new QuestObjective("names", "Mülteciler", "Mültecilere adlarını hatırlat.", HexMap.PurpleGate)),
        new SideQuest(PatrolQuest, "Silinmiş Devriye",
            "Haritadan silinen bir devriye hâlâ var olmayan bir yolu bekliyor.",
            new QuestObjective("oath", "Devriye", "Devriyeye eski yeminini hatırlat.", PatrolSite)),
        new MainQuest(BishopQuest, "Göz Piskoposu",
            "Kuleye giden son köprüyü Vaelor'un Göz Piskoposu tutuyor.", null,
            new QuestObjective("bridge", "Köprü", "Piskoposu savaşla, iknayla ya da göz mührünü bozarak aş.", BridgeSite)),
        new MainQuest(FateLensQuest, "Kader Merceği",
            "Kulenin içinde Vaelor'un arşivi, Unutulanlar Hapishanesi ve Kader Merceği bekliyor.", null,
            new QuestObjective("archive", "Vaelor'un arşivi", "Vaelor'un arşivini incele."),
            new QuestObjective("prison", "Hapishane", "Unutulanlar Hapishanesi'nde {kisi} adını ara."),
            new QuestObjective("lens", "Mercek Odası", "Kader Merceği Odası'na ulaş.")),
        new MainQuest(ScriptedDefeatQuest, "Vaelor'la Yüzleşme",
            "Baş Haritacı Vaelor, Mercek'in önündeki salonda bekliyor.", null,
            new QuestObjective("hall", "Vaelor'un salonu", "Vaelor'un salonuna gir ve onunla yüzleş.", HexMap.EyeTower)),
        new MainQuest(ReturnQuest, "Kaderbozan Dönüş",
            "Kaderbozan Zırhı aynı darbeyi ikinci kez kabul etmez. Temizlenen kuleye dön.", null,
            new QuestObjective("return", "Kuleye dönüş", "Kaderbozan Zırhı'yla Göz Kulesi'ne (-3,-5) dön.", HexMap.EyeTower),
            new QuestObjective("vaelor", "Vaelor", "Vaelor'u gerçek savaşta yen.")),
        new MainQuest(FinalChoiceQuest, "Yolların Kaderi",
            "Vaelor yenildi. Kader Merceği'nin ve yolların kaderine sen karar vereceksin.", null,
            new QuestObjective("choice", "Final kararı", "Vaelor'un ve Kader Merceği'nin kaderini seç."))
    ];

    private static readonly Dictionary<string, string> QuestTitles = CreateQuests().ToDictionary(x => x.Id, x => x.Title);
    public static string QuestTitle(string questId) => QuestTitles.GetValueOrDefault(questId, questId);

    public static IReadOnlyList<EndingStoryEvent> Endings { get; } =
    [
        new(EndingRules.Restore, "Eski Yolları Geri Getir", _ => true, "Her zaman seçilebilir."),
        new(EndingRules.Share, "Yolları Halklara Paylaştır", EndingRules.CanShare,
            "2 uzlaşmacı taş + 2 taş bölgesinde güven"),
        new(EndingRules.Destroy, "Kader Merceği'ni Yok Et", _ => true, "Her zaman seçilebilir."),
        new(EndingRules.Watcher, "Yeni Gözcü Ol", _ => true, "Mercek'i kendine bağlarsın.")
    ];

    public static IReadOnlyList<StoryEvent> All { get; } = CreateEvents();

    public static StoryEvent? Find(string id) => All.FirstOrDefault(x => x.Id == id);

    private static IReadOnlyList<StoryEvent> CreateEvents() =>
    [
        Prologue(), GreenfordWitness(), CaravanWreckSearch(), RedburgArchive(), ArchiveAlternative(),
        RootWaystone(), SkyWaystone(), SkyGuardianFight(), SkyWaystoneSeized(), SunWaystone(), BrokenArchiveMessage(), PurpleGateOpening(),
        CorruptedRefugees(), ErasedPatrol(), MemoryEcho(), EyeBishopConfrontation(), EyeBishopFight(),
        TowerGateRest(), TowerArchive(), TowerPrison(), FateLensChamber(), VaelorFirstFight(), TowerFall(),
        RiderRevival(), FatebreakerReturn(), VaelorSecondForm(), VaelorFinalFight(),
        .. Endings
    ];

    // ---------------------------------------------------------------- Prolog ve Bölüm I

    private static StoryEvent Prologue() => new SkillCheckStoryEvent(PrologueId, "Haritadan Silinen Yol",
        StoryScene.Sequence("caravan-prologue-v1", 0, 4, choiceFrame: 1),
        [
            "Yeşilgeçit'in hemen dışında parçalanmış bir kervan yatıyor. Haritanda yol açıkça çizili, ama köylüler burada hiç yol olmadığına yemin ediyor.",
            "Enkazın gölgesinde son bir yolcu nefes alıyor. Elinde göz işaretli siyah bir harita parçası, yanında ölmüş bir haritacının yarım günlüğü var."
        ],
        [
            new StoryChoice("save-survivor", "Son yolcuyu kurtar",
                "Kanamayı durdurdun. Yolcu Yeşilgeçit'e kadar dayanacak ve handa tanıklık edecek.",
                [new FlagConsequence("prologue:survivor-saved"), new ReputationConsequence("greenford", 1)])
            {
                Check = new StoryCheck(BackgroundStat.Wisdom, 11,
                    Bonus: s => ClassIn(s, "cleric", "druid", "paladin") || HasChoice(s, "profession-healer") ? 2 : 0),
                FailureText = "Elinden geleni yaptın; yolcu son nefesinde yalnız 'Kızılburç' diyebildi.",
                Failure = [new FlagConsequence("prologue:survivor-lost")]
            },
            new StoryChoice("study-fragment", "Harita parçasını incele",
                "Rünler üç Yol Taşı'nı ve Baş Haritacı Vaelor'un mührünü gösteriyor.",
                [new FlagConsequence("knowledge:vaelor-early")])
            {
                Check = new StoryCheck(BackgroundStat.Intelligence, 12),
                FailureText = "Rünler gözlerini yaktı; yalnız bir göz mührü seçebildin.",
                Failure = [new CostConsequence(health: -2)]
            },
            new StoryChoice("follow-trail", "Kervanın izini sür",
                "İzler seni enkazın gizli bölmesine götürdü; kervan enkazındaki ipucunu şimdiden buldun.",
                [new FlagConsequence("prologue:trail-found"), new QuestConsequence(MainOne, QuestChange.CompleteObjective, "caravan-wreck")])
            {
                Check = new StoryCheck(BackgroundStat.Wisdom, 11, BackgroundStat.Dexterity),
                FailureText = "İz bataklığa saptı; geri dönerken yorgun düştün.",
                Failure = [new CostConsequence(health: -2)]
            }
        ])
    {
        Trigger = StoryTrigger.Start,
        AfterPages =
        [
            "Parça avucuna değdiğinde altın çizgiler kıpırdıyor. Günlüğün son satırı: 'Vaelor yolları siliyor; Yol Taşları uyanmalı.'",
            "Üç ışık ormana, dağlara ve çöle uzanıyor. Herkesin unuttuğu birini sen hâlâ hatırlıyorsun: {kisi}, {bag}."
        ],
        Always =
        [
            new ItemConsequence(BlackMap, replacesQuestKey: "story:frontier-map"),
            new QuestConsequence(MainOne, QuestChange.Start),
            RouteConsequence.MoveTo(Greenford, "Yeşilgeçit'e döndün")
        ]
    };

    private static StoryEvent GreenfordWitness() => new DialogueStoryEvent("greenford-witness", "Yeşilgeçit Hanı",
        StoryScene.Image("assets/final-production-v5/scenes/settlements/04-inn-tavern-interior-v1.png"),
        ["Handa herkes kervanı hatırlıyor ama yolu hatırlamıyor. Köylülere göre Kızılburç arşivi, haritalardan silinen her yolun kaydını tutar."],
        [
            new StoryChoice("survivor", "Kurtardığın yolcuyu dinle",
                "Yolcu, kervanın Kızılburç arşivine kayıtlı bir haritayı taşıdığını anlattı.",
                [new FlagConsequence("clue:witness-strong"), new ReputationConsequence("greenford", 1)])
            {
                Requirement = s => s.Story.HasFlag("prologue:survivor-saved"),
                RequirementText = "Prologda yolcuyu kurtarmalısın"
            },
            new StoryChoice("villagers", "Hancıyı ve köylüleri dinle",
                "Hancı, kervanın Kızılburç yolunda kaybolduğunu söyledi.", [new FlagConsequence("clue:witness")])
        ])
    {
        Trigger = StoryTrigger.Inn("greenford"),
        Requirement = s => s.Story.IsActive(MainOne) && !s.Story.Quest(MainOne).IsObjectiveDone("greenford-witness"),
        Always = [new QuestConsequence(MainOne, QuestChange.CompleteObjective, "greenford-witness")]
    };

    private static StoryEvent CaravanWreckSearch() => new SkillCheckStoryEvent("caravan-wreck-search", "Kervan Enkazı",
        StoryScene.Sequence("caravan-prologue-v1", 0, 1),
        ["Enkaz gün ışığında daha da tuhaf görünüyor: tekerlek izleri haritadaki yoldan geliyor, ama yolun kendisi yok."],
        [
            new StoryChoice("search", "Enkazı dikkatle ara",
                "Haritacının mühürlü defterini buldun: Kızılburç arşivine ait bir kayıt numarası.",
                [new FlagConsequence("clue:archive-ledger")])
            {
                Check = new StoryCheck(BackgroundStat.Intelligence, 11),
                FailureText = "Enkaz çöktü ve elini kestin, ama defterin yırtık bir sayfasını kurtardın.",
                Failure = [new CostConsequence(health: -2), new FlagConsequence("clue:archive-ledger")]
            },
            new StoryChoice("tracks", "Tekerlek izlerini oku",
                "İzler Kızılburç'un arka kapısına çıkıyor; kale muhafızlarından önce oraya varabilirsin.",
                [new FlagConsequence("clue:back-gate")])
            {
                Check = new StoryCheck(BackgroundStat.Wisdom, 11),
                FailureText = "İzler karıştı; yine de kervanın Kızılburç'a gittiği belli.",
                Failure = [new CostConsequence(health: -1)]
            }
        ])
    {
        Trigger = StoryTrigger.Search(CaravanWreck),
        CanPostpone = true,
        Requirement = s => s.Story.IsActive(MainOne) && !s.Story.Quest(MainOne).IsObjectiveDone("caravan-wreck"),
        Always = [new QuestConsequence(MainOne, QuestChange.CompleteObjective, "caravan-wreck")]
    };

    private static StoryEvent RedburgArchive() => new SkillCheckStoryEvent("redburg-archive", "Kızılburç Arşivi",
        StoryScene.Image("assets/final-production-v5/scenes/settlements/10-great-hall-war-room-v1.png"),
        ["Kızılburç arşivi muhafızlı salonun ardında. Silinen yolların kaydı burada olmalı; fakat görevli yabancılara kapıyı açmak istemiyor."],
        [
            new StoryChoice("persuade", "Muhafızı ikna et", "Muhafız kapıyı açtı ve seni arşive götürdü.", ArchiveSuccess())
            {
                Check = new StoryCheck(BackgroundStat.Charisma, 13,
                    Bonus: s => (s.Story.HasFlag("clue:witness-strong") ? 2 : 0) + (s.Story.HasFlag("clue:archive-ledger") ? 2 : 0)),
                FailureText = "Muhafızlar seni geri çevirdi; arşiv görevlisi korkup kayboldu.",
                Failure = ArchiveFailure()
            },
            new StoryChoice("credential", "Kimliğini göster", "Mührün kapıları açtı; arşiv görevlisi seni içeri aldı.", ArchiveSuccess())
            {
                Requirement = s => new CredentialApproach().CanAttempt(s.Player, LocationService.Barracks),
                RequirementText = "Soylu ya da muhafız geçmişi gerekir"
            },
            new StoryChoice("stealth", "Gece arşive sız", "Nöbet değişiminde içeri süzüldün.", ArchiveSuccess())
            {
                Check = new StoryCheck(BackgroundStat.Dexterity, 13,
                    Bonus: s => (ClassIn(s, "rogue", "ranger") ? 2 : 0) + (s.Story.HasFlag("clue:back-gate") ? 2 : 0)),
                FailureText = "Bir nöbetçi seni gördü; kaçarken yaralandın ve arşiv kapandı.",
                Failure = [.. ArchiveFailure(), new CostConsequence(health: -2)]
            }
        ])
    {
        Trigger = StoryTrigger.Arrive(Redburg),
        CanPostpone = true,
        Requirement = s => s.Story.IsActive(MainOne) && !((MainQuest)s.Story.Quest(MainOne)).UsingAlternative,
        Next = ["archive-alternative"]
    };

    private static StoryConsequence[] ArchiveSuccess() =>
        [new QuestConsequence(MainOne, QuestChange.CompleteObjective, "redburg-archive"), .. ChapterOneEnd("archive")];

    // Arşiv kapansa bile hikâye kilitlenmez: alternatif bilgi kaynağı hemen açılır.
    private static StoryConsequence[] ArchiveFailure() =>
        [new QuestConsequence(MainOne, QuestChange.Fail, note: "Arşiv kapandı; bilgi başka bir kaynaktan alınmalı.")];

    // Arşiv başarısı veya alternatif kaynak aynı bölüm sonunu üretir.
    private static StoryConsequence[] ChapterOneEnd(string source) =>
    [
        new FlagConsequence("archive:source", source),
        new QuestConsequence(MainOne, QuestChange.Complete, note: "Üç Yol Taşı'nın yeri öğrenildi."),
        new QuestConsequence(MainTwo, QuestChange.Start),
        new QuestConsequence(RootQuest, QuestChange.Start),
        new QuestConsequence(SkyQuest, QuestChange.Start),
        new QuestConsequence(SunQuest, QuestChange.Start),
        new QuestConsequence(ArchiveMessageQuest, QuestChange.Start)
    ];

    private static StoryEvent ArchiveAlternative() => new SkillCheckStoryEvent("archive-alternative", "Göz İşaretli İz",
        StoryScene.Image("assets/final-production-v5/scenes/settlements/11-dungeon-secret-passage-v1.png"),
        ["Kalenin altındaki geçitte göz işaretli bir subay ve duvara gömülü eski bir anı levhası var. İkisi de Yol Taşları'nı biliyor."],
        [
            new StoryChoice("officer", "Subayı sıkıştır",
                "Subay korkuyla üç taşın yerini itiraf etti.", ChapterOneEnd("officer"))
            {
                Check = new StoryCheck(BackgroundStat.Strength, 11, BackgroundStat.Dexterity),
                FailureText = "Subay kaçarken düşürdüğü haritayı kaptın, ama yaralandın.",
                Failure = [new CostConsequence(health: -3), .. ChapterOneEnd("officer")]
            },
            new StoryChoice("plaque", "Anı levhasını oku",
                "Levhanın yankısı bataklığı, dağı ve çölü gösterdi.", ChapterOneEnd("plaque"))
            {
                Check = new StoryCheck(BackgroundStat.Wisdom, 11),
                FailureText = "Levha zihnini yaktı; görüntüler bulanık ama üç taşın yeri seçiliyor.",
                Failure = [new CostConsequence(health: -3), .. ChapterOneEnd("plaque")]
            }
        ])
    {
        Requirement = s => s.Story.IsActive(MainOne) && ((MainQuest)s.Story.Quest(MainOne)).UsingAlternative,
        ResumeAt = Redburg,
        Always = [new QuestConsequence(MainOne, QuestChange.CompleteObjective, "archive-alternative")]
    };

    // ---------------------------------------------------------------- Bölüm II: Üç Yol Taşı

    private static StoryConsequence[] WaystoneDone(string questId, string stoneId, string mode, Func<Item> shard, string note) =>
    [
        new FlagConsequence(StoryFlags.WaystoneMode(stoneId), mode),
        new ItemConsequence(shard),
        new QuestConsequence(questId, QuestChange.Complete, note: note),
        new QuestConsequence(MainTwo, QuestChange.CompleteObjective, stoneId)
    ];

    // Statik alan değil özellik: All listesi oluşturulurken henüz başlatılmamış alan okunmasın.
    private static string[] AfterWaystone => ["broken-archive-message", "purple-gate-opening"];

    private static StoryEvent RootWaystone() => new SkillCheckStoryEvent("waystone-root-activation", "Kök Taşı",
        StoryScene.Sequence("root-waystone-v1", 0, 4, choiceFrame: 0),
        ["Bataklığın kalbinde batmış tapınak sudan yükseliyor. Köylüler taşı yok etmek, yaşlı cadı bağlamak, tapınak ruhları arındırmak istiyor."],
        [
            new StoryChoice("cleanse", "Taşı arındır", "Köylülerle tapınağı boşalttın; ruhlar huzura erdi.",
                [.. WaystoneDone(RootQuest, "root", "cleanse", RootShard, "Karar: arındırma."),
                 new ReputationConsequence("swamp", 2), new CostConsequence(health: 6)])
            {
                Check = new StoryCheck(BackgroundStat.Wisdom, 12,
                    Bonus: s => ClassIn(s, "cleric", "druid", "monk", "paladin") ? 2 : 0),
                FailureText = "Arındırma ağır oldu; ruhlar dağılırken seni de yaraladı.",
                Failure = [.. WaystoneDone(RootQuest, "root", "cleanse", RootShard, "Karar: arındırma (bedelli)."),
                           new ReputationConsequence("swamp", 1), new CostConsequence(health: -3)]
            },
            new StoryChoice("bind", "Cadıyla taşı bağla", "Cadı taşı köklere bağladı; bataklık sana borçlu.",
                [.. WaystoneDone(RootQuest, "root", "bind", RootShard, "Karar: bağlama."),
                 new ReputationConsequence("swamp", 1), new ItemConsequence(Potion)])
            {
                Check = new StoryCheck(BackgroundStat.Charisma, 12),
                FailureText = "Cadı anlaşmanın bedelini altınla istedi.",
                Failure = [.. WaystoneDone(RootQuest, "root", "bind", RootShard, "Karar: bağlama (pahalı)."), new CostConsequence(gold: -5)]
            },
            new StoryChoice("seize", "Ruhları yenip taşı al", "Ruhları dağıttın ve taşı zorla aldın.",
                [.. WaystoneDone(RootQuest, "root", "seize", RootShard, "Karar: zorla alma."),
                 new ReputationConsequence("swamp", -1), new CostConsequence(gold: 10)])
            {
                Check = new StoryCheck(BackgroundStat.Strength, 13),
                FailureText = "Ruhlar seni hırpaladı, ama taşı söküp aldın.",
                Failure = [.. WaystoneDone(RootQuest, "root", "seize", RootShard, "Karar: zorla alma (yaralı)."),
                           new ReputationConsequence("swamp", -2), new CostConsequence(health: -4)]
            }
        ])
    {
        Trigger = StoryTrigger.Arrive(RootSite),
        CanPostpone = true,
        OneShot = false,
        Requirement = s => s.Story.IsActive(RootQuest),
        AfterPages =
        [
            "Elini taşa koyduğunda kökler parlıyor ve yeşil bir damar taşın içinden yükseliyor.",
            "Yeşil ışık tapınağın sütunlarını sarıyor; bataklık suyu geri çekiliyor.",
            "Kök Taşı uyandı. Kara Harita Parçası'ndaki ilk yol yeniden parlıyor."
        ],
        Next = AfterWaystone
    };

    private static StoryEvent SkyWaystone() => new SkillCheckStoryEvent("waystone-sky-activation", "Gök Taşı",
        StoryScene.Sequence("sky-waystone-v1", 0, 4, choiceFrame: 0),
        ["Zirve manastırının çanları susmuş. Keşişler taşı uyandırmanın çığ getireceğinden korkuyor; yozlaşmış bir muhafız taşı kendine istiyor."],
        [
            new StoryChoice("protect", "Keşişlerle taşı koru", "Keşişler taşı birlikte koruma sözünü kabul etti ve sana kısa geçidi gösterdi.",
                [.. WaystoneDone(SkyQuest, "sky", "protect", SkyShard, "Karar: koruma."),
                 new ReputationConsequence("mountain", 2), MountainPass()])
            {
                Check = new StoryCheck(BackgroundStat.Wisdom, 12,
                    Bonus: s => (ClassIn(s, "monk", "cleric") ? 2 : 0) + (s.Flags.ContainsKey("encounter:mountain-monk") ? 2 : 0)),
                FailureText = "Keşişler ikna olmadı ama taşı korumana izin verdi; tırmanış seni yordu.",
                Failure = [.. WaystoneDone(SkyQuest, "sky", "protect", SkyShard, "Karar: koruma (zorlu)."),
                           new ReputationConsequence("mountain", 1), new CostConsequence(health: -2), MountainPass()]
            },
            new StoryChoice("awaken", "Eski madenden taşa ulaş", "Maden tünelinden çıkıp taşı tam güçle uyandırdın.",
                [.. WaystoneDone(SkyQuest, "sky", "awaken", SkyShard, "Karar: uyandırma."), MountainPass()])
            {
                Check = new StoryCheck(BackgroundStat.Intelligence, 13, BackgroundStat.Wisdom),
                FailureText = "Tünel çöktü; taşı uyandırdın ama keşişler seni suçluyor.",
                Failure = [.. WaystoneDone(SkyQuest, "sky", "awaken", SkyShard, "Karar: uyandırma (çığ)."),
                           new ReputationConsequence("mountain", -1), new CostConsequence(health: -3), MountainPass()]
            },
            // Taş henüz uyanmadı: uyanış kareleri savaştan önce değil, zaferden sonra oynar.
            new StoryChoice("seize", "Muhafızla savaş", "Yozlaşmış muhafız taşın önüne çıktı.", [new FlagConsequence("sky:fight")])
            {
                PlaysAfterFrames = false
            }
        ])
    {
        Trigger = StoryTrigger.Arrive(SkySite),
        CanPostpone = true,
        OneShot = false,
        Requirement = s => s.Story.IsActive(SkyQuest) && !s.Story.HasFlag("sky:fight"),
        AfterPages = SkyAwakeningPages,
        Next = ["sky-guardian-fight", .. AfterWaystone]
    };

    private static string[] SkyAwakeningPages =>
    [
        "Taşın üstündeki kanat işareti mavi bir ışıkla doluyor.",
        "Rüzgâr dönüyor; bulutlar manastırın çevresinde halka çiziyor.",
        "Gök Taşı uyandı. Dağların ardındaki yol haritada yeniden belirdi."
    ];

    private static RouteConsequence MountainPass() => RouteConsequence.Unlock("unlock:mountain-pass", "Kısa dağ geçidi açıldı");

    private static StoryEvent SkyGuardianFight() => new CombatStoryEvent("sky-guardian-fight", "Zirve Muhafızı",
        _ => new MiniBossNpc("Zirve Muhafızı", "boss.mountain.stormclaw-matriarch", 16, 11, new FighterClass(), 15,
            "Fırtına Pençesi") { Weapon = new Weapon("storm-claw", "Fırtına Pençesi", 0, 0, 6) },
        GuardianIdle, GuardianAttack,
        [.. WaystoneDone(SkyQuest, "sky", "seize", SkyShard, "Karar: zorla alma."),
         new ReputationConsequence("mountain", -1), MountainPass()])
    {
        Requirement = s => s.Story.IsActive(SkyQuest) && s.Story.HasFlag("sky:fight"),
        FleeAllowed = true,
        FleeClearsFlag = "sky:fight",
        ResumeAt = SkySite,
        Next = ["sky-waystone-seized"]
    };

    // Muhafız yenildikten sonra Gök Taşı'nın uyanışı. Kaçışta oynamaz; taş seçenekleri yeniden açılır.
    private static StoryEvent SkyWaystoneSeized() => new DialogueStoryEvent("sky-waystone-seized", "Gök Taşı",
        StoryScene.Sequence("sky-waystone-v1", 1, 3), SkyAwakeningPages)
    {
        Requirement = s => s.Story.Quest(SkyQuest).Status == QuestStatus.Completed && s.Story.WaystoneMode("sky") == "seize",
        ResumeAt = SkySite,
        Next = AfterWaystone
    };

    private static StoryEvent SunWaystone() => new SkillCheckStoryEvent("waystone-sun-activation", "Güneş Taşı",
        StoryScene.Sequence("sun-waystone-v1", 0, 4, choiceFrame: 0),
        ["Eski gözlemevi iki ticaret şehrinin arasında yükseliyor. Altın Liman ve Kızıl Vaha taşın ışığını kendi yollarına çevirmek istiyor."],
        [
            new StoryChoice("share", "Şehirleri uzlaştır", "İki şehir taşın ışığını ortak yola bağlamayı kabul etti.",
                [.. WaystoneDone(SunQuest, "sun", "share", SunShard, "Karar: paylaşım."), new ReputationConsequence("desert", 2)])
            {
                Check = new StoryCheck(BackgroundStat.Charisma, 13,
                    Bonus: s => (ClassIn(s, "bard") ? 2 : 0) + (HasChoice(s, "family-merchants", "profession-trader") ? 2 : 0)),
                FailureText = "Uzlaşma yarım kaldı; ışık iki şehre bölündü ve sen arada ezildin.",
                Failure = [.. WaystoneDone(SunQuest, "sun", "share", SunShard, "Karar: paylaşım (kırılgan)."),
                           new ReputationConsequence("desert", 1), new CostConsequence(health: -2)]
            },
            new StoryChoice("faction-a", "Altın Liman'ı destekle", "Altın Liman ticaret yolunu aldı ve sana ödeme yaptı.",
                [.. WaystoneDone(SunQuest, "sun", "faction-a", SunShard, "Karar: Altın Liman."), new CostConsequence(gold: 15)]),
            new StoryChoice("faction-b", "Kızıl Vaha'yı destekle", "Kızıl Vaha sana şifalı su ve iksir verdi.",
                [.. WaystoneDone(SunQuest, "sun", "faction-b", SunShard, "Karar: Kızıl Vaha."),
                 new ItemConsequence(Potion), new CostConsequence(fullHeal: true)]),
            new StoryChoice("steal", "Taşı gece çal", "Taşın kalbini sessizce aldın; iki şehir de seni arıyor.",
                [.. WaystoneDone(SunQuest, "sun", "steal", SunShard, "Karar: hırsızlık."),
                 new ReputationConsequence("desert", -1), new CostConsequence(gold: 5)])
            {
                Check = new StoryCheck(BackgroundStat.Dexterity, 13),
                FailureText = "Muhafızlar fark etti; taşı kaptın ama ağır yaralandın.",
                Failure = [.. WaystoneDone(SunQuest, "sun", "steal", SunShard, "Karar: hırsızlık (yaralı)."),
                           new ReputationConsequence("desert", -2), new CostConsequence(health: -4)]
            }
        ])
    {
        Trigger = StoryTrigger.Arrive(SunSite),
        CanPostpone = true,
        OneShot = false,
        Requirement = s => s.Story.IsActive(SunQuest),
        AfterPages =
        [
            "Gözlemevinin aynaları güneşi taşın merkezine topluyor.",
            "Altın ışık kumların üstünde iki şehri birbirine bağlayan bir çizgi çiziyor.",
            "Güneş Taşı uyandı. Çöl yolu haritada yeniden parlıyor."
        ],
        Next = AfterWaystone
    };

    private static StoryEvent BrokenArchiveMessage() => new DialogueStoryEvent("broken-archive-message", "Kırık Arşivin Mesajı",
        StoryScene.Sequence("archive-gate-v1", 0, 2),
        [
            "İki taş uyandığında Kızılburç'taki göz mühürlü levha ışıyor. Arşivin taş masasında üç yolun ışığı birleşiyor.",
            "Bilgi, {kaynak} ile doğrulanıyor: {kisi} Göz Kulesi'nde, Kader Merceği'nin içinde bir anı yankısına dönüştürüldü."
        ])
    {
        Requirement = s => s.Story.CompletedWaystones == 2,
        Always =
        [
            new FlagConsequence("archive:message"),
            new QuestConsequence(ArchiveMessageQuest, QuestChange.Complete, note: "{kisi} Göz Kulesi'nde tutuluyor.")
        ]
    };

    private static StoryEvent PurpleGateOpening() => new DialogueStoryEvent("purple-gate-opening", "Mor Kapı",
        StoryScene.Sequence("archive-gate-v1", 2, 2),
        [
            "Üç taşın ışığı mor bölgenin güneyindeki mühürlü kapıda buluşuyor. Zincirler titriyor.",
            "Kapı açıldı. Kuleye giden tek yol güney kapısından (-5,-3) geçiyor; diğer mor sınırlar mühürlü kalıyor."
        ])
    {
        Requirement = s => s.Story.CompletedWaystones == 3 && !s.Story.PurpleGateOpen,
        Always =
        [
            new FlagConsequence(StoryFlags.PurpleGateOpen),
            RouteConsequence.Unlock("unlock:purple-gate", "Mor bölgenin güney kapısı açıldı"),
            new QuestConsequence(MainTwo, QuestChange.Complete, note: "Üç taş uyandı."),
            new QuestConsequence(MainThree, QuestChange.Start)
        ]
    };

    // ---------------------------------------------------------------- Bölüm III: Mor bölge

    private static StoryEvent CorruptedRefugees() => new SkillCheckStoryEvent("corrupted-refugees", "Adını Unutanlar",
        StoryScene.Meaning("corrupted-region-v1", "forgotten-refugees"),
        ["Kapının ardında küçük bir mülteci kampı var. İnsanların yüzleri soluyor; çoğu kendi adını hatırlayamıyor."],
        [
            new StoryChoice("remember", "Adlarını birlikte hatırla", "Her biri adını yüksek sesle söyledi; yüzlerine renk döndü.",
                [new QuestConsequence(RefugeesQuest, QuestChange.Complete, note: "Mülteciler adlarını hatırladı."),
                 new ReputationConsequence("refugees", 2), new ItemConsequence(Potion)])
            {
                Check = new StoryCheck(BackgroundStat.Charisma, 12, Bonus: s => ClassIn(s, "bard") ? 2 : 0),
                FailureText = "Adlar sisle birlikte silindi; mülteciler seni de unutmaya başladı.",
                Failure = [new QuestConsequence(RefugeesQuest, QuestChange.Fail, note: "Adlar sisle silindi."), new CostConsequence(health: -2)]
            },
            new StoryChoice("share", "Erzağını paylaş", "Sıcak bir öğün, birkaç adı geri getirdi.",
                [new CostConsequence(supplies: 1),
                 new QuestConsequence(RefugeesQuest, QuestChange.Complete, note: "Erzak paylaşıldı."),
                 new ReputationConsequence("refugees", 1)])
            {
                Requirement = s => s.Player.Inventory.Find<Consumable>() is not null,
                RequirementText = "Paylaşacak erzak veya iksir gerekir"
            },
            new StoryChoice("pass", "Yoluna devam et", "Mültecileri geride bıraktın.",
                [new QuestConsequence(RefugeesQuest, QuestChange.Fail, note: "Mültecileri geride bıraktın.")])
        ])
    {
        Trigger = StoryTrigger.Pass(HexMap.PurpleGate),
        Requirement = s => s.Story.PurpleGateOpen,
        Always = [new QuestConsequence(MainThree, QuestChange.CompleteObjective, "gate")]
    };

    private static StoryEvent ErasedPatrol() => new SkillCheckStoryEvent("erased-patrol", "Silinmiş Devriye",
        StoryScene.Meaning("corrupted-region-v1", "erased-patrol"),
        ["Köprüde yarısı silinmiş bir devriye yürüyor. Askerler artık var olmayan bir yolu korumak için emir bekliyor."],
        [
            new StoryChoice("oath", "Yeminlerini hatırlat", "Devriye yeminini hatırladı ve Piskopos'a karşı seninle yürüyecek.",
                [new QuestConsequence(PatrolQuest, QuestChange.Complete, note: "Devriye sana katıldı."),
                 new FlagConsequence("ally:erased-patrol")])
            {
                Check = new StoryCheck(BackgroundStat.Wisdom, 12,
                    Bonus: s => HasChoice(s, "family-soldiers", "profession-guard", "education-military", "soldier-family") ? 2 : 0),
                FailureText = "Askerler seni yabancı sandı ve saldırdı; geri çekildin.",
                Failure = [new QuestConsequence(PatrolQuest, QuestChange.Fail, note: "Devriye silindi."), new CostConsequence(health: -3)]
            },
            new StoryChoice("sneak", "Sessizce geç", "Devriyenin gölgesinden geçtin; onlar yürümeye devam ediyor.",
                [new QuestConsequence(PatrolQuest, QuestChange.Fail, note: "Devriyeyi geride bıraktın.")])
            {
                Check = new StoryCheck(BackgroundStat.Dexterity, 11),
                FailureText = "Bir mızrak seni sıyırdı, ama karşı tarafa geçtin.",
                Failure = [new QuestConsequence(PatrolQuest, QuestChange.Fail, note: "Devriyeyi geride bıraktın."), new CostConsequence(health: -3)]
            }
        ])
    {
        Trigger = StoryTrigger.Pass(PatrolSite),
        Requirement = s => s.Story.PurpleGateOpen,
        Always = [new QuestConsequence(PatrolQuest, QuestChange.Start)]
    };

    private static StoryEvent MemoryEcho() => new SkillCheckStoryEvent("memory-echo", "Anı Yankısı",
        StoryScene.Meaning("corrupted-region-v1", "memory-echo"),
        ["Altın bir ışık köprünün üstünde sana el uzatıyor. Bu {kisi}, {bag}; ya da Mercek'in ondan bıraktığı yankı."],
        [
            new StoryChoice("hold", "Anıya tutun", "{kisi} sesini tanıdın. Yankı sana hapishanenin yolunu gösterdi.",
                [new FlagConsequence(StoryFlags.MemoryAnchored)])
            {
                Check = new StoryCheck(BackgroundStat.Wisdom, 11),
                FailureText = "Yankı dağıldı; geriye yalnız kulenin yönü kaldı.",
                Failure = [new CostConsequence(health: -2)]
            }
        ])
    {
        Trigger = StoryTrigger.Pass(BridgeSite),
        Requirement = s => s.Story.PurpleGateOpen,
        Next = ["eye-bishop-confrontation"]
    };

    private static StoryEvent EyeBishopConfrontation() => new SkillCheckStoryEvent("eye-bishop-confrontation", "Göz Piskoposu",
        StoryScene.Meaning("corrupted-region-v1", "eye-bishop"),
        ["Köprünün sonunda Göz Piskoposu bekliyor. Asasındaki göz seni süzüyor: 'Kader yazıldı, yolcu. Geri dön.'"],
        [
            new StoryChoice("persuade", "Piskoposu ikna et", "Piskopos gözlerini kapadı ve yolu açtı.", BishopDone("persuaded"))
            {
                Check = new StoryCheck(BackgroundStat.Charisma, 14,
                    Bonus: s => (s.Story.MemoryAnchored ? 2 : 0) + (ClassIn(s, "bard") ? 2 : 0)),
                FailureText = "Piskopos güldü ve asasını kaldırdı.",
                Failure = [new FlagConsequence("bishop:fight"), new CostConsequence(health: -2)]
            },
            new StoryChoice("seal", "Göz mührünü boz", "Asadaki mührü çözdün; Piskopos güçsüz kaldı.", BishopDone("sealed"))
            {
                Check = new StoryCheck(BackgroundStat.Intelligence, 13, BackgroundStat.Wisdom,
                    s => ClassIn(s, "wizard", "sorcerer", "warlock") ? 2 : 0),
                FailureText = "Mühür sana geri çarptı; Piskopos saldırıya geçti.",
                Failure = [new FlagConsequence("bishop:fight"), new CostConsequence(health: -2)]
            },
            new StoryChoice("fight", "Savaş", "Silahını çektin.", [new FlagConsequence("bishop:fight")])
        ])
    {
        Trigger = StoryTrigger.Pass(BridgeSite),
        OneShot = false,
        Requirement = s => s.Story.HasCompleted("memory-echo") && s.Story.Quest(BishopQuest).Status != QuestStatus.Completed &&
                           !s.Story.HasFlag("bishop:fight"),
        Always = [new QuestConsequence(BishopQuest, QuestChange.Start)],
        Next = ["eye-bishop-fight"]
    };

    private static StoryConsequence[] BishopDone(string how) =>
    [
        new FlagConsequence("bishop:result", how),
        new QuestConsequence(BishopQuest, QuestChange.Complete, note: how switch
        {
            "persuaded" => "Piskopos ikna oldu.",
            "sealed" => "Göz mührü bozuldu.",
            _ => "Piskopos savaşta yenildi."
        })
    ];

    private static StoryEvent EyeBishopFight() => new CombatStoryEvent("eye-bishop-fight", "Göz Piskoposu",
        s => new MiniBossNpc("Göz Piskoposu", "boss.corrupted.eye-bishop", s.Story.HasFlag("ally:erased-patrol") ? 12 : 18, 12,
            new ClericClass(), 25, "Göz Işını") { Weapon = new Weapon("eye-ray", "Göz Işını", 0, 0, 6) },
        BishopIdle, BishopAttack, BishopDone("defeated"))
    {
        Requirement = s => s.Story.HasFlag("bishop:fight") && s.Story.Quest(BishopQuest).Status != QuestStatus.Completed,
        FleeAllowed = true,
        FleeClearsFlag = "bishop:fight",
        ResumeAt = BridgeSite,
        PlayerAttackBonus = StoneAttackBonus,
        PlayerDamageBonus = StoneDamageBonus
    };

    // Üç Yol Taşı'nın ışığı mor bölgedeki hikâye savaşlarında oyuncuyu güçlendirir.
    private static int StoneAttackBonus(GameSession session) => session.Story.CompletedWaystones >= 3 ? 2 : 0;
    private static int StoneDamageBonus(GameSession session) => session.Story.CompletedWaystones;

    // ---------------------------------------------------------------- Bölüm IV: Göz Kulesi

    private static StoryEvent TowerGateRest() => new DialogueStoryEvent("tower-gate-rest", "Kule Kapısı",
        StoryScene.Image("assets/final-production-v1/scenes/routes/corrupted/05-corrupted-arrival-v1.png"),
        ["Kule kapısının önünde mor sisin değmediği küçük bir taş çember var. Burası Vaelor'un salonundan önceki son dinlenme noktası."],
        [new StoryChoice("rest", "Dinlen ve hazırlan", "Yaralarını sardın ve kuleye girdin.", [CostConsequence.FullHeal()])])
    {
        Trigger = StoryTrigger.Pass(HexMap.EyeTower),
        Requirement = s => s.Story.Quest(BishopQuest).Status == QuestStatus.Completed,
        Always =
        [
            new QuestConsequence(MainThree, QuestChange.Complete, note: "Kule kapısına ulaştın."),
            new QuestConsequence(FateLensQuest, QuestChange.Start)
        ],
        Next = ["tower-archive"]
    };

    private static StoryEvent TowerArchive() => new DialogueStoryEvent("tower-archive", "Vaelor'un Arşivi",
        StoryScene.Image("assets/final-production-v5/scenes/settlements/10-great-hall-war-room-v1.png"),
        ["Silinmiş yolların haritalarında aynı not var: 'Salgın ve savaş yolla gelir. Yol yoksa acı da yok.' Vaelor dünyayı yok etmek değil, yalnızlaştırmak istiyor."])
    {
        Always = [new FlagConsequence("tower:archive-read"), new QuestConsequence(FateLensQuest, QuestChange.CompleteObjective, "archive")],
        Next = ["tower-prison"]
    };

    private static StoryEvent TowerPrison() => new SkillCheckStoryEvent("tower-prison", "Unutulanlar Hapishanesi",
        StoryScene.Image("assets/final-production-v5/scenes/settlements/11-dungeon-secret-passage-v1.png"),
        ["Hapishanede adları silinmiş insanlar ışıktan zincirlerle bekliyor. Zincirlerden biri {kisi} adını fısıldıyor."],
        [
            PrisonChoice("chain", "Zinciri kır", new StoryCheck(BackgroundStat.Strength, 13)),
            PrisonChoice("seal", "Mührü çöz", new StoryCheck(BackgroundStat.Intelligence, 13)),
            PrisonChoice("name", "Adını seslen", new StoryCheck(BackgroundStat.Charisma, 12, BackgroundStat.Wisdom,
                s => s.Story.MemoryAnchored ? 3 : 0))
        ])
    {
        Always = [new QuestConsequence(FateLensQuest, QuestChange.CompleteObjective, "prison")],
        Next = ["fate-lens-chamber"]
    };

    private static StoryChoice PrisonChoice(string id, string label, StoryCheck check) =>
        new(id, label, "{kisi} zincirden kurtuldu ve seni hatırladı.", [new FlagConsequence(StoryFlags.PrisonerRescued)])
        {
            Check = check,
            FailureText = "Zincir seni yaktı; {kisi} yeniden Mercek'in yankısına çekildi.",
            Failure = [new CostConsequence(health: -3)]
        };

    private static StoryEvent FateLensChamber() => new DialogueStoryEvent("fate-lens-chamber", "Kader Merceği Odası",
        StoryScene.Image("scenes/biomes/08-eye-tower-abyss.png"),
        ["Kulenin tepesinde dev bir göz bütün yolları izliyor. Cebindeki üç taş parçası titriyor; Mercek onlarla ne yapacağını soruyor."],
        [
            new StoryChoice("share", "Taşları halklara bağla", "Taşları halkların yollarına bağlamaya karar verdin.", [new FlagConsequence("lens:intent", "share")]),
            new StoryChoice("restore", "Merceği onarmayı düşün", "Merceği onarıp eski yolları geri getirmeyi düşündün.", [new FlagConsequence("lens:intent", "restore")]),
            new StoryChoice("destroy", "Merceği kırmaya hazırlan", "Merceği kırmaya yemin ettin.", [new FlagConsequence("lens:intent", "destroy")]),
            new StoryChoice("watch", "Merceği kendine bağla", "Mercek'in gücünü kendine çekmeyi aklından geçirdin.", [new FlagConsequence("lens:intent", "watch")])
        ])
    {
        Always =
        [
            new QuestConsequence(FateLensQuest, QuestChange.Complete, note: "Mercek'in önünde Vaelor bekliyor."),
            new QuestConsequence(ScriptedDefeatQuest, QuestChange.Start)
        ],
        Next = ["vaelor-first-fight"]
    };

    // İlk savaş: oyuncu ilk eylemini yapar, ardından Vaelor Kaderin Hükmü'nü kullanır.
    // Can 0'a indiği için bu olaydan sonra autosave alınmaz; kayıt diriltmeden sonra alınır.
    private static StoryEvent VaelorFirstFight() => new CombatStoryEvent("vaelor-first-fight", "Baş Haritacı Vaelor",
        _ => new MainBossNpc("Baş Haritacı Vaelor", "boss.story.vaelor", 80, 14, new WizardClass(), 0, 1)
            { Weapon = new Weapon("fate-ray", "Kader Işını", 0, 0, 8) },
        VaelorFirstIdle, VaelorFirstAttack, [])
    {
        Requirement = s => s.Story.Quest(FateLensQuest).Status == QuestStatus.Completed && !s.Story.ScriptedDefeatPlayed,
        FateStrikeEventId = "tower-fall",
        SaveAfter = false,
        ResumeAt = HexMap.EyeTower,
        Next = ["tower-fall"]
    };

    private static StoryEvent TowerFall() => new ScriptedDefeatStoryEvent("tower-fall", "Kaderin Hükmü",
        StoryScene.Sequence("tower-fall-v1", 0, 6),
        [
            "Vaelor elini kaldırıyor: 'Kaderin Hükmü.' Darbe zırhına değil, yazgına iniyor.",
            "Kırmızı ışık göğsünü deliyor. Ne AC, ne kalkan, ne savunma; hiçbir şey bu darbeyi yavaşlatmıyor.",
            "Vaelor bedenini kulenin tepesinden mor boşluğa fırlatıyor.",
            "Kule duvarı yanından akıp gidiyor.",
            "Mor bulutlar seni yutuyor.",
            "Kulenin dibindeki yıkık taşlık karanlığa gömülüyor."
        ])
    {
        Requirement = s => s.Story.ScriptedDefeatPlayed && !s.Story.FatebreakerGranted,
        SaveAfter = false,
        Next = ["rider-revival"]
    };

    // Kalpaklı Süvari yalnız burada görünür; tek cümlesini söyler, zırhı bırakır ve ayrılır.
    public const string RiderLine = "“Seni öldüren güç bedenine değil, kaderine vurdu. Bu zırh aynı darbeyi ikinci kez kabul etmez.”";

    private static StoryEvent RiderRevival() => new DialogueStoryEvent("rider-revival", "Kulenin Dibinde",
        StoryScene.Sequence("rider-revival-v1", 0, 4),
        [
            "Kulenin dibindeki taşlıkta yatıyorsun. Sisin içinden kalpaklı bir süvari yaklaşıyor.",
            "Süvari atından iniyor ve elini durmuş kalbinin üstüne koyuyor.",
            "Mavi bir ışık göğsünü dolduruyor; kalbin yeniden atıyor.",
            RiderLine
        ],
        [new StoryChoice("armor", "ZIRHI KUŞAN",
            "Süvari mavi-altın zırhı yanına bıraktı ve tek söz daha etmeden sisin içinde kayboldu.",
            [
                CostConsequence.FullHeal(),
                new ItemConsequence(() => new FatebreakerArmor(), equip: true),
                new FlagConsequence(StoryFlags.Fatebreaker),
                RouteConsequence.MoveTo(BridgeSite, "Kulenin dibinde uyandın"),
                new QuestConsequence(ScriptedDefeatQuest, QuestChange.Complete,
                    note: "Kaderin Hükmü seni kuleden düşürdü; adını bilmediğin bir süvari seni geri getirdi."),
                new QuestConsequence(ReturnQuest, QuestChange.Start)
            ])])
    {
        Requirement = s => s.Story.ScriptedDefeatPlayed && !s.Story.FatebreakerGranted
    };

    private static StoryEvent FatebreakerReturn() => new ScriptedDefeatStoryEvent("fatebreaker-return", "Kaderbozan Dönüş",
        StoryScene.Sequence("fatebreaker-return-v1", 0, 4),
        [
            "Kuleye düşmansız dönüş yolundan giriyorsun; temizlediğin katlar sessiz. Vaelor salonda seni bekliyor.",
            "Vaelor şaşkın: 'Kaderin Hükmü!' Aynı kırmızı ışık yeniden göğsüne iniyor.",
            "Kaderbozan Zırhı darbeyi yutuyor ve kader ışığını paramparça ediyor.",
            "Vaelor ilk kez geri adım atıyor. Artık kader değil, kılıç ve zar konuşacak."
        ])
    {
        Trigger = StoryTrigger.Pass(HexMap.EyeTower),
        Requirement = s => s.Story.FatebreakerGranted && s.Story.VaelorState == VaelorState.FirstVictory,
        Always = [new FlagConsequence("fate-strike:blocked"), new QuestConsequence(ReturnQuest, QuestChange.CompleteObjective, "return")],
        Next = ["vaelor-second-form"]
    };

    private static StoryEvent VaelorSecondForm() => new DialogueStoryEvent("vaelor-second-form", "Kader Merceği",
        StoryScene.Sequence("fate-lens-v1", 0, 4),
        [
            "Kader Merceği seni göremiyor; Kaderbozan Zırhı yazgını gizliyor.",
            "Merceğe vurduğunda yüzeyi çatlıyor. Vaelor'un yollarla bağı kopmaya başlıyor.",
            "Vaelor Mercek'in gücünü kendine çekiyor ve gözlerle dolu ikinci biçimine dönüşüyor.",
            "Üç Yol Taşı'nın ışığı sana uzanıyor: saldırına +2 isabet ve taş başına +1 hasar."
        ],
        [StoryChoice.Continue("SAVAŞA HAZIRLAN")])
    {
        Requirement = s => s.Story.HasFlag("fate-strike:blocked") && s.Story.VaelorState == VaelorState.FirstVictory,
        Next = ["vaelor-final-fight"]
    };

    // Gerçek, kazanılabilir boss savaşı. Yenilgi normal ölüm ekranı ve son autosave akışıdır.
    private static StoryEvent VaelorFinalFight() => new CombatStoryEvent("vaelor-final-fight", "Vaelor, Kader Gözü",
        _ => new MainBossNpc("Vaelor, Kader Gözü", "boss.story.vaelor", 24, 11, new WizardClass(), 60, 2)
            { Weapon = new Weapon("fate-ray", "Kader Işını", 0, 0, 6) },
        VaelorFinalIdle, VaelorFinalAttack,
        [
            new FlagConsequence(StoryFlags.VaelorState, nameof(VaelorState.Defeated)),
            EndingConsequence.UnlockAvailable(),
            new QuestConsequence(ReturnQuest, QuestChange.Complete, note: "Vaelor gerçek savaşta diz çöktü."),
            new QuestConsequence(FinalChoiceQuest, QuestChange.Start)
        ])
    {
        Requirement = s => s.Story.HasCompleted("vaelor-second-form") && s.Story.VaelorState == VaelorState.FirstVictory,
        EnemyRemainsOnDefeat = true,
        ResumeAt = HexMap.EyeTower,
        PlayerAttackBonus = StoneAttackBonus,
        PlayerDamageBonus = StoneDamageBonus,
        Next = [.. Endings.Select(x => x.Id)]
    };

    // ---------------------------------------------------------------- Yardımcılar

    private static bool ClassIn(GameSession session, params string[] classIds) => classIds.Contains(session.Player.Class.Id);

    private static bool HasChoice(GameSession session, params string[] ids) =>
        ids.Contains(session.Player.Background.Id) || session.Player.Background.Choices.Any(x => ids.Contains(x.Id));
}
