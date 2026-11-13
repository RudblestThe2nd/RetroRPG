using RetroRpg.Core.Characters;
using System.Text;
using RetroRpg.Core.Common;

namespace RetroRpg.Core.World;

public sealed record EncounterOutcome(
    string Text,
    int GoldDelta = 0,
    int HealthDelta = 0,
    bool AddPotion = false,
    BackgroundStat? Stat = null,
    int StatDelta = 0,
    string? UnlockFlag = null,
    string? ItemId = null)
{
    public string Summary
    {
        get
        {
            var parts = new List<string> { Text };
            if (GoldDelta > 0) parts.Add($"+{GoldDelta} altın");
            if (GoldDelta < 0) parts.Add($"{GoldDelta} altın");
            if (HealthDelta > 0) parts.Add($"+{HealthDelta} CAN");
            if (HealthDelta < 0) parts.Add($"{HealthDelta} CAN");
            if (AddPotion) parts.Add("+1 iyileştirme iksiri");
            if (Stat is BackgroundStat stat && StatDelta != 0)
                parts.Add($"{EncounterReport.StatName(stat)} {(StatDelta > 0 ? "+" : "")}{StatDelta}");
            if (UnlockFlag is not null) parts.Add("yeni geçiş açıldı");
            if (ItemId is not null) parts.Add($"+{ItemName(ItemId)}");
            return string.Join(" · ", parts);
        }
    }

    private static string ItemName(string id) => id switch
    {
        "ration" => "Yol Azığı",
        "honey" => "Yabani Bal",
        "herb" => "Şifalı Ot",
        "lockpicks" => "Maymuncuk Takımı",
        "torch" => "Maceracı Meşalesi",
        _ => "Keşif Eşyası"
    };
}

public sealed record EncounterChoice(
    string Id,
    string Label,
    BackgroundStat CheckStat,
    int Difficulty,
    EncounterOutcome Success,
    EncounterOutcome Failure);

public static class EncounterChoiceCatalog
{
    public static IReadOnlyList<EncounterChoice> For(WorldEncounter encounter)
    {
        (string cautious, string bold) = Labels(encounter.EncounterId);
        BackgroundStat boldStat = SecondaryStat(encounter);

        IReadOnlyList<EncounterChoice>? special = Special(encounter, cautious, bold, boldStat);
        if (special is not null) return special;

        if (encounter is HazardEncounter)
        {
            string flag = $"route:{encounter.EncounterId}";
            return
            [
                new($"{encounter.EncounterId}:careful", cautious, encounter.CheckStat, encounter.Difficulty,
                    new("Geçidi güvenli hâle getirip ilerledin.", UnlockFlag: flag),
                    new("Geçit seni yaraladı; bu yoldan ilerleyemedin.", HealthDelta: -2)),
                new($"{encounter.EncounterId}:bold", bold, boldStat, encounter.Difficulty + 2,
                    new("Riskli hamle işe yaradı; kestirme açıldı.", UnlockFlag: flag),
                    new("Tehlikeye hazırlıksız yakalandın ve geri çekildin.", HealthDelta: -4))
            ];
        }

        if (encounter is TravelerEncounter)
        {
            string? item = encounter.EncounterId is "fisherman" or "freezing-traveler" ? "ration" : null;
            return
            [
                new($"{encounter.EncounterId}:careful", cautious, encounter.CheckStat, encounter.Difficulty,
                    new("Yolcu güvenini kazandı; faydalı bilgi paylaştı.", UnlockFlag: $"knowledge:{encounter.EncounterId}", ItemId: item),
                    new("Sözlerin karşılık bulmadı; yolcu konuşmayı kesti.")),
                new($"{encounter.EncounterId}:bold", bold, boldStat, encounter.Difficulty + 2,
                    new("Yardımın karşılığında güvenli bir rota öğrendin.", UnlockFlag: $"route:{encounter.EncounterId}"),
                    new("Yaklaşımın ters tepti; ilişkiyi düzeltemeden ayrıldın."))
            ];
        }

        bool treasure = encounter.EncounterId is "abandoned-wagon" or "forgotten-grave" or "sinking-chest" or
            "eagle-nest" or "buried-obelisk" or "dried-well" or "fossil-field";
        string? foundItem = encounter.EncounterId switch
        {
            "rare-herb" => "herb",
            "giant-beehive" => "honey",
            "hunter-camp" => "ration",
            "tidal-cave" => "torch",
            _ => null
        };

        return
        [
            new($"{encounter.EncounterId}:careful", cautious, encounter.CheckStat,
                encounter.Difficulty,
                new(treasure ? "Değerli bir buluntu çıkardın." : "İncelemen işe yaradı; çevre hakkında bilgi kazandın.",
                    GoldDelta: treasure ? encounter.RewardGold : 0, UnlockFlag: $"knowledge:{encounter.EncounterId}", ItemId: foundItem),
                new("İpuçlarını yanlış yorumladın; bir şey elde edemedin.")),
            new($"{encounter.EncounterId}:bold", bold, boldStat,
                encounter.Difficulty + 2,
                new(treasure ? "Riskli arama daha büyük bir buluntu verdi." : "Gizli ayrıntıyı keşfettin.",
                    GoldDelta: treasure ? encounter.RewardGold + 2 : 0, UnlockFlag: $"discovery:{encounter.EncounterId}", ItemId: foundItem),
                new("Riskli deneme zarar verdi.", HealthDelta: -2))
        ];
    }

