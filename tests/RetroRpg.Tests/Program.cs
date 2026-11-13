using RetroRpg.Core.Characters;
using RetroRpg.Core.Combat;
using RetroRpg.Core.Common;
using RetroRpg.Core.Assets;
using RetroRpg.Core.Content;
using RetroRpg.Core.Game;
using RetroRpg.Core.Items;
using RetroRpg.Core.Persistence;
using RetroRpg.Core.Rendering;
using RetroRpg.Core.Story;
using RetroRpg.Core.World;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RetroRpg.Tests;

internal static class Program
{
    private static int _passed;
    private static int _total;

    private static int Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--encounter-report")
        {
            string? directory = Path.GetDirectoryName(Path.GetFullPath(args[1]));
            if (directory is not null) Directory.CreateDirectory(directory);
            File.WriteAllText(args[1], EncounterReport.BuildMarkdown());
            Console.WriteLine(Path.GetFullPath(args[1]));
            return 0;
        }
        Run("Natural 20 kritik ve isabet", NaturalTwentyIsCritical);
        Run("Natural 1 kesin başarısızlık", NaturalOneAlwaysMisses);
        Run("Saldırı hasarı uygular", AttackDealsDamage);
        Run("Hex yalnızca komşuya hareket eder", HexMovementUsesNeighbors);
        Run("Harita tamamen açık ve rota bulur", OpenMapFindsWalkingRoute);
        Run("Irk ve sınıf kalıtımı çalışır", InheritanceCatalogsWork);
        Run("Eşya yuvası doğrulanır", EquipmentSlotsAreValidated);
        Run("JSON kayıt yazılır ve okunur", SaveRoundTripWorks);
        Run("Eşyasız eski kayıt başlangıç eşyası alır", OldSaveGetsStartingKit);
        Run("Asset kataloğu gerçek dosyaları çözer", AssetCatalogResolvesFiles);
        Run("İçerik JSON'u okunur", ContentJsonLoads);
        Run("Runtime karakter atlası kare seçer", RuntimeAtlasSelectsFrame);
        Run("Irk ve beden ekipman ankrajı okunur", EquipmentAnchorsLoad);
        Run("Npc listesi dost ve düşmanı taşır", NpcListHoldsFriendsAndEnemies);
        Run("EnemyNpc bir Npc'dir, CharacterClass'a sahiptir", EnemyNpcHasCharacterClass);
        Run("Alt sınıf ana sınıfı override eder", SubclassOverridesMainClass);
        Run("MainBossNpc EnemyNpc can davranışını kullanır", MainBossUsesEnemyHealth);
        Run("Smoke: oluştur, yürü, savaş, kaydet, yükle", SmokeFlowWorks);
        Run("Düşman ve hex karo görselleri çözülür", EnemyAndTileAssetsResolve);
        Run("Geçmiş yolu yedi aşamayı ve kaydı korur", BackgroundPathHasSevenStages);
        Run("50 savaş dışı encounter ve görselleri hazır", EncounterCatalogIsComplete);
        Run("Yerleşimler ortak sınıftan türer ve hizmet taşır", LocationInheritanceWorks);
        Run("Göz Kulesi zorunlu savaştır", EyeTowerForcesCombat);
        Run("Saldırı türü zar ve isabet ayarını uygular", AttackVariantWorks);
        Run("Her encounter iki sonuçlu seçenek taşır", EncounterChoicesHaveConsequences);
        Run("Pazar ve demirci satın alma katalogları hazır", ShopCatalogsWork);
        Run("Kuşanılan eşya çıkarılabilir", EquipmentCanBeUnequipped);
        Run("Pazar ve demirci eşya satın alır", ShopCanBuyFromPlayer);
        Run("Sınıf geçmiş ve hikâye başlangıç paketini değiştirir", StartingKitsUseCharacterChoices);
        Run("Muhafız erişimi kalıtımlı sosyal yaklaşımları kullanır", SocialAccessUsesApproaches);
        Run("Keşif sonucu kilitli geçidi açar", EncounterUnlocksRoute);
        Run("Encounter ödülleri temaya uygundur", EncounterRewardsAreThematic);
        Run("Editörde seçilen mor biyom engelleri haritaya uygulanır", TerrainBarriersForceChokePoints);
        Run("Hikâye seçimi prologu başlatır, haritaya atlamaz", StorySelectionStartsPrologue);
        Run("Prolog Kara Harita Parçası verir ve ana görevi başlatır", PrologueGivesBlackMapAndStartsQuest);
        Run("Hatırlanan kişi geçmiş önceliğine göre seçilir", RememberedPersonFollowsBackgroundPriority);
        Run("Üç Yol Taşı istenen sırada tamamlanır", WaystonesCompleteInAnyOrder);
        Run("İkinci taş Kırık Arşiv mesajını yalnız bir kez tetikler", SecondWaystoneTriggersArchiveOnce);
        Run("Üç taş olmadan mor kapı ve kule rotası kapalıdır", PurpleGateStaysClosedBeforeThreeStones);
        Run("Üç taş yalnız güney kapısını açar", ThreeStonesOpenOnlySouthGate);
        Run("Başarısız kontrol ana hikâyeyi kilitlemez", FailedChecksNeverLockMainStory);
        Run("Göz Piskoposu olayı tek sefer çalışır", EyeBishopRunsOnce);
        Run("İlk Vaelor savaşı scripted yenilgi üretir", FirstVaelorFightIsScriptedDefeat);
        Run("Scripted yenilgi ölüm ekranını tetiklemez", ScriptedDefeatDoesNotOpenDeathScreen);
        Run("Diriltme tam canla döndürür ve Kaderbozan verir", RiderRevivalRestoresHealthAndGrantsArmor);
        Run("Kaderbozan göğüs ekipmanını kaybettirmez", FatebreakerKeepsChestArmor);
        Run("Kaderbozan ikinci Kaderin Hükmü'nü engeller", FatebreakerBlocksSecondFateStrike);
        Run("İkinci Vaelor savaşı kazanılabilir ve normal ölüm kullanır", SecondVaelorFightIsWinnableWithNormalDeath);
        Run("Sürüm 1 ve 2 kayıtları sürüm 3 modeline yüklenir", OldSavesLoadIntoVersionThree);
        Run("Kayıt/yükleme tek-seferlik olayları tekrarlamaz", SaveLoadDoesNotRepeatOneShotEvents);
        Run("Dört final belirlenmiş durumlarla açılır", AllFourEndingsAreReachable);
        Run("Paylaştırma finali uzlaşma ve güven ister", ShareEndingRequiresCompromiseAndTrust);
        Run("Manifest kareleri gerçek 800x800 dosyalara çözülür", CinematicFramesResolveToRealFiles);
        Run("Gök Taşı savaş dalında uyanış yalnız zaferden sonra oynar", SkyAwakeningPlaysOnlyAfterGuardianVictory);
        Run("Mülteci güveni ve tek bölge paylaştırma finalini açmaz", RefugeeTrustDoesNotUnlockShareEnding);
        Run("Uçtan uca: prologdan finale bütün olaylar tek oturumda", FullStoryPlaysFromPrologueToEnding);
        Console.WriteLine($"Tüm testler geçti: {_passed}/{_total}");
        return 0;
    }

    private static void Run(string name, Action test)
    {
        _total++;
        try { test(); _passed++; Console.WriteLine($"PASS {name}"); }
        catch (Exception ex) { Console.Error.WriteLine($"FAIL {name}: {ex.Message}"); Environment.ExitCode = 1; throw; }
    }

    private static void NaturalTwentyIsCritical()
    {
        var roll = new AttackRoll(20, -5, 99);
        Assert(roll.IsCritical && roll.IsHit, "Doğal 20 her hedefte isabet etmelidir.");
    }

    private static void NaturalOneAlwaysMisses()
    {
        var roll = new AttackRoll(1, 99, 2);
        Assert(!roll.IsHit, "Doğal 1 bonus ne olursa olsun ıskalamalıdır.");
    }

    private static void AttackDealsDamage()
    {
        PlayerCharacter attacker = Player("Saldıran");
        PlayerCharacter defender = Player("Savunan");
        var combat = new CombatService(new FixedDiceRoller(15, 4));
        int before = defender.Health;
        AttackRoll roll = combat.Attack(attacker, defender);
        Assert(roll.IsHit && defender.Health < before, "Başarılı saldırı can azaltmalıdır.");
    }

    private static void HexMovementUsesNeighbors()
    {
        var session = new GameSession(Player("Gezgin"));
        Assert(session.TryMove(new HexCoordinate(1, 0)), "Komşu hexe yürünebilmelidir.");
        Assert(!session.TryMove(new HexCoordinate(3, 0)), "Uzak hexe tek adımda yürünmemelidir.");
    }

    private static void OpenMapFindsWalkingRoute()
    {
        HexMap map = HexMap.CreateDemoMap();
        Assert(map.Tiles.Values.All(x => x.Discovery == DiscoveryState.Discovered), "Fog of war kaldırılmalı; bütün hexler açık olmalıdır.");

        var start = new HexCoordinate(0, 0);
        HexCoordinate target = map.Tiles.Values.First(x => x.Coordinate.DistanceTo(start) == 4 && !x.IsLocked).Coordinate;
        IReadOnlyList<HexCoordinate> path = map.FindPath(start, target);
        Assert(path.Count > 1 && path[0] == start && path[^1] == target, "Rota başlangıçtan seçilen hedefe ulaşmalıdır.");
        Assert(path.Zip(path.Skip(1)).All(x => x.First.DistanceTo(x.Second) == 1), "Rota yalnız komşu hex adımlarından oluşmalıdır.");
        Assert(path.All(x => map.Get(x)?.IsLocked == false), "Rota kilitli hexlerden geçmemelidir.");
    }

    private static void InheritanceCatalogsWork()
    {
        Assert(RaceCatalog.All.All(x => x is CharacterRace), "Irklar CharacterRace'ten türemelidir.");
        Assert(ClassCatalog.All.All(x => x is CharacterClass && !x.IsSubclass), "Ana sınıflar CharacterClass'tan türemelidir.");
        Assert(ClassCatalog.Subclasses.Count == 24, "Alt sınıf kataloğu eksik.");
        foreach (CharacterClass subclass in ClassCatalog.Subclasses)
        {
            CharacterClass mainClass = ClassCatalog.Find(subclass.Id);
            Assert(mainClass.GetType().IsInstanceOfType(subclass), $"{subclass.GetType().Name}, {mainClass.GetType().Name} sınıfından türemelidir.");
        }
    }

    private static void NpcListHoldsFriendsAndEnemies()
    {
        List<Npc> npcs =
        [
            new FriendlyNpc("Köy Muhtarı", "npc.village.elder", "dialogue.elder.greeting"),
            ForestRaider()
        ];
        Assert(npcs.All(x => x.VisualAssetId.Length > 0), "Her NPC'nin görsel kimliği olmalıdır.");
        Assert(npcs[0].Interact() != npcs[1].Interact(), "Dost ve düşman Interact metodunu farklı override etmelidir.");
    }

    private static void EnemyNpcHasCharacterClass()
    {
        object enemy = ForestRaider();
        Assert(enemy is Npc && enemy is EnemyNpc, "BiomeEnemyNpc hem EnemyNpc hem Npc olmalıdır.");
        Assert(enemy is not CharacterClass, "EnemyNpc bir CharacterClass türü olmamalıdır.");
        Assert(((EnemyNpc)enemy).Class is FighterClass, "EnemyNpc sınıfını Class özelliğinde taşımalıdır.");
    }

    private static void SubclassOverridesMainClass()
    {
        FighterClass fighter = new FighterClass();
        FighterClass champion = new ChampionClass();
        Assert(champion.AttackBonus() == fighter.AttackBonus() + 1, "ChampionClass, FighterClass saldırı bonusunu override etmelidir.");
        Assert(champion.SignatureAction == fighter.SignatureAction, "Alt sınıf ana sınıfın imza hareketini devralmalıdır.");
        Assert(new AssassinClass().UsesDexterity && new ThiefClass().UsesDexterity, "Rogue alt sınıfları Dexterity kullanımını devralmalıdır.");
    }

    private static void MainBossUsesEnemyHealth()
    {
        var boss = new MainBossNpc("Kül Lordu", "boss.volcanic.lord", 40, 14, new FighterClass(), 100, 2);
        int firstPhaseBonus = boss.AttackBonus;
        var combat = new CombatService(new FixedDiceRoller(15, 3));
        AttackRoll roll = combat.Attack(Player("Kahraman"), boss);
        Assert(roll.IsHit && boss.Health < boss.MaxHealth, "Boss, EnemyNpc'den gelen can davranışıyla hasar almalıdır.");
        boss.ReceiveDamage(boss.Health - boss.MaxHealth / 2);
        Assert(boss.CurrentPhase == 2 && boss.AttackBonus == firstPhaseBonus + 2, "Boss canı yarıya inince ikinci aşamaya geçmelidir.");
    }

    private static void SmokeFlowWorks()
    {
        CharacterClass rogue = ClassCatalog.SubclassesOf("rogue").First();
        var player = new PlayerCharacter("Gölge", Gender.Male, RaceCatalog.All.First(), rogue,
            BackgroundCatalog.All.First(), new CharacterStats());
        var session = new GameSession(player);
        Assert(session.TryMove(new HexCoordinate(1, 0)), "Haritada komşu hexe yürünemedi.");

        EnemyNpc enemy = ForestRaider();
        var combat = new CombatService(new DiceRoller(new Random(7)));
        for (int round = 0; round < 100 && player.IsAlive && enemy.IsAlive; round++)
        {
            combat.Attack(player, enemy);
            if (enemy.IsAlive) combat.Attack(enemy, player);
        }
        Assert(!player.IsAlive || !enemy.IsAlive, "Savaş 100 turda bitmedi.");
        Assert(combat.Log.Count > 0 && combat.Log.All(x => x.Text.Contains("d20") || x.Text.StartsWith("Hasar")), "Savaş günlüğü zar ayrıntısı göstermiyor.");

        GameSession loaded = GameSession.FromSaveData(session.ToSaveData());
        Assert(loaded.Player.Class.SubclassId == rogue.SubclassId && loaded.CurrentHex == session.CurrentHex, "Kayıt sınıfı veya konumu korumadı.");
    }

    private static BiomeEnemyNpc ForestRaider() =>
        new("Orman Yağmacısı", "enemy.forest.raider", 13, 10, new FighterClass(), 12, Biome.Forest);

    private static void EquipmentSlotsAreValidated()
    {
        Weapon dagger = ItemCatalog.StartingWeapons.First();
        var set = new EquipmentSet();
        Assert(set.CanEquip(dagger, EquipmentSlot.MainHand), "Hançer ana ele takılabilmelidir.");
        Assert(!set.CanEquip(dagger, EquipmentSlot.Head), "Hançer baş yuvasına takılmamalıdır.");
    }

    private static void SaveRoundTripWorks()
    {
        string directory = Path.Combine(Path.GetTempPath(), "retro-rpg-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            var service = new SaveGameService(directory);
            PlayerCharacter saved = Player("Kayıtçı");
            saved.Inventory.Add(new Consumable("İyileştirme İksiri", 10, 0.5, 6));
            GameSaveData expected = new GameSession(saved).ToSaveData();
            service.Save(expected);
            GameSaveData actual = service.Load();
            Assert(actual.Character.Name == expected.Character.Name, "Kayıt karakteri geri okunamadı.");
            PlayerCharacter loaded = GameSession.FromSaveData(actual).Player;
            Assert(loaded.Class is ChampionClass, "Kayıttan alt sınıf geri yüklenemedi.");
            Assert(loaded.Equipment.Get(EquipmentSlot.MainHand) is Weapon { AssetId: "dagger" } && loaded.DamageDie == 4,
                "Kayıttan takılı silah geri yüklenemedi.");
            Assert(loaded.Inventory.Find<Consumable>() is { Healing: 6 } && loaded.Inventory.Items.Count == 2,
                "Kayıttan envanter geri yüklenemedi.");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    private static void OldSaveGetsStartingKit()
    {
        GameSaveData save = new GameSession(Player("Eski Kayıt")).ToSaveData();
        GameSaveData oldSave = save with { Version = 1, Character = save.Character with { Items = null } };
        PlayerCharacter loaded = GameSession.FromSaveData(oldSave).Player;
        Assert(loaded.Equipment.Get(EquipmentSlot.MainHand) is Weapon { AssetId: "greatsword" },
            "Eşyasız eski kayıt sınıfın başlangıç silahını almalıdır.");
        Assert(loaded.Inventory.Find<Consumable>() is not null, "Eşyasız eski kayıt iksir almalıdır.");
    }

    private static void EnemyAndTileAssetsResolve()
    {
        string root = Environment.CurrentDirectory;
        EnemyCatalog enemies = EnemyCatalog.Load(Path.Combine(root, "Content", "definitions", "enemies-v1.json"));
        Assert(enemies.Enemies.Count == 120, "Düşman tanımı sayısı 120 olmalıdır.");
        foreach (EnemyDefinition enemy in enemies.Enemies)
        {
            Assert(File.Exists(Path.Combine(root, enemies.SpritePath(enemy, "idle"))), $"{enemy.Id} sprite dosyası yok.");
            Assert(File.Exists(Path.Combine(root, enemies.TokenPath(enemy))), $"{enemy.Id} token dosyası yok.");
        }

        HexTilesetCatalog tiles = HexTilesetCatalog.Load(root);
        foreach (HexTile tile in HexMap.CreateDemoMap().Tiles.Values)
            Assert(File.Exists(Path.Combine(root, tiles.BaseTilePath(tile.Biome, tile.Coordinate))), $"{tile.Biome} karosu yok.");

        var session = new GameSession(Player("Temizleyen"));
        HexTile danger = session.Map.Tiles.Values.First(x => x.IsDangerous);
        session.ClearDanger(danger.Coordinate);
        GameSession loaded = GameSession.FromSaveData(session.ToSaveData());
        Assert(!loaded.Map.Get(danger.Coordinate)!.IsDangerous, "Temizlenen hex kayıttan sonra güvenli kalmalıdır.");
    }

    private static void AssetCatalogResolvesFiles()
    {
        AssetCatalog catalog = DefaultAssetCatalog.Create(Environment.CurrentDirectory);
        Assert(File.Exists(catalog.Resolve("dice.d20")), "D20 asset yolu çözülemedi.");
        Assert(File.Exists(catalog.Resolve("character.human.female")), "Karakter manifesti çözülemedi.");
        Assert(File.Exists(catalog.Resolve("ui.main-menu.background.wood-red-v1")), "Ahşap-kırmızı ana menü asseti çözülemedi.");
        Assert(File.Exists(catalog.Resolve("scene.combat.forest.pixel-v2")), "Piksel-art orman savaş sahnesi çözülemedi.");
        foreach (string id in catalog.Entries.Keys)
            Assert(File.Exists(catalog.Resolve(id)), $"Asset kataloğundaki dosya çözülemedi: {id}");
    }

    private static void ContentJsonLoads()
    {
        string path = Path.Combine(Environment.CurrentDirectory, "Content", "definitions", "game-data-v1.json");
        ContentDefinitionDocument data = new ContentDefinitionLoader().Load(path);
        Assert(data.Races.Count == 11 && data.Classes.Count == 12 && data.Subclasses.Count == 24, "İçerik sayıları beklenen değerlerde değil.");
    }

    private static void RuntimeAtlasSelectsFrame()
    {
        string root = Path.Combine(Environment.CurrentDirectory, "characters", "runtime-v1");
        string manifest = Path.Combine(root, "human", "female", "manifest.json");
        CharacterAtlasRuntime runtime = CharacterAtlasRuntime.Load(manifest, root);
        RuntimeFrame frame = runtime.Select(CharacterAnimation.Idle, Direction.South);
        Assert(File.Exists(runtime.ResolvePath(frame)), "Idle karakter karesi çözülemedi.");
    }

    private static void EquipmentAnchorsLoad()
    {
        string path = Path.Combine(Environment.CurrentDirectory, "assets", "final-production-v1", "equipment", "manifests", "equipment-runtime-v1.json");
        EquipmentAnchorCatalog anchors = EquipmentAnchorCatalog.Load(path);
        EquipmentAnchorProfile human = anchors.For("human", Gender.Female);
        Assert(human.Hands.ContainsKey(Direction.South) && anchors.Profiles.Count == 22, "Ekipman ankraj profilleri eksik.");
    }

    private static void BackgroundPathHasSevenStages()
    {
        BackgroundStageChoice[] choices = BackgroundCatalog.Stages
            .Select(stage => BackgroundCatalog.OptionsFor(stage)[1])
            .ToArray();
        CompositeBackground background = BackgroundCatalog.Create(choices);
        Assert(background.Choices.Count == 7, "Geçmiş yedi aşamadan oluşmalıdır.");
        Assert(background.Id.StartsWith("path/") && BackgroundCatalog.Find(background.Id).Choices.Count == 7,
            "Geçmiş yolu kimliğinden yeniden kurulamadı.");

        var player = new PlayerCharacter("Yolcu", Gender.Female, new HumanRace(), new ChampionClass(),
            background, new CharacterStats());
        GameSession loaded = GameSession.FromSaveData(new GameSession(player).ToSaveData());
        Assert(loaded.Player.Background.Id == background.Id && loaded.Player.Background.Choices.Count == 7,
            "Yedi aşamalı geçmiş kayıt ve yüklemede korunmadı.");
    }

    private static void EncounterCatalogIsComplete()
    {
        Assert(EncounterCatalog.All.Count == 50, "Encounter kataloğunda 50 kayıt olmalıdır.");
        Assert(EncounterCatalog.All.Select(x => x.EncounterId).Distinct().Count() == 50, "Encounter kimlikleri benzersiz olmalıdır.");
        Assert(EncounterCatalog.All.All(x => x is DiscoveryEncounter or TravelerEncounter or HazardEncounter),
            "Encounterlar WorldEncounter alt sınıflarından oluşmalıdır.");
        foreach (WorldEncounter encounter in EncounterCatalog.All)
            Assert(File.Exists(Path.Combine(Environment.CurrentDirectory, encounter.AssetPath)), $"Encounter görseli yok: {encounter.AssetPath}");
    }

    private static void LocationInheritanceWorks()
    {
        WorldLocation[] locations = [new VillageLocation("v", "Köy"), new TownLocation("t", "Kasaba"), new CastleLocation("c", "Kale")];
        Assert(locations.All(x => x.Services.Count >= 3), "Her yerleşimde en az üç hizmet olmalıdır.");
        Assert(locations[2] is CastleLocation && locations[2].Services.Contains(LocationService.Dungeon), "Kale zindan hizmetini taşımalıdır.");
        Assert(locations.All(x => File.Exists(Path.Combine(Environment.CurrentDirectory, x.SceneAsset))), "Yerleşim dış görünüşü eksik.");
        WorldLocation tower = WorldLocationCatalog.FindAt(HexMap.CreateDemoMap().Get(new HexCoordinate(-3, -5))!)!;
        Assert(tower is EyeTowerLocation && File.Exists(Path.Combine(Environment.CurrentDirectory, tower.SceneAsset)),
            "Göz Kulesi savaş sonrasında keşfedilebilir bir konum olmalıdır.");
    }

    private static void EyeTowerForcesCombat()
    {
        HexTile tower = HexMap.CreateDemoMap().Get(new HexCoordinate(-3, -5))!;
        Assert(tower.IsForcedCombat && tower.IsDangerous && !tower.IsLocked, "Göz Kulesi geçilebilir ve zorunlu savaş olmalıdır.");
    }

    private static void AttackVariantWorks()
    {
        PlayerCharacter attacker = Player("Varyant");
        PlayerCharacter defender = Player("Hedef");
        var combat = new CombatService(new FixedDiceRoller(10, 6));
        int before = defender.Health;
        AttackRoll roll = combat.Attack(attacker, defender, 5, 12, 2);
        Assert(roll.Bonus == attacker.AttackBonus + 5 && defender.Health < before, "Özel saldırı isabet ve hasar ayarını uygulamalıdır.");
    }

    private static void EncounterChoicesHaveConsequences()
    {
        foreach (WorldEncounter encounter in EncounterCatalog.All)
        {
            Assert(encounter.Choices.Count == 2, $"{encounter.Name} iki seçenek taşımıyor.");
            Assert(encounter.Choices.All(x => x.Difficulty > 0 && x.Success.Summary.Length > 0 && x.Failure.Summary.Length > 0),
                $"{encounter.Name} seçenek sonuçları eksik.");
        }
    }

    private static void ShopCatalogsWork()
    {
        Assert(ShopCatalog.For(LocationService.Market).Count >= 6, "Pazar kataloğu eksik.");
        Assert(ShopCatalog.For(LocationService.Blacksmith).Count >= 8, "Demirci kataloğu eksik.");
        Assert(ShopCatalog.For(LocationService.Market).Concat(ShopCatalog.For(LocationService.Blacksmith))
            .All(x => x.Price > 0 && x.CreateItem() is Item), "Satın alma teklifleri geçerli eşya üretmelidir.");
        PlayerCharacter buyer = Player("Alıcı");
        buyer.Gold = 20;
        ShopOffer offer = ShopCatalog.Market[0];
        int itemCount = buyer.Inventory.Items.Count;
        Assert(new ShopService().TryPurchase(buyer, offer, out _) && buyer.Gold == 10 && buyer.Inventory.Items.Count == itemCount + 1,
            "Satın alma altını düşürüp eşyayı envantere eklemelidir.");
    }

    private static void EquipmentCanBeUnequipped()
    {
        PlayerCharacter player = Player("Çıkaran");
        Item weapon = player.Equipment.Get(EquipmentSlot.MainHand)!;
        Assert(player.Equipment.IsEquipped(weapon), "Başlangıç silahı takılı olmalıdır.");
        Assert(player.Equipment.Unequip(weapon) && player.Equipment.Get(EquipmentSlot.MainHand) is null,
            "Kuşanılan silah çıkarılamadı.");
    }

    private static void ShopCanBuyFromPlayer()
    {
        PlayerCharacter seller = Player("Satıcı");
        Item weapon = seller.Inventory.Items.OfType<Weapon>().First();
        int before = seller.Gold;
        var shop = new ShopService();
        Assert(shop.TrySell(seller, weapon, out _) && seller.Gold == before + shop.SellPrice(weapon),
            "Satış altını artırmalıdır.");
        Assert(!seller.Inventory.Items.Contains(weapon) && !seller.Equipment.IsEquipped(weapon),
            "Satılan eşya envanterden ve ekipmandan çıkarılmalıdır.");
    }

    private static void StartingKitsUseCharacterChoices()
    {
        BackgroundStageChoice[] noblePath = BackgroundCatalog.DefaultPath().ToArray();
        noblePath[(int)BackgroundStage.Family] = BackgroundCatalog.OptionsFor(BackgroundStage.Family)
            .First(x => x.Id == "family-nobles");
        var paladin = new PlayerCharacter("Soylu", Gender.Female, new HumanRace(), new DevotionPaladinClass(),
            BackgroundCatalog.Create(noblePath), new CharacterStats());
        ItemCatalog.GiveStartingKit(paladin, "story-slot-1");
        Assert(paladin.Equipment.Get(EquipmentSlot.MainHand) is Weapon { AssetId: "warhammer" },
            "Paladin savaş çekiciyle başlamalıdır.");
        Assert(paladin.Inventory.Items.OfType<QuestItem>().Any(x => x.QuestKey == "credential:noble"),
            "Soylu geçmiş aile mührü vermelidir.");
        Assert(paladin.Inventory.Items.OfType<QuestItem>().Any(x => x.QuestKey == "story:frontier-map"),
            "Hikâye seçimi hikâye eşyası vermelidir.");
    }

    private static void SocialAccessUsesApproaches()
    {
        Assert(AccessApproachCatalog.All.Count == 3 && AccessApproachCatalog.All.All(x => x is AccessApproach),
            "Sosyal yaklaşımlar ortak tabandan türemelidir.");
        BackgroundStageChoice[] noblePath = BackgroundCatalog.DefaultPath().ToArray();
        noblePath[(int)BackgroundStage.Family] = BackgroundCatalog.OptionsFor(BackgroundStage.Family)
            .First(x => x.Id == "family-nobles");
        var noble = new PlayerCharacter("Soylu", Gender.Female, new HumanRace(), new ChampionClass(),
            BackgroundCatalog.Create(noblePath), new CharacterStats());
        AccessApproach credential = AccessApproachCatalog.All.OfType<CredentialApproach>().Single();
        Assert(credential.CanAttempt(noble, LocationService.WarRoom) && credential.AutomaticSuccess(noble, LocationService.WarRoom),
            "Soylu kimliği savaş odasında geçerli olmalıdır.");
    }

    private static void EncounterUnlocksRoute()
    {
        var session = new GameSession(Player("Kaşif"));
        HexTile pass = session.Map.Get(new HexCoordinate(3, -2))!;
        Assert(pass.IsLocked && pass.LockReason is not null, "Dağ geçidi başlangıçta gerekçesiyle kilitli olmalıdır.");
        session.ApplyUnlock("unlock:mountain-pass");
        Assert(!pass.IsLocked && pass.LockReason is null, "Keşif sonucu dağ geçidini açmalıdır.");
    }

    private static void TerrainBarriersForceChokePoints()
    {
        var session = new GameSession(Player("Yolcu"));
        HashSet<HexCoordinate> expected =
        [
            new(2, -6), new(1, -5),
            new(-4, -4), new(-3, -4), new(-2, -4), new(0, -4),
            new(-3, -3), new(-2, -3)
        ];
        HashSet<HexCoordinate> actual = session.Map.Tiles.Values
            .Where(x => x.IsPermanentObstacle)
            .Select(x => x.Coordinate)
            .ToHashSet();

        Assert(actual.SetEquals(expected),
            "Haritadaki kalıcı engeller editörde kaydedilen sekiz mor biyom koordinatıyla aynı olmalıdır.");
        Assert(expected.All(x => session.Map.Get(x) is { IsLocked: true, IsPermanentObstacle: true }),
            "Seçilen her dağ hex'i rota bulmada gerçekten geçilmez olmalıdır.");
    }

    private static void EncounterRewardsAreThematic()
    {
        WorldEncounter monk = EncounterCatalog.All.First(x => x.EncounterId == "mountain-monk");
        Assert(monk.Choices[0].Success.Stat == BackgroundStat.Wisdom && monk.Choices[0].Success.GoldDelta == 0,
            "Keşiş öğüdü altın değil Bilgelik vermelidir.");
        WorldEncounter bridge = EncounterCatalog.All.First(x => x.EncounterId == "rope-bridge");
        Assert(bridge.Choices.All(x => x.Success.GoldDelta == 0 && x.Success.UnlockFlag is not null),
            "Köprüyü aşmak altın değil rota açmalıdır.");
    }

    // ------------------------------------------------------------------ Ana hikâye testleri

    private static void StorySelectionStartsPrologue()
    {
        Assert(StorySlotCatalog.Slots[0].Title == "Kader Yolları: Göz Kulesi" && !StorySlotCatalog.Slots[0].IsLocked &&
               StorySlotCatalog.Slots.Skip(1).All(x => x.IsLocked), "Yalnız ilk yuva Kader Yolları olarak açık olmalıdır.");
        GameSession session = StorySession();
        StoryEvent? first = StoryDirector.ResumeEvent(session);
        Assert(first is { Id: StoryCatalog.PrologueId } && first.Scene.SequenceId == "caravan-prologue-v1",
            "Hikâye seçiminden sonra harita değil kervan prologu açılmalıdır.");
        Assert(session.Story.Chapter == 0 && !session.Story.PrologueCompleted, "Prolog bitmeden bölüm başlamamalıdır.");
    }

    private static void PrologueGivesBlackMapAndStartsQuest()
    {
        GameSession session = StorySession();
        session.TryMove(new HexCoordinate(1, 0));
        Assert(HasQuestItem(session, "story:frontier-map"), "Başlangıç paketi eski Sınır Yolu Haritası'nı vermelidir.");
        Play(session, StoryCatalog.PrologueId, "save-survivor", 20);
        Assert(session.Player.Inventory.Items.OfType<QuestItem>().Count(x => x.QuestKey == "story:black-map") == 1 &&
               !HasQuestItem(session, "story:frontier-map"), "Prolog tek bir Kara Harita Parçası vermeli ve eski haritanın yerine geçmelidir.");
        Assert(session.Story.IsActive(StoryCatalog.MainOne) && session.Story.Chapter == 1, "main-01-silinen-yol başlamalıdır.");
        Assert(session.CurrentHex == new HexCoordinate(0, 0), "Oyuncu Yeşilgeçit'te (0,0) başlamalıdır.");
        Assert(session.Story.Remembered is not null && session.Story.HasFlag("prologue:survivor-saved"),
            "Hatırlanan kişi ve prolog seçimi kaydedilmelidir.");
        Assert(StoryDirector.ResumeEvent(session) is null, "Prolog ikinci kez açılmamalıdır.");
    }

    private static void RememberedPersonFollowsBackgroundPriority()
    {
        (string[] Choices, string Type)[] cases =
        [
            (["turning-loss", "family-soldiers", "goal-protect"], "lost-friend"),
            (["turning-oath", "family-nobles"], "sworn-ward"),
            (["goal-protect"], "sworn-ward"),
            (["profession-guard", "family-nobles"], "forgotten-commander"),
            (["family-nobles", "profession-scribe"], "family-envoy"),
            (["profession-scribe", "childhood-caravan"], "lost-mentor"),
            (["childhood-caravan"], "caravan-leader"),
            ([], "lost-traveler")
        ];
        foreach ((string[] choices, string type) in cases)
        {
            RememberedPerson person = RememberedPerson.From(PathWith(choices));
            Assert(person.Type == type && person.Name.Length > 0 && person.Relation.Length > 0,
                $"{string.Join(",", choices)} geçmişi {type} üretmelidir; {person.Type} üretti.");
        }
        Assert(RememberedPerson.From(BackgroundCatalog.Find("soldier-family")).Type == "forgotten-commander",
            "Eski asker ailesi kaydı unutulan komutanı hatırlamalıdır.");
    }

    private static void WaystonesCompleteInAnyOrder()
    {
        GameSession session = ChapterTwoSession();
        Assert(StoryDirector.EventOnStep(session, StoryCatalog.SunSite, routeEnd: false) is null,
            "Yol Taşı olayı yalnız rota orada bitince açılmalıdır.");
        StoryEvent? sun = StoryDirector.EventOnStep(session, StoryCatalog.SunSite, routeEnd: true);
        Assert(sun?.Id == "waystone-sun-activation", "Güneş Taşı ilk sırada oynanabilmelidir.");
        Play(session, sun!.Id, "share", 20);
        Assert(Play(session, "waystone-root-activation", "cleanse", 20)?.Id == "broken-archive-message", "İkinci taş arşivi açmalıdır.");
        Assert(Play(session, "broken-archive-message", "continue") is null, "Arşiv mesajından sonra zincir sürmemelidir.");

        StoryEvent? fight = Play(session, "waystone-sky-activation", "seize");
        Assert(fight is CombatStoryEvent { Id: "sky-guardian-fight" }, "Muhafızla savaş seçimi gerçek savaşı açmalıdır.");
        StoryEvent? awakening = WinBattle(session, (CombatStoryEvent)fight!);
        Assert(awakening?.Id == "sky-waystone-seized", "Muhafız yenilince önce Gök Taşı'nın uyanışı oynamalıdır.");
        StoryEvent? gate = Play(session, awakening!.Id, "continue");
        Assert(gate?.Id == "purple-gate-opening", "Üçüncü taş mor kapı olayını açmalıdır.");

        Assert(new[] { StoryCatalog.RootQuest, StoryCatalog.SkyQuest, StoryCatalog.SunQuest }
                .All(x => session.Story.Quest(x).Status == QuestStatus.Completed), "Üç taş görevi tamamlanmalıdır.");
        Assert(session.Story.WaystoneMode("sun") == "share" && session.Story.WaystoneMode("root") == "cleanse" &&
               session.Story.WaystoneMode("sky") == "seize", "Her taşın karar modu kaydedilmelidir.");
        Assert(new[] { "story:waystone-root", "story:waystone-sky", "story:waystone-sun" }.All(x => HasQuestItem(session, x)),
            "Her taş kendi parçasını vermelidir.");
        Assert(session.Flags.ContainsKey("unlock:mountain-pass"), "Gök Taşı mevcut dağ geçidi bayrağını kullanmalıdır.");
    }

    private static void SecondWaystoneTriggersArchiveOnce()
    {
        GameSession session = ChapterTwoSession();
        Assert(Play(session, "waystone-root-activation", "bind", 20) is null, "İlk taş arşiv mesajını açmamalıdır.");
        StoryEvent? message = Play(session, "waystone-sun-activation", "faction-a");
        Assert(message?.Id == "broken-archive-message", "İkinci taş Kırık Arşiv mesajını açmalıdır.");
        Assert(message!.Scene is { SequenceId: "archive-gate-v1", FirstFrame: 0, FrameCount: 2 }, "Mesaj arşiv paketinin ilk iki karesini kullanmalıdır.");
        Play(session, message.Id, "continue");
        Assert(session.Story.Quest(StoryCatalog.ArchiveMessageQuest).Status == QuestStatus.Completed &&
               session.Story.HasFlag("archive:message"), "Arşiv görevi tamamlanmalıdır.");
        Assert(!message.CanTrigger(session), "Arşiv mesajı ikinci kez tetiklenmemelidir.");
        StoryEvent? third = Play(session, "waystone-sky-activation", "protect", 20);
        Assert(third?.Id == "purple-gate-opening", "Üçüncü taş arşiv mesajını değil mor kapıyı açmalıdır.");
    }

    private static void PurpleGateStaysClosedBeforeThreeStones()
    {
        GameSession session = ChapterTwoSession();
        Assert(session.Map.FindPath(session.CurrentHex, HexMap.EyeTower).Count == 0, "Başlangıçta kuleye rota olmamalıdır.");
        Play(session, "waystone-root-activation", "cleanse", 20);
        Play(session, "broken-archive-message", "continue", requireTrigger: false);
        Play(session, "waystone-sun-activation", "share", 20);
        Play(session, "broken-archive-message", "continue");
        Assert(session.Map.Get(HexMap.PurpleGate)!.IsLocked && HexMap.PurpleCorridor.All(x => session.Map.Get(x)!.IsLocked),
            "İki taşla mor kapı açılmamalıdır.");
        Assert(session.Map.FindPath(session.CurrentHex, HexMap.EyeTower).Count == 0, "İki taşla kuleye rota bulunmamalıdır.");
        Assert(session.Map.Get(HexMap.PurpleGate)!.LockReason!.Contains("Üç Yol Taşı"), "Kapı kilidi gerekçesini göstermelidir.");
    }

    private static void ThreeStonesOpenOnlySouthGate()
    {
        GameSession session = GateOpenSession();
        IReadOnlyList<HexCoordinate> path = session.Map.FindPath(new HexCoordinate(0, 0), HexMap.EyeTower);
        HashSet<HexCoordinate> allowed = [HexMap.PurpleGate, .. HexMap.PurpleCorridor, HexMap.EyeTower];
        Assert(path.Count > 0 && path.Contains(HexMap.PurpleGate), "Üç taştan sonra kuleye yalnız güney kapısından ulaşılmalıdır.");
        Assert(path.Where(x => session.Map.Get(x)!.Biome == Biome.Corrupted).All(allowed.Contains), "Rota mor bölgede yalnız kapı yolunu kullanmalıdır.");
        HexTile[] sealedBorder = session.Map.Tiles.Values
            .Where(x => x.Biome == Biome.Corrupted && !x.IsPermanentObstacle && !allowed.Contains(x.Coordinate)).ToArray();
        Assert(sealedBorder.Length >= 4 && sealedBorder.All(x => x.IsLocked && x.IsSealed), "Diğer mor sınırlar geçilmez kalmalıdır.");
        Assert(allowed.All(x => !session.Map.Get(x)!.IsDangerous), "Kapı yolunda rastgele savaş yerine sabit olaylar olmalıdır.");
        GameSession loaded = GameSession.FromSaveData(session.ToSaveData());
        Assert(!loaded.Map.Get(HexMap.PurpleGate)!.IsLocked && loaded.Story.PurpleGateOpen, "Mor kapı kayıttan sonra açık kalmalıdır.");
    }

    private static void FailedChecksNeverLockMainStory()
    {
        GameSession session = PrologueDoneSession();
        StoryEvent? alternative = Play(session, "redburg-archive", "persuade", 1);
        var mainOne = (MainQuest)session.Story.Quest(StoryCatalog.MainOne);
        Assert(alternative?.Id == "archive-alternative" && mainOne.Status == QuestStatus.Active && mainOne.UsingAlternative &&
               mainOne.ActiveObjective?.Id == "archive-alternative", "Arşiv başarısızlığı alternatif hedef açmalıdır.");
        Play(session, alternative!.Id, "plaque", 1);
        Assert(mainOne.Status == QuestStatus.Completed && session.Story.IsActive(StoryCatalog.RootQuest) && session.Player.IsAlive,
            "Alternatif kaynak başarısız olsa bile bölüm bitmeli ve taş görevleri açılmalıdır.");

        Play(session, "waystone-root-activation", "seize", 1);
        Assert(session.Story.Quest(StoryCatalog.RootQuest).Status == QuestStatus.Completed, "Başarısız taş kontrolü taşı kilitlememelidir.");

        StoryEvent fight = Play(session, "waystone-sky-activation", "seize")!;
        ((CombatStoryEvent)fight).Flee(session);
        Assert(StoryDirector.EventOnStep(session, StoryCatalog.SkySite, routeEnd: true)?.Id == "waystone-sky-activation",
            "Muhafız savaşından kaçış, taş seçeneklerini yeniden açmalıdır.");

        session.Player.ReceiveDamage(session.Player.Health - 1);
        new CostConsequence(health: -6).Apply(session);
        Assert(session.Player.Health == 1, "Hikâye bedeli oyuncuyu öldürmemelidir.");
    }

    private static void EyeBishopRunsOnce()
    {
        GameSession session = GateOpenSession();
        Assert(StoryDirector.EventOnStep(session, HexMap.PurpleGate, routeEnd: false)?.Id == "corrupted-refugees",
            "Kapıdan geçerken mülteci olayı oynamalıdır.");
        Play(session, "corrupted-refugees", "remember", 20);
        Play(session, "erased-patrol", "oath", 20);
        StoryEvent? bishop = Play(session, "memory-echo", "hold", 20);
        Assert(bishop?.Id == "eye-bishop-confrontation" && bishop.Scene.FrameMeaning == "eye-bishop",
            "Anı yankısından sonra Göz Piskoposu gelmeli ve frameMeanings karesini kullanmalıdır.");
        Assert(Play(session, bishop!.Id, "persuade", 20) is null, "İkna edilen Piskopos savaş açmamalıdır.");
        Assert(session.Story.Quest(StoryCatalog.BishopQuest).Status == QuestStatus.Completed, "Piskopos sonucu kaydedilmelidir.");
        Assert(StoryDirector.EventOnStep(session, StoryCatalog.BridgeSite, routeEnd: true) is null, "Piskopos olayı tekrar oynamamalıdır.");
        GameSession loaded = GameSession.FromSaveData(session.ToSaveData());
        Assert(StoryDirector.EventOnStep(loaded, StoryCatalog.BridgeSite, routeEnd: true) is null, "Kayıttan sonra da tekrar etmemelidir.");
    }

    private static void FirstVaelorFightIsScriptedDefeat()
    {
        GameSession session = TowerSession();
        var battle = (CombatStoryEvent)StoryCatalog.Find("vaelor-first-fight")!;
        Assert(battle.CanTrigger(session) && battle.FateStrikeEventId == "tower-fall" && !battle.FleeAllowed,
            "İlk Vaelor savaşı kaçışsız scripted savaş olmalıdır.");
        EnemyNpc vaelor = battle.CreateEnemy(session);
        var combat = new CombatService(new FixedDiceRoller(15, 3));
        combat.Attack(session.Player, vaelor);
        Assert(vaelor.Health < vaelor.MaxHealth, "Oyuncu en az bir gerçek eylem yapabilmelidir.");
        session.Player.DefenseBonus = 30;
        EnemyTurnResult result = battle.EnemyTurn(session, vaelor, combat);
        Assert(result.Kind == EnemyTurnKind.ScriptedDefeat && !session.Player.IsAlive && session.Story.ScriptedDefeatPlayed,
            "Kaderin Hükmü AC'yi yok sayıp scripted yenilgi üretmelidir.");
        Assert(vaelor.IsAlive && session.Story.VaelorState == VaelorState.FirstVictory, "İlk savaşta Vaelor ölmemelidir.");
    }

    private static void ScriptedDefeatDoesNotOpenDeathScreen()
    {
        GameSession session = TowerSession();
        var battle = (CombatStoryEvent)StoryCatalog.Find("vaelor-first-fight")!;
        EnemyTurnResult scripted = battle.EnemyTurn(session, battle.CreateEnemy(session), new CombatService(new FixedDiceRoller()));
        Assert(!scripted.OpensDeathScreen, "Scripted yenilgi ölüm ekranı açmamalıdır.");
        Assert(StoryDirector.Finish(session, battle) is ScriptedDefeatStoryEvent { Id: "tower-fall" } && !battle.SaveAfter,
            "Scripted yenilgiden sonra kuleden düşüş oynamalı ve can 0 iken kayıt alınmamalıdır.");

        GameSession normal = GateOpenSession();
        var bishop = (CombatStoryEvent)StoryCatalog.Find("eye-bishop-fight")!;
        normal.Player.ReceiveDamage(normal.Player.Health - 1);
        EnemyTurnResult real = bishop.EnemyTurn(normal, bishop.CreateEnemy(normal), new CombatService(new FixedDiceRoller(20, 6)));
        Assert(real.Kind == EnemyTurnKind.PlayerDown && real.OpensDeathScreen, "Gerçek savaştaki yenilgi normal ölüm sayılmalıdır.");
    }

    private static void RiderRevivalRestoresHealthAndGrantsArmor()
    {
        GameSession session = RevivedSession();
        Assert(session.Player.Health == session.Player.MaxHealth, "Süvari oyuncuyu tam canla diriltmelidir.");
        Assert(session.Player.Equipment.Get(EquipmentSlot.StoryLayer) is FatebreakerArmor && session.Story.FatebreakerGranted,
            "Kaderbozan Zırhı verilmeli ve hikâye katmanında kuşanılmalıdır.");
        Assert(session.CurrentHex == StoryCatalog.BridgeSite && session.Story.IsActive(StoryCatalog.ReturnQuest),
            "Oyuncu kulenin dibinde uyanmalı ve dönüş görevi başlamalıdır.");
        StoryEvent rider = StoryCatalog.Find("rider-revival")!;
        Assert(rider.Pages.Contains(StoryCatalog.RiderLine) && rider.Scene.SequenceId == "rider-revival-v1",
            "Süvari yalnız istenen kısa mesajı söylemelidir.");
        foreach (StoryEvent other in StoryCatalog.All.Where(x => x != rider))
        {
            string text = string.Join(" ", other.Pages.Concat(other.AfterPages).Concat(other.Choices.Select(x => x.Label + x.SuccessText)));
            Assert(!text.Contains("Süvari", StringComparison.OrdinalIgnoreCase) && !text.Contains("süvari"),
                $"Kalpaklı Süvari {other.Id} olayında görünmemelidir.");
        }
        Assert(StoryDirector.ResumeEvent(session) is null && StoryCatalog.Find("tower-fall")!.CanTrigger(session) == false,
            "Diriltmeden sonra düşüş tekrar oynamamalıdır.");
    }

    private static void FatebreakerKeepsChestArmor()
    {
        PlayerCharacter player = Player("Zırhlı");
        var chain = new Armor("Zincir Gömlek", 30, 10, 2, EquipmentSlot.Torso);
        player.Inventory.Add(chain);
        player.Equipment.Equip(chain, EquipmentSlot.Torso);
        int before = player.ArmorClass;
        var session = new GameSession(player);
        new ItemConsequence(() => new FatebreakerArmor(), equip: true).Apply(session);
        Assert(player.Equipment.Get(EquipmentSlot.Torso) == chain && player.Equipment.Get(EquipmentSlot.StoryLayer) is FatebreakerArmor,
            "Kaderbozan göğüs zırhını çıkarmamalıdır.");
        Assert(player.ArmorClass == before + 1, "Kaderbozan normal düşmanlara karşı yalnız +1 AC vermelidir.");

        string directory = Path.Combine(Path.GetTempPath(), "retro-rpg-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            var saves = new SaveGameService(directory);
            saves.Save(session.ToSaveData());
            PlayerCharacter loaded = GameSession.FromSaveData(saves.Load()).Player;
            Assert(loaded.Equipment.Get(EquipmentSlot.Torso) is Armor { Name: "Zincir Gömlek" } &&
                   loaded.Equipment.Get(EquipmentSlot.StoryLayer) is FatebreakerArmor { LastStandCharges: 1 },
                "Kayıt iki zırhı da korumalıdır.");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        foreach (CharacterRace race in RaceCatalog.All)
        {
            var wearer = new PlayerCharacter("Yolcu", Gender.Male, race, new ChampionClass(), BackgroundCatalog.Find("soldier-family"), new CharacterStats());
            var armor = new FatebreakerArmor();
            Assert(wearer.Equipment.CanEquip(armor, armor.Slot), $"{race.DisplayName} Kaderbozan Zırhı'nı kuşanabilmelidir.");
        }
        Assert(!new ShopService().TrySell(player, player.Inventory.Find<FatebreakerArmor>()!, out _), "Kaderbozan satılamamalıdır.");
    }

    private static void FatebreakerBlocksSecondFateStrike()
    {
        GameSession session = RevivedSession();
        session.Player.Equipment.Unequip(EquipmentSlot.StoryLayer);
        StoryEvent? back = StoryDirector.EventOnStep(session, HexMap.EyeTower, routeEnd: false);
        Assert(back is ScriptedDefeatStoryEvent { Id: "fatebreaker-return" } && back.Scene.SequenceId == "fatebreaker-return-v1",
            "Kuleye dönüş zırhlı dönüş sahnesini açmalıdır.");
        int health = session.Player.Health;
        StoryOutcome outcome = back!.Resolve(session, back.Choices[0], null);
        Assert(outcome.Success && session.Player.Health == health && session.Player.Equipment.Get(EquipmentSlot.StoryLayer) is FatebreakerArmor,
            "Kaderbozan Zırhı ikinci Kaderin Hükmü'nü kırmalıdır.");

        session.Player.ReceiveDamage(session.Player.Health - 3);
        session.Player.ReceiveDamage(50);
        Assert(session.Player.Health == 1, "Zırh ölümcül darbeyi bir kez 1 CAN'da durdurmalıdır.");
        session.Player.ReceiveDamage(50);
        Assert(!session.Player.IsAlive, "Son direniş yalnız bir kez çalışmalıdır.");
    }

    private static void SecondVaelorFightIsWinnableWithNormalDeath()
    {
        GameSession session = FinalFightSession();
        var battle = (CombatStoryEvent)StoryDirector.ResumeEvent(session)!;
        Assert(battle.Id == "vaelor-final-fight" && !battle.FleeAllowed && battle.EnemyRemainsOnDefeat,
            "Gerçek final savaşı kaldığı yerden açılmalıdır.");
        EnemyNpc vaelor = battle.CreateEnemy(session);
        var combat = new CombatService(new FixedDiceRoller(20, 4, 20, 4, 20, 4));
        for (int i = 0; i < 3 && vaelor.IsAlive; i++)
            combat.Attack(session.Player, vaelor, battle.PlayerAttackBonus!(session), session.Player.DamageDie, battle.PlayerDamageBonus!(session));
        Assert(!vaelor.IsAlive, "İkinci Vaelor savaşı normal saldırılarla kazanılabilmelidir.");
        StoryEvent? next = WinBattle(session, battle);
        Assert(next is EndingStoryEvent && session.Story.VaelorState == VaelorState.Defeated && !session.Story.IsFinished,
            "Vaelor sıfır cana inince oyun bitmemeli, final kararı açılmalıdır.");
        Assert(session.Story.UnlockedEndings.Contains(EndingRules.Restore) && session.Story.UnlockedEndings.Contains(EndingRules.Destroy),
            "Genel finaller açılmalıdır.");

        GameSession losing = FinalFightSession();
        ((FatebreakerArmor)losing.Player.Equipment.Get(EquipmentSlot.StoryLayer)!).TryUseLastStand();
        losing.Player.ReceiveDamage(losing.Player.Health - 1);
        var finalBattle = (CombatStoryEvent)StoryCatalog.Find("vaelor-final-fight")!;
        EnemyTurnResult death = finalBattle.EnemyTurn(losing, finalBattle.CreateEnemy(losing), new CombatService(new FixedDiceRoller(20, 6)));
        Assert(death.OpensDeathScreen && losing.Story.ActiveNodeId == "vaelor-final-fight",
            "Gerçek savaşta ölüm ölüm ekranını açmalı ve autosave final savaşından devam etmelidir.");
    }

    private static void OldSavesLoadIntoVersionThree()
    {
        string directory = Path.Combine(Path.GetTempPath(), "retro-rpg-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            var saves = new SaveGameService(directory);
            GameSaveData current = new GameSession(Player("Eski")).ToSaveData();
            foreach (int version in new[] { 1, 2 })
            {
                GameSaveData old = current with
                {
                    Version = version,
                    Character = version == 1 ? current.Character with { Items = null } : current.Character
                };
                JsonObject json = JsonSerializer.SerializeToNode(old, JsonOptions.Default)!.AsObject();
                json.Remove("Story");
                if (version == 1) json["Character"]!.AsObject().Remove("Items");
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, "slot-1.json"), json.ToJsonString());

                GameSession loaded = GameSession.FromSaveData(saves.Load());
                Assert(loaded.Story.CompletedEvents.Count == 0 && loaded.Story.Quests.Values.All(x => x.Status == QuestStatus.NotStarted),
                    $"Sürüm {version} kaydı güvenli hikâye varsayılanlarıyla açılmalıdır.");
                Assert(StoryDirector.ResumeEvent(loaded)?.Id == StoryCatalog.PrologueId, $"Sürüm {version} kaydı prologdan başlamalıdır.");
                Assert(loaded.ToSaveData() is { Version: 3, Story: not null }, "Yeni kayıt sürüm 3 olmalıdır.");
                Assert(loaded.Player.Equipment.Get(EquipmentSlot.MainHand) is Weapon, $"Sürüm {version} kaydı silahı korumalı veya başlangıç paketi almalıdır.");
            }
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    private static void SaveLoadDoesNotRepeatOneShotEvents()
    {
        string directory = Path.Combine(Path.GetTempPath(), "retro-rpg-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            var saves = new SaveGameService(directory);
            GameSession revived = RevivedSession();
            saves.Save(revived.ToSaveData());
            GameSession loaded = GameSession.FromSaveData(saves.Load());
            Assert(loaded.Story.PrologueCompleted && StoryDirector.ResumeEvent(loaded) is null, "Prolog kayıttan sonra tekrar açılmamalıdır.");
            Assert(StoryDirector.EventOnStep(loaded, HexMap.PurpleGate, routeEnd: true) is null &&
                   StoryDirector.EventOnStep(loaded, StoryCatalog.BridgeSite, routeEnd: true) is null,
                "Mor bölge olayları kayıttan sonra tekrar etmemelidir.");
            Assert(StoryDirector.EventOnStep(loaded, HexMap.EyeTower, routeEnd: true)?.Id == "fatebreaker-return" &&
                   loaded.Story.ScriptedDefeatPlayed, "Scripted yenilgi kayıttan sonra tekrar oynamamalıdır.");
            Assert(loaded.Story.Remembered == revived.Story.Remembered && loaded.Story.TrustIn("swamp") == revived.Story.TrustIn("swamp") &&
                   loaded.Story.Quests.Values.All(x => x.Status == revived.Story.Quest(x.Id).Status) &&
                   loaded.Story.WaystoneMode("root") == revived.Story.WaystoneMode("root"),
                "Görev, güven, karar ve hatırlanan kişi kayıtta korunmalıdır.");

            GameSession beforeFinal = FinalFightSession();
            saves.Save(beforeFinal.ToSaveData());
            Assert(StoryDirector.ResumeEvent(GameSession.FromSaveData(saves.Load()))?.Id == "vaelor-final-fight",
                "Final savaşı öncesi autosave doğrudan savaşa dönmelidir.");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    private static void AllFourEndingsAreReachable()
    {
        (string Ending, VaelorState Fate)[] cases =
        [
            (EndingRules.Restore, VaelorState.Killed),
            (EndingRules.Share, VaelorState.Persuaded),
            (EndingRules.Destroy, VaelorState.Imprisoned),
            (EndingRules.Watcher, VaelorState.Killed)
        ];
        foreach ((string endingId, VaelorState fate) in cases)
        {
            GameSession session = DefeatedVaelorSession(compromise: true);
            EndingStoryEvent ending = StoryCatalog.Endings.Single(x => x.EndingId == endingId);
            Assert(session.Story.UnlockedEndings.Contains(endingId), $"{endingId} uygun duruma açılmalıdır.");
            StoryOutcome outcome = StoryDirector.ChooseEnding(session, fate, ending);
            Assert(session.Story.ChosenEnding == endingId && session.Story.IsFinished && session.Story.VaelorState == fate &&
                   outcome.Text.Length > 40 && session.Story.Quest(StoryCatalog.FinalChoiceQuest).Status == QuestStatus.Completed,
                $"{endingId} finali seçilebilmeli ve epilog üretmelidir.");
            Assert(StoryDirector.ResumeEvent(session) is null, "Biten hikâye kayıttan sonra yeniden başlamamalıdır.");
        }
        GameSession watcher = DefeatedVaelorSession(compromise: false);
        StoryDirector.ChooseEnding(watcher, VaelorState.Killed, StoryCatalog.Endings.Single(x => x.EndingId == EndingRules.Watcher));
        Assert(EndingRules.Epilogue(watcher, EndingRules.Watcher).Contains("Gözcü") || EndingRules.Epilogue(watcher, EndingRules.Watcher).Contains("hükümdar"),
            "Yeni Gözcü epilogu sınıf, amaç ve kararlardan ton almalıdır.");
    }

    private static void ShareEndingRequiresCompromiseAndTrust()
    {
        GameSession session = DefeatedVaelorSession(compromise: false);
        EndingStoryEvent share = StoryCatalog.Endings.Single(x => x.EndingId == EndingRules.Share);
        Assert(!share.IsUnlocked(session.Story) && !session.Story.UnlockedEndings.Contains(EndingRules.Share),
            "Zorla alınan taşlarla paylaştırma finali açılmamalıdır.");
        Assert(Throws(() => StoryDirector.ChooseEnding(session, VaelorState.Killed, share)), "Kilitli final seçilememelidir.");
        session.Story.AddTrust("swamp", 5);
        session.Story.AddTrust("desert", 5);
        Assert(!share.IsUnlocked(session.Story), "Güven tek başına yetmemeli; uzlaşmacı taş kararları da gerekir.");
        Assert(!EndingRules.CanPersuadeVaelor(session.Story) || session.Story.PrisonerRescued,
            "Vaelor'u ikna etmek tanık veya uzlaşma gerektirmelidir.");
        Assert(DefeatedVaelorSession(compromise: true).Story.UnlockedEndings.Contains(EndingRules.Share),
            "Uzlaşma ve yüksek güvenle paylaştırma finali açılmalıdır.");
    }

    private static void CinematicFramesResolveToRealFiles()
    {
        string root = Environment.CurrentDirectory;
        CinematicLibrary library = CinematicLibrary.Load(root);
        string[] expected = ["caravan-prologue-v1", "root-waystone-v1", "sky-waystone-v1", "sun-waystone-v1", "archive-gate-v1",
            "corrupted-region-v1", "fate-lens-v1", "endings-v1", "tower-fall-v1", "rider-revival-v1", "fatebreaker-return-v1"];
        Assert(expected.All(x => library.Sequences.Any(s => s.Id == x)) && library.Sequences.Sum(x => x.Frames.Count) == 46,
            "İki manifest 11 dizi ve 46 kare vermelidir.");
        foreach (string frame in library.Sequences.SelectMany(x => x.Frames))
        {
            string path = Path.Combine(root, frame);
            Assert(frame.EndsWith(".png") && File.Exists(path), $"Sinematik kare yok: {frame}");
            Assert(PngSize(path) == (800, 800), $"Sinematik kare 800x800 değil: {frame}");
        }
        foreach (StoryEvent story in StoryCatalog.All)
        {
            IReadOnlyList<string> frames = library.FramesFor(story.Scene);
            Assert(frames.Count > 0 && frames.All(x => File.Exists(Path.Combine(root, x))), $"{story.Id} sahnesi çözülemedi.");
            Assert(story.Scene.ChoiceFrame < frames.Count && story.Pages.Count <= story.Scene.ChoiceFrame + 1 &&
                   story.AfterPages.Count <= frames.Count - 1 - story.Scene.ChoiceFrame,
                $"{story.Id} sayfa ve kare sayısı uyuşmuyor; fazla sayfa ekranda hiç görünmez.");
            if (story is CombatStoryEvent battle)
                Assert(File.Exists(Path.Combine(root, battle.EnemySprite)) && File.Exists(Path.Combine(root, battle.EnemyAttackSprite)),
                    $"{story.Id} düşman görseli yok.");
        }
        string[] finalStoryAssets =
        [
            "items/item-pack-01/icons/armor/25-fatebreaker-armor-v1.png",
            "assets/final-production-v6/equipment/wearables/story/fatebreaker/south/fatebreaker-story-layer-south-idle-v1.png"
        ];
        Assert(finalStoryAssets.All(x => File.Exists(Path.Combine(root, x)) && PngSize(Path.Combine(root, x)) == (256, 256)),
            "Kaderbozan final ikon veya ekipman katmanı eksik ya da 256x256 değil.");
        for (int i = 0; i < StoryCatalog.Endings.Count; i++)
            Assert(library.FramesFor(StoryCatalog.Endings[i].Scene)[0].EndsWith($"endings-frame-0{i}-v1.png"),
                $"{StoryCatalog.Endings[i].Title} doğru final karesini kullanmalıdır.");
    }

    private static void SkyAwakeningPlaysOnlyAfterGuardianVictory()
    {
        GameSession session = ChapterTwoSession();
        StoryEvent sky = StoryCatalog.Find("waystone-sky-activation")!;
        StoryEvent awakening = StoryCatalog.Find("sky-waystone-seized")!;
        Assert(!sky.Choices.Single(x => x.Id == "seize").PlaysAfterFrames && sky.Choices.Where(x => x.Id != "seize").All(x => x.PlaysAfterFrames),
            "Yalnız muhafızla savaş seçimi uyanış karelerini savaştan önce oynatmamalıdır.");

        StoryEvent? fight = Play(session, sky.Id, "seize");
        Assert(fight is CombatStoryEvent { Id: "sky-guardian-fight" } && session.Story.WaystoneMode("sky") is null &&
               !HasQuestItem(session, "story:waystone-sky") && !awakening.CanTrigger(session),
            "Savaş seçimi taşı uyandırmamalı; uyanış savaştan önce tetiklenmemelidir.");

        ((CombatStoryEvent)fight!).Flee(session);
        Assert(!awakening.CanTrigger(session) && session.Story.IsActive(StoryCatalog.SkyQuest) &&
               StoryDirector.EventOnStep(session, StoryCatalog.SkySite, routeEnd: true) == sky,
            "Kaçışta uyanış oynamamalı ve taş seçenekleri yeniden açılmalıdır.");

        StoryEvent? next = WinBattle(session, (CombatStoryEvent)Play(session, sky.Id, "seize")!);
        Assert(next == awakening && awakening.Scene is { SequenceId: "sky-waystone-v1", FirstFrame: 1, FrameCount: 3 } &&
               awakening.Pages.SequenceEqual(sky.AfterPages) && session.Story.WaystoneMode("sky") == "seize",
            "Uyanış kareleri ve metinleri muhafız yenildikten sonra oynamalıdır.");
        Play(session, awakening.Id, "continue");
        Assert(!awakening.CanTrigger(session) && StoryDirector.EventOnStep(session, StoryCatalog.SkySite, routeEnd: true) is null,
            "Uyanış bir kez oynamalı ve Gök Taşı tekrar açılmamalıdır.");
    }

    private static void RefugeeTrustDoesNotUnlockShareEnding()
    {
        Assert(EndingRules.TrustRegions.SequenceEqual(["swamp", "mountain", "desert"]), "Sayılan güven bölgeleri yalnız taş bölgeleri olmalıdır.");
        GameSession session = ChapterTwoSession();
        Play(session, "waystone-root-activation", "cleanse", 20);
        Play(session, "waystone-sun-activation", "share", 1);
        Play(session, "broken-archive-message", "continue");
        WinBattle(session, (CombatStoryEvent)Play(session, "waystone-sky-activation", "seize")!);
        Play(session, "sky-waystone-seized", "continue");
        Play(session, "purple-gate-opening", "continue");
        Play(session, "corrupted-refugees", "remember", 20);
        StoryProgress story = session.Story;
        Assert(story.TrustIn("refugees") >= EndingRules.HighTrust && story.TrustIn("swamp") >= EndingRules.HighTrust &&
               story.TrustIn("desert") < EndingRules.HighTrust && story.TrustIn("mountain") < EndingRules.HighTrust &&
               EndingRules.CompromiseCount(story) == 2, "Test durumu: iki uzlaşma kararı, mülteciler ve tek taş bölgesinde güven.");

        EndingConsequence.UnlockAvailable().Apply(session);
        Assert(EndingRules.HighTrustRegions(story) == 1 && !EndingRules.CanShare(story) && !story.UnlockedEndings.Contains(EndingRules.Share),
            "Mülteci güveni bölge sayılmamalı; mülteciler + tek bölge paylaştırma finalini açmamalıdır.");
        story.AddTrust("greenford", 5);
        Assert(!EndingRules.CanShare(story), "Yeşilgeçit güveni de taş bölgesi sayılmamalıdır.");
        story.AddTrust("desert", 1);
        EndingConsequence.UnlockAvailable().Apply(session);
        Assert(EndingRules.CanShare(story) && story.UnlockedEndings.Contains(EndingRules.Share),
            "İkinci taş bölgesi yüksek güvene ulaşınca paylaştırma açılmalıdır.");
    }

    // Prologdan finale kadar her olayı tek oturumda oynatır. Hedeflere haritadaki gibi BFS rotasıyla
    // adım adım yürünür ve olaylar aynı tetik kurallarıyla (han, ÇEVREYİ ARA, geçiş, varış, zincir) açılır.
    private static void FullStoryPlaysFromPrologueToEnding()
    {
        GameSession session = StorySession();
        var played = new List<string>();
        var chapters = new List<int> { session.Story.Chapter };
        var plan = new Dictionary<string, (string Choice, int Roll)>
        {
            [StoryCatalog.PrologueId] = ("save-survivor", 20),
            ["greenford-witness"] = ("survivor", 0),
            ["caravan-wreck-search"] = ("search", 20),
            ["redburg-archive"] = ("persuade", 1),
            ["archive-alternative"] = ("officer", 20),
            ["waystone-root-activation"] = ("cleanse", 20),
            ["waystone-sun-activation"] = ("share", 20),
            ["waystone-sky-activation"] = ("seize", 0),
            ["corrupted-refugees"] = ("remember", 20),
            ["erased-patrol"] = ("oath", 20),
            ["memory-echo"] = ("hold", 20),
            ["eye-bishop-confrontation"] = ("fight", 0),
            ["tower-prison"] = ("name", 20),
            ["fate-lens-chamber"] = ("share", 0),
            ["rider-revival"] = ("armor", 0)
        };

        // Bir olayı ve ardından gelen zinciri oynatır; finale gelirse final olayını döndürür.
        EndingStoryEvent? RunChain(StoryEvent? story)
        {
            while (story is not null and not EndingStoryEvent)
            {
                Assert(story.CanTrigger(session), $"{story.Id} sırası geldiğinde tetiklenemedi.");
                StoryDirector.Begin(session, story);
                played.Add(story.Id);
                if (story is CombatStoryEvent battle) Fight(session, battle);
                else
                {
                    (string choiceId, int roll) = plan.GetValueOrDefault(story.Id, (story.Choices[0].Id, 20));
                    StoryChoice choice = story.Choices.Single(x => x.Id == choiceId);
                    Assert(choice.IsAvailable(session), $"{story.Id} için {choiceId} seçilemiyor.");
                    D20Roll? d20 = choice.Check is StoryCheck check
                        ? new FixedDiceRoller(roll).RollD20(SkillCheckStoryEvent.CheckBonus(session, check))
                        : null;
                    story.Resolve(session, choice, d20);
                }
                story = StoryDirector.Finish(session, story);
                chapters.Add(session.Story.Chapter);
            }
            return story as EndingStoryEvent;
        }

        RunChain(StoryDirector.ResumeEvent(session));
        Assert(session.CurrentHex == StoryCatalog.Greenford, "Prolog Yeşilgeçit'te bitmelidir.");
        RunChain(StoryDirector.EventAtInn(session, "greenford"));
        Assert(Travel(session, StoryCatalog.CaravanWreck) is null, "Kervan enkazı geçişte değil ÇEVREYİ ARA ile açılmalıdır.");
        RunChain(StoryDirector.EventOnSearch(session, StoryCatalog.CaravanWreck));
        RunChain(Travel(session, StoryCatalog.Redburg));
        RunChain(Travel(session, StoryCatalog.RootSite));
        RunChain(Travel(session, StoryCatalog.SunSite));
        RunChain(Travel(session, StoryCatalog.SkySite));
        Assert(session.Story.PurpleGateOpen, "Üç taştan sonra mor kapı açık olmalıdır.");

        EndingStoryEvent? ending = null;
        for (int leg = 0; leg < 8 && ending is null; leg++)
        {
            StoryEvent? found = Travel(session, HexMap.EyeTower);
            Assert(found is not null, "Kuleye giden yolda sıradaki hikâye olayı bulunamadı.");
            ending = RunChain(found);
        }
        Assert(ending is not null, "Hikâye final kararına ulaşmadı.");
        StoryOutcome finale = StoryDirector.ChooseEnding(session, VaelorState.Persuaded,
            StoryCatalog.Endings.Single(x => x.EndingId == EndingRules.Share));
        chapters.Add(session.Story.Chapter);

        string[] expected =
        [
            StoryCatalog.PrologueId, "greenford-witness", "caravan-wreck-search", "redburg-archive", "archive-alternative",
            "waystone-root-activation", "waystone-sun-activation", "broken-archive-message", "waystone-sky-activation",
            "sky-guardian-fight", "sky-waystone-seized", "purple-gate-opening", "corrupted-refugees", "erased-patrol",
            "memory-echo", "eye-bishop-confrontation", "eye-bishop-fight", "tower-gate-rest", "tower-archive", "tower-prison",
            "fate-lens-chamber", "vaelor-first-fight", "tower-fall", "rider-revival", "fatebreaker-return",
            "vaelor-second-form", "vaelor-final-fight"
        ];
        Assert(played.SequenceEqual(expected), $"Olay sırası beklenenden farklı: {string.Join(" > ", played)}");
        Assert(StoryCatalog.All.Where(x => x is not EndingStoryEvent).All(x => played.Contains(x.Id)),
            "Finaller dışındaki bütün hikâye olayları oynanmalıdır.");
        Assert(chapters[0] == 0 && chapters[^1] == 5 && chapters.Zip(chapters.Skip(1)).All(x => x.First <= x.Second),
            "Bölümler 0'dan 5'e geri gitmeden ilerlemelidir.");
        Assert(session.Story.Quests.Values.All(x => x.Status == QuestStatus.Completed), "Bütün görevler tamamlanmalıdır.");
        Assert(session.Story.IsFinished && session.Story.ChosenEnding == EndingRules.Share && session.Story.VaelorState == VaelorState.Persuaded &&
               finale.Text.Length > 40, "Paylaştırma finali seçilmeli ve epilog üretmelidir.");
        Assert(session.Player.IsAlive && session.Player.Equipment.Get(EquipmentSlot.StoryLayer) is FatebreakerArmor &&
               new[] { "story:black-map", "story:waystone-root", "story:waystone-sky", "story:waystone-sun" }.All(x => HasQuestItem(session, x)) &&
               !HasQuestItem(session, "story:frontier-map"), "Görev eşyaları ve Kaderbozan Zırhı envanterde olmalıdır.");

        GameSession loaded = GameSession.FromSaveData(JsonRoundTrip(session.ToSaveData()));
        Assert(StoryDirector.ResumeEvent(loaded) is null && loaded.Story.ChosenEnding == EndingRules.Share &&
               loaded.Story.CompletedEvents.Count == session.Story.CompletedEvents.Count,
            "Biten hikâye kayıttan aynen yüklenmeli ve yeniden başlamamalıdır.");
    }

    // ------------------------------------------------------------------ Hikâye test yardımcıları

    // Haritadaki gibi yürür: BFS rotası, adım adım TryMove ve her adımda hikâye tetik kontrolü.
    private static StoryEvent? Travel(GameSession session, HexCoordinate target)
    {
        IReadOnlyList<HexCoordinate> path = session.Map.FindPath(session.CurrentHex, target);
        Assert(path.Count > 1, $"{session.CurrentHex} konumundan {target} hedefine yürüyüş rotası yok.");
        for (int i = 1; i < path.Count; i++)
        {
            Assert(session.TryMove(path[i]), $"{path[i]} hexine yürünemedi.");
            if (StoryDirector.EventOnStep(session, path[i], i == path.Count - 1) is StoryEvent story) return story;
        }
        return null;
    }

    // Sabit zarla hikâye savaşı: oyuncu her tur doğal 20 atar, düşman doğal 1 ile ıskalar.
    // Scripted savaşta düşman sırası Kaderin Hükmü'dür ve savaş orada biter.
    private static void Fight(GameSession session, CombatStoryEvent battle)
    {
        EnemyNpc enemy = battle.CreateEnemy(session);
        for (int round = 0; round < 30; round++)
        {
            int die = session.Player.DamageDie;
            new CombatService(new FixedDiceRoller(20, die)).Attack(session.Player, enemy,
                battle.PlayerAttackBonus?.Invoke(session) ?? 0, die, battle.PlayerDamageBonus?.Invoke(session) ?? 0);
            if (!enemy.IsAlive)
            {
                battle.Victory(session);
                return;
            }
            EnemyTurnResult turn = battle.EnemyTurn(session, enemy, new CombatService(new FixedDiceRoller(1)));
            if (turn.Kind == EnemyTurnKind.ScriptedDefeat) return;
            Assert(!turn.OpensDeathScreen, $"{battle.Id} sırasında oyuncu öldü.");
        }
        throw new InvalidOperationException($"{battle.Id} 30 turda bitmedi.");
    }

    private static GameSaveData JsonRoundTrip(GameSaveData data) =>
        JsonSerializer.Deserialize<GameSaveData>(JsonSerializer.Serialize(data, JsonOptions.Default), JsonOptions.Default)!;

    // Ekranın yaptığını aynen yapar: başlat, (gerekirse sabit zarla) çöz, bitir ve sıradaki olayı döndür.
    private static StoryEvent? Play(GameSession session, string eventId, string choiceId, int roll = 0, bool requireTrigger = true)
    {
        StoryEvent story = StoryCatalog.Find(eventId) ?? throw new InvalidOperationException($"Olay yok: {eventId}");
        if (requireTrigger) Assert(story.CanTrigger(session), $"{eventId} şu anda tetiklenemiyor.");
        else if (!story.CanTrigger(session)) return null;
        StoryDirector.Begin(session, story);
        StoryChoice choice = story.Choices.First(x => x.Id == choiceId);
        D20Roll? d20 = choice.Check is StoryCheck check
            ? new FixedDiceRoller(roll).RollD20(SkillCheckStoryEvent.CheckBonus(session, check))
            : null;
        story.Resolve(session, choice, d20);
        return StoryDirector.Finish(session, story);
    }

    private static StoryEvent? WinBattle(GameSession session, CombatStoryEvent battle)
    {
        battle.Victory(session);
        return StoryDirector.Finish(session, battle);
    }

    private static GameSession StorySession(PlayerCharacter? player = null)
    {
        player ??= Player("Kahraman");
        StartingKitCatalog.AddStoryItem(player, StorySlotCatalog.MainStoryId);
        var session = new GameSession(player);
        StoryDirector.ResumeEvent(session);
        return session;
    }

    private static GameSession PrologueDoneSession()
    {
        GameSession session = StorySession();
        Play(session, StoryCatalog.PrologueId, "study-fragment", 20);
        return session;
    }

    private static GameSession ChapterTwoSession()
    {
        GameSession session = PrologueDoneSession();
        Play(session, "redburg-archive", "persuade", 20);
        return session;
    }

    private static GameSession GateOpenSession(bool compromise = true)
    {
        GameSession session = ChapterTwoSession();
        Play(session, "waystone-root-activation", compromise ? "cleanse" : "seize", 20);
        Play(session, "waystone-sun-activation", compromise ? "share" : "steal", 20);
        Play(session, "broken-archive-message", "continue");
        if (compromise) Play(session, "waystone-sky-activation", "protect", 20);
        else
        {
            WinBattle(session, (CombatStoryEvent)Play(session, "waystone-sky-activation", "seize")!);
            Play(session, "sky-waystone-seized", "continue");
        }
        Play(session, "purple-gate-opening", "continue");
        return session;
    }

    private static GameSession TowerSession(bool compromise = true)
    {
        GameSession session = GateOpenSession(compromise);
        Play(session, "corrupted-refugees", "remember", 20);
        Play(session, "erased-patrol", "oath", 20);
        Play(session, "memory-echo", "hold", 20);
        Play(session, "eye-bishop-confrontation", "persuade", 20);
        Play(session, "tower-gate-rest", "rest");
        Play(session, "tower-archive", "continue");
        Play(session, "tower-prison", "name", compromise ? 20 : 1);
        Play(session, "fate-lens-chamber", "share");
        return session;
    }

    private static GameSession RevivedSession(bool compromise = true)
    {
        GameSession session = TowerSession(compromise);
        var battle = (CombatStoryEvent)StoryCatalog.Find("vaelor-first-fight")!;
        battle.EnemyTurn(session, battle.CreateEnemy(session), new CombatService(new FixedDiceRoller()));
        StoryDirector.Finish(session, battle);
        Play(session, "tower-fall", "continue");
        Play(session, "rider-revival", "armor");
        return session;
    }

    private static GameSession FinalFightSession(bool compromise = true)
    {
        GameSession session = RevivedSession(compromise);
        Play(session, "fatebreaker-return", "continue");
        Play(session, "vaelor-second-form", "continue");
        return session;
    }

    private static GameSession DefeatedVaelorSession(bool compromise)
    {
        GameSession session = FinalFightSession(compromise);
        WinBattle(session, (CombatStoryEvent)StoryCatalog.Find("vaelor-final-fight")!);
        return session;
    }

    private static CompositeBackground PathWith(params string[] choiceIds)
    {
        BackgroundStageChoice[] path = BackgroundCatalog.DefaultPath().ToArray();
        foreach (string id in choiceIds)
        {
            BackgroundStageChoice choice = BackgroundCatalog.Stages.SelectMany(BackgroundCatalog.OptionsFor).First(x => x.Id == id);
            path[(int)choice.Stage] = choice;
        }
        return BackgroundCatalog.Create(path);
    }

    private static bool HasQuestItem(GameSession session, string questKey) =>
        session.Player.Inventory.Items.OfType<QuestItem>().Any(x => x.QuestKey == questKey);

    private static bool Throws(Action action)
    {
        try { action(); return false; }
        catch (InvalidOperationException) { return true; }
    }

    private static (int Width, int Height) PngSize(string path)
    {
        using FileStream stream = File.OpenRead(path);
        byte[] header = new byte[24];
        stream.ReadExactly(header);
        int Read(int offset) => header[offset] << 24 | header[offset + 1] << 16 | header[offset + 2] << 8 | header[offset + 3];
        return (Read(16), Read(20));
    }

    private static PlayerCharacter Player(string name)
    {
        var player = new PlayerCharacter(name, Gender.Female, new HumanRace(), new ChampionClass(),
            BackgroundCatalog.Find("soldier-family"), new CharacterStats());
        Weapon weapon = ItemCatalog.StartingWeapons.First(x => x.AssetId == "dagger");
        player.Inventory.Add(weapon);
        player.Equipment.Equip(weapon, EquipmentSlot.MainHand);
        return player;
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