    private static IReadOnlyList<EncounterChoice>? Special(WorldEncounter encounter, string cautious, string bold,
        BackgroundStat boldStat) => encounter.EncounterId switch
    {
        "mountain-monk" =>
        [
            new("mountain-monk:careful", cautious, BackgroundStat.Wisdom, 10,
                new("Keşişin öğretisini kavradın.", Stat: BackgroundStat.Wisdom, StatDelta: 1),
                new("Sözler bugün sende karşılık bulmadı.")),
            new("mountain-monk:bold", bold, BackgroundStat.Charisma, 12,
                new("Keşiş sana çığın ardındaki güvenli dağ geçidini gösterdi.", UnlockFlag: "unlock:mountain-pass"),
                new("Keşiş henüz bu sırrı paylaşmaya güvenmedi."))
        ],
        "mine-elevator" =>
        [
            new("mine-elevator:careful", cautious, BackgroundStat.Intelligence, 12,
                new("Asansörü onarıp yeraltı geçidini açtın.", UnlockFlag: "unlock:cave-route"),
                new("Mekanizma kilitlendi; geçit kapalı kaldı.")),
            new("mine-elevator:bold", bold, BackgroundStat.Strength, 14,
                new("Freni denetleyip aşağıdaki geçide ulaştın.", UnlockFlag: "unlock:cave-route"),
                new("Asansör sertçe durdu; geri çekilmek zorunda kaldın.", HealthDelta: -4))
        ],
        "traveling-merchant" =>
        [
            new("traveling-merchant:careful", cautious, BackgroundStat.Charisma, 10,
                new("Tüccarla indirim anlaşması yaptın.", UnlockFlag: "discount:market"),
                new("Tüccar fiyatını değiştirmedi.")),
            new("traveling-merchant:bold", bold, BackgroundStat.Wisdom, 12,
                new("Tüccardan güvenli kervan yolunu öğrendin.", UnlockFlag: "route:merchant"),
                new("Tüccarın verdiği tarif güvenilir görünmedi."))
        ],
        "road-toll" =>
        [
            new("road-toll:careful", cautious, BackgroundStat.Charisma, 11,
                new("Geçiş ücretini kaldırttın.", UnlockFlag: "route:toll-road"),
                new("Geçebilmek için 2 altın ödedin.", GoldDelta: -2, UnlockFlag: "route:toll-road")),
            new("road-toll:bold", bold, BackgroundStat.Strength, 13,
                new("Görevliler geri çekildi; yol açıldı.", UnlockFlag: "route:toll-road"),
                new("Muhafızlar seni yoldan çevirdi."))
        ],
        "roadside-shrine" =>
        [
            new("roadside-shrine:careful", cautious, BackgroundStat.Wisdom, 10,
                new("Dua zihnini berraklaştırdı.", HealthDelta: 2),
                new("Sunakta huzur bulamadın.")),
            new("roadside-shrine:bold", bold, BackgroundStat.Intelligence, 12,
                new("Eski işaretlerin anlamını çözdün.", UnlockFlag: "knowledge:shrine"),
                new("Adakları bozup elini yaraladın.", HealthDelta: -2))
        ],
        "rope-bridge" =>
        [
            new("rope-bridge:careful", cautious, BackgroundStat.Intelligence, 13,
                new("Köprüyü onardın; dağ yolu artık açık.", UnlockFlag: "unlock:mountain-pass"),
                new("Destek halatı koptu; geri çekildin.", HealthDelta: -2)),
            new("rope-bridge:bold", bold, BackgroundStat.Dexterity, 15,
                new("Köprüyü koşarak aştın; karşı yaka açıldı.", UnlockFlag: "unlock:mountain-pass"),
                new("Ayağın kaydı ve başlangıç tarafına düştün.", HealthDelta: -4))
        ],
        "giant-beehive" =>
        [
            new("giant-beehive:careful", cautious, BackgroundStat.Wisdom, 11,
                new("Arıları sakinleştirip bir parça bal aldın.", ItemId: "honey"),
                new("Arılar seni kovandan uzaklaştırdı.", HealthDelta: -1)),
            new("giant-beehive:bold", bold, BackgroundStat.Dexterity, 13,
                new("Peteği zarar vermeden kestin.", ItemId: "honey"),
                new("Arı sürüsü saldırdı.", HealthDelta: -3))
        ],
        _ => null
    };

    private static BackgroundStat SecondaryStat(WorldEncounter encounter)
    {
        if (encounter is TravelerEncounter) return BackgroundStat.Wisdom;
        if (encounter is HazardEncounter)
            return encounter.CheckStat == BackgroundStat.Strength ? BackgroundStat.Dexterity : BackgroundStat.Strength;
        return encounter.CheckStat == BackgroundStat.Intelligence ? BackgroundStat.Wisdom : BackgroundStat.Intelligence;
    }

    private static (string, string) Labels(string id) => id switch
    {
        "collapsed-signposts" => ("Tabelaları onar", "İzleri okuyup yön seç"),
        "safe-risky-crossroads" => ("Güvenli yolu seç", "Riskli kestirmeyi dene"),
        "broken-bridge" => ("Köprüyü sağlamlaştır", "İple karşıya geç"),
        "traveling-merchant" => ("Tüccarla pazarlık et", "Değerli rota bilgisi iste"),
        "wounded-messenger" => ("Yarasını sar", "Mesajı teslim al"),
        "lost-pilgrim" => ("Yolu tarif et", "Hacıyı tapınağa götür"),
        "abandoned-wagon" => ("Arabayı dikkatle ara", "Çalılıktaki izleri takip et"),
        "road-toll" => ("Ücreti düşürmeye çalış", "Geçiş hakkını savun"),
        "sudden-storm" => ("Yakında sığınak bul", "Fırtınada ilerlemeyi sürdür"),
        "roadside-shrine" => ("Sunakta dua et", "Eski adakları incele"),
        "rare-herb" => ("Bitkiyi dikkatle topla", "Köküyle birlikte sök"),
        "trapped-animal" => ("Tuzağı yavaşça çöz", "Halatı hızla kes"),
        "hunter-camp" => ("Kampı araştır", "Taze izlerin peşine düş"),
        "woodcutter-dispute" => ("Tarafları uzlaştır", "Sınırı yeniden ölç"),
        "fairy-ring" => ("Çemberi dışarıdan incele", "Çemberin içine adım at"),
        "whispering-well" => ("Kuyuyu dinle", "Kovayı derine indir"),
        "forgotten-grave" => ("Mezara saygı göster", "Bozulmuş toprağı kaz"),
        "forest-fire" => ("Dere yoluna sap", "Dumanın içinden koş"),
        "giant-beehive" => ("Arıları sakinleştir", "Peteği hızla kes"),
        "hidden-grove" => ("Koruluğu gözlemle", "Köklerin altını araştır"),
        "swamp-lights" => ("Işıkları uzaktan izle", "Işıkların peşinden git"),
        "sinking-chest" => ("Sandığa halat bağla", "Çamura girip sandığı çek"),
        "swamp-healer" => ("Şifacıyla konuş", "Nadir ilaç tarifini iste"),
        "poison-fog" => ("Yüksek kök yolunu seç", "Sisin içinden kestirme yap"),
        "lily-crossing" => ("Sağlam yaprakları seç", "Nilüferlerin üzerinden koş"),
        "unknown-fruit" => ("Meyveleri tanımla", "Bir meyveyi tadıp dene"),
        "fisherman" => ("Balıkçıya su ver", "Teknesini kurtarmaya yardım et"),
        "tidal-cave" => ("Gelgiti hesapla", "Mağaraya hemen gir"),
        "tropical-flood" => ("Düşmüş ağacı kullan", "Akıntıyı yüzerek geç"),
        "vine-altar" => ("Sarmaşıkları temizle", "Gizli taşı zorla aç"),
        "avalanche" => ("Sessizce geri çekil", "Geçidi hızla aş"),
        "mountain-monk" => ("Keşişin öğüdünü dinle", "Gizli geçidi sor"),
        "rope-bridge" => ("Destek halatını onar", "Köprüyü koşarak geç"),
        "freezing-traveler" => ("Ateşini yeniden yak", "Gezgini kampa taşı"),
        "hot-spring" => ("Suyu önce kontrol et", "Kaynakta uzun süre dinlen"),
        "mine-elevator" => ("Mekanizmayı onar", "Freni bırakıp aşağı in"),
        "eagle-nest" => ("Kartalın uzaklaşmasını bekle", "Yuvaya tırman"),
        "frozen-lake" => ("Buzu adım adım yokla", "Gölü koşarak geç"),
        "echo-cave" => ("Yankıları karşılaştır", "En güçlü yankıyı takip et"),
        "blocked-pass" => ("Karı temizle", "Dar çıkıntıdan dolaş"),
        "sandstorm" => ("Kaya sığınağına git", "Fırtınada yönünü koru"),
        "buried-obelisk" => ("Yazıtları çöz", "Taş kapıyı zorla aç"),
        "caravan-wheel" => ("Tekeri tamir et", "Arabayı birlikte kaldır"),
        "dried-well" => ("Nemli taşı incele", "Kuyunun dibine in"),
        "mirage-oasis" => ("İzleri doğrula", "Vahaya doğru ilerle"),
        "fossil-field" => ("Fosilleri kaydet", "Değerli parçayı çıkar"),
        "ash-rain" => ("Taş sığınağa gir", "Kül altında ilerle"),
        "lava-channel" => ("Soğuk taşları seç", "Lav kanalının üzerinden atla"),
        "corrupted-tree" => ("Fısıltıya diren", "Karanlık özü incele"),
        "town-gate" => ("Kapı muhafızına danış", "Kasabanın gizli geçidini sor"),
        _ => ("Dikkatle yaklaş", "Risk al")
    };
}

public static class EncounterReport
{
    public static string BuildMarkdown()
    {
        var text = new StringBuilder();
        text.AppendLine("# Random Encounter Listesi ve Sonuçları");
        text.AppendLine();
        text.AppendLine("Her karşılaşmada iki aktif seçenek ve sonuçsuz ayrılma seçeneği bulunur. Doğal 20 otomatik başarı, doğal 1 otomatik başarısızlıktır.");
        text.AppendLine();
        foreach (WorldEncounter encounter in EncounterCatalog.All)
        {
            text.AppendLine($"## {encounter.Name} — {BiomeName(encounter.Biome)}");
            text.AppendLine();
            text.AppendLine(encounter.Description);
            text.AppendLine();
            foreach (EncounterChoice choice in encounter.Choices)
            {
                text.AppendLine($"- **{choice.Label}** — d20 + {StatName(choice.CheckStat)}, Zorluk {choice.Difficulty}");
                text.AppendLine($"  - Başarı: {choice.Success.Summary}");
                text.AppendLine($"  - Başarısızlık: {choice.Failure.Summary}");
            }
            text.AppendLine("- **Uzaklaş** — Zar atılmaz; ödül veya ceza yoktur.");
            text.AppendLine();
        }
        return text.ToString();
    }

    public static string StatName(BackgroundStat stat) => stat switch
    {
        BackgroundStat.Strength => "GÜÇ",
        BackgroundStat.Dexterity => "ÇEVİKLİK",
        BackgroundStat.Constitution => "DAYANIKLILIK",
        BackgroundStat.Intelligence => "ZEKÂ",
        BackgroundStat.Wisdom => "BİLGELİK",
        _ => "KARİZMA"
    };

    private static string BiomeName(Biome biome) => biome switch
    {
        Biome.Forest => "Orman",
        Biome.Swamp => "Bataklık",
        Biome.Desert => "Çöl",
        Biome.Snow => "Kar",
        Biome.Mountain => "Dağ",
        Biome.Tropical => "Tropik",
        Biome.Volcanic => "Volkanik",
        Biome.Corrupted => "Yozlaşmış",
        Biome.Underground => "Yeraltı",
        _ => "Ova"
    };
}
