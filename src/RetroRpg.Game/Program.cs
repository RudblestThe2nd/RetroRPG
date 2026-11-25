using RetroRpg.Core.Characters;
using RetroRpg.Core.Combat;
using RetroRpg.Core.Common;
using RetroRpg.Core.Game;
using RetroRpg.Core.Items;
using RetroRpg.Core.Story;
using RetroRpg.Core.World;
using RetroRpg.Game.Screens;

namespace RetroRpg.Game;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (args.Length == 2 && args[0] == "--dice-qa")
        {
            Directory.CreateDirectory(args[1]);
            using var image = new Bitmap(240, 240);
            using var g = Graphics.FromImage(image);
            for (int face = 1; face <= 20; face++)
            for (int ticks = 24; ticks >= 0; ticks--)
            {
                g.Clear(Color.FromArgb(30, 23, 34));
                int visible = RollingD20.Draw(g, new(16, 16, 208, 208), face, ticks);
                if (RollingD20.Landed(ticks) && visible != face)
                    throw new InvalidOperationException($"d20: {face} beklendi, görünen yüz {visible}.");
                if (face == 17 || ticks == 0)
                    image.Save(Path.Combine(args[1], $"d20-{face:00}-{24 - ticks:00}.png"), System.Drawing.Imaging.ImageFormat.Png);
            }
            return;
        }
        if (args.Length == 2 && args[0] is "--combat-motion" or "--formation" or "--wounds" or "--dodge")
        {
            Directory.CreateDirectory(args[1]);
            using var form = new MainGameForm(Path.Combine(args[1], "qa-saves"), true) { Opacity = 0, ShowInTaskbar = false };
            form.Show();
            var player = new PlayerCharacter("Gezgin", Gender.Female, RaceCatalog.Find("elf"), ClassCatalog.FindSubclass("champion"),
                BackgroundCatalog.Create(BackgroundCatalog.DefaultPath()), new CharacterStats());
            var session = new GameSession(player);
            form.UseSessionForQa(session);
            if (args[0] == "--dodge")
            {
                form.ShowCombat(session.Map.Tiles.Values.First(x => x.IsDangerous && x.Coordinate.DistanceTo(default) <= 2), new DiceRoller(new Random(31)));
                ((CombatScreen)form.CurrentScreen!).CheckDodge(name => Capture(form, args[1], name));
            }
            if (args[0] is "--formation" or "--wounds" or "--dodge")
            {
                player.AddExperience(10000);
                var tile = session.Map.Tiles.Values.Where(x => x.IsDangerous && !x.IsLocked && x.Coordinate != HexMap.EyeTower
                    && x.Coordinate.DistanceTo(default) >= 5 && Math.Abs(x.Coordinate.Q * 31 + x.Coordinate.R * 17) % 6 is 2 or 4)
                    .OrderByDescending(x => x.Biome is Biome.Forest or Biome.Plains or Biome.Tropical).First();
                form.ShowCombat(tile, new DiceRoller(new Random(31)));
                var screen = (CombatScreen)form.CurrentScreen!;
                void SaveFrame(string name) => Capture(form, args[1], name);
                if (args[0] == "--dodge") screen.CheckDodge(SaveFrame);
                else if (args[0] == "--wounds") screen.CheckWounds(SaveFrame);
                else screen.CheckFormation(SaveFrame);
                return;
            }
            form.ShowCombat(session.Map.Tiles.Values.First(x => x.IsDangerous && x.Coordinate.DistanceTo(default) <= 2));
            ((CombatScreen)form.CurrentScreen!).CaptureWeaponMotion(name => Capture(form, args[1], name));
            return;
        }
        if (args.Length == 2 && args[0] is "--screenshots" or "--screenshots-large")
        {
            CombatAudio.SuppressPlayback = true;
            SaveScreenshots(args[1], fixedLogicalSize: args[0] == "--screenshots");
            return;
        }
        Application.Run(new MainGameForm());
    }

    // Geliştirme kontrolü: her ekranı açıp 800 x 800 PNG olarak kaydeder.
    private static void SaveScreenshots(string folder, bool fixedLogicalSize)
    {
        Directory.CreateDirectory(folder);
        using var form = new MainGameForm(Path.Combine(folder, "qa-saves"), fixedLogicalSize) { Opacity = 0, ShowInTaskbar = false };
        form.Show();

        Capture(form, folder, "01-ana-menu");
        form.ShowSettings();
        Capture(form, folder, "01b-ayarlar");
        form.ToggleFullscreen();
        Capture(form, folder, "01c-tam-ekran");
        form.ToggleFullscreen();
        form.ShowCharacterCreation();
        string[] steps =
        [
            "02-kimlik", "03-irk", "04-sinif", "05-gecmis-koken", "06-gecmis-aile",
            "07-gecmis-cocukluk", "08-gecmis-egitim", "09-gecmis-meslek",
            "10-gecmis-donum-noktasi", "11-gecmis-amac"
        ];
        foreach (string step in steps)
        {
            Capture(form, folder, step);
            if (step != steps[^1]) form.CurrentScreen!.HandleKey(Keys.Enter);
        }

        var draft = new CharacterCreationDraft("Gezgin", Gender.Female, RaceCatalog.Find("elf"),
            ClassCatalog.FindSubclass("champion"),
            BackgroundCatalog.Stages.Select(x => BackgroundCatalog.OptionsFor(x)[0]).ToArray(), 3);
        draft.Adjust(0, 1); draft.Adjust(1, 1); draft.Adjust(2, 1);
        form.ShowAttributeAllocation(draft);
        Capture(form, folder, "11b-ozellik-puanlari");
        form.ShowCharacterSummary(draft);
        Capture(form, folder, "11c-karakter-ozeti");
        form.ShowStorySelection(draft);
        Capture(form, folder, "11d-hikaye-secimi");

        var player = new PlayerCharacter("Gezgin", Gender.Female, RaceCatalog.Find("elf"), ClassCatalog.FindSubclass("champion"),
            BackgroundCatalog.Create(BackgroundCatalog.DefaultPath()), new CharacterStats());
        ItemCatalog.GiveStartingKit(player);
        var session = new GameSession(player) { PortraitIndex = 3 };
        // Harita ekranlarının QA'sı için prolog önceden tamamlanır; aksi hâlde StartGame prologu açar.
        Resolve(session, StoryCatalog.PrologueId, "follow-trail");
        session.TryMove(new HexCoordinate(-1, 0));
        form.StartGame(session);
        ((MapScreen)form.CurrentScreen!).PreviewRouteForQa(StoryCatalog.RootSite);
        Capture(form, folder, "12-harita");
        ((MapScreen)form.CurrentScreen!).ShowGridForQa();
        Capture(form, folder, "12b-harita-hex-acik");
        form.ToggleFullscreen();
        ((MapScreen)form.CurrentScreen!).CheckProvinceForQa(-1);
        Capture(form, folder, "12d-bati-tam-ekran");
        ((MapScreen)form.CurrentScreen!).CheckProvinceForQa(1);
        Capture(form, folder, "12e-dogu-tam-ekran");
        form.ToggleFullscreen();
        form.ShowMap();
        ((MapScreen)form.CurrentScreen!).OpenLocationForQa(new CastleLocation("qa-castle", "Kızılburç Kalesi"));
        Capture(form, folder, "12c-yerlesim-giris-penceresi");

        form.ShowInventory();
        Item qaLoot = LootGenerator.Create(ItemRarity.Epic, 8);
        session.Player.Inventory.Add(qaLoot);
        ((InventoryScreen)form.CurrentScreen!).SelectItemForQa(qaLoot);
        Capture(form, folder, "13-envanter");
        form.ShowCharacterRecord();
        Capture(form, folder, "14-karakter-kaydi");
        form.ShowJournal();
        Capture(form, folder, "15-gunluk");
        form.ShowCamp();
        Capture(form, folder, "16-kamp");
        form.ShowEncounter(EncounterCatalog.All[0]);
        Capture(form, folder, "17-savas-disi-encounter");
        form.ShowLocation(new TownLocation("qa-town", "Yol Kasabası"));
        Capture(form, folder, "18-kasaba");
        form.ShowShop(new TownLocation("qa-town", "Yol Kasabası"), LocationService.Market);
        Capture(form, folder, "18b-satin-alma");
        ((ShopScreen)form.CurrentScreen!).ShowSellForQa();
        Capture(form, folder, "18c-esya-satma");
        var castle = new CastleLocation("qa-castle", "Kızılburç Kalesi");
        form.ShowInn(castle);
        Capture(form, folder, "18d-han-ucretsiz-giris");
        form.ShowGuardDialogue(castle, LocationService.WarRoom);
        Capture(form, folder, "18e-muhafiz-konusmasi");

        HexTile danger = session.Map.Tiles.Values.First(x => x.IsDangerous && x.Coordinate.DistanceTo(default) <= 2);
        form.ShowCombat(danger);
        Capture(form, folder, "19-savas");
        ((CombatScreen)form.CurrentScreen!).OpenAttackForQa();
        Capture(form, folder, "20-saldiri-secimi");
        ((CombatScreen)form.CurrentScreen!).OpenPowerForQa();
        Capture(form, folder, "20a-sinif-gucu");
        ((CombatScreen)form.CurrentScreen!).ShowDiceResultForQa();
        Capture(form, folder, "20b-zar-sonucu");
        ((CombatScreen)form.CurrentScreen!).ShowImpactForQa();
        Capture(form, folder, "20c-vurus-hissi");
        ((CombatScreen)form.CurrentScreen!).ShowStatusesForQa();
        Capture(form, folder, "20d-durum-ve-niyet");
        ((CombatScreen)form.CurrentScreen!).ShowLootVictoryForQa();
        Capture(form, folder, "20e-nadir-ganimet");
        form.ShowDeath("QA karşılaşması");
        Capture(form, folder, "21-olum");

        SaveStoryScreenshots(form, folder);
        File.WriteAllLines(Path.Combine(folder, "text-overflow.log"), UiTheme.OverflowReports);
    }

    // Ana hikâyeyi prologdan finale kadar ekran ekran oynatır ve her önemli sahneyi kaydeder.
    private static void SaveStoryScreenshots(MainGameForm form, string folder)
    {
        var player = new PlayerCharacter("Gezgin", Gender.Male, RaceCatalog.Find("human"), ClassCatalog.FindSubclass("champion"),
            BackgroundCatalog.Create(BackgroundCatalog.DefaultPath()), new CharacterStats());
        ItemCatalog.GiveStartingKit(player);
        var session = new GameSession(player) { PortraitIndex = 2 };

        form.StartGame(session);
        Capture(form, folder, "22-prolog");
        Scene(form).ShowChoiceForQa();
        Capture(form, folder, "22b-prolog-secim");
        Scene(form).ShowRollForQa(0, 14);
        Capture(form, folder, "22c-prolog-zar");
        Scene(form).ShowResultForQa(0, 17);
        Capture(form, folder, "22d-prolog-sonuc");
        form.FinishStory(StoryCatalog.Find(StoryCatalog.PrologueId)!);
        Capture(form, folder, "22e-harita-yesilgecit");

        form.ShowInn(new VillageLocation("greenford", "Yeşilgeçit"));
        Capture(form, folder, "23-han-tanik");
        PlayScreen(form, session, "greenford-witness", folder, "23b-tanik-konusmasi", 0);
        PlayScreen(form, session, "caravan-wreck-search", folder, "24-kervan-enkazi", 0);
        PlayScreen(form, session, "redburg-archive", folder, "24b-kizilburc-arsivi", 0, resultName: "24c-arsiv-sonuc");
        form.ShowJournal();
        Capture(form, folder, "25-gunluk-gorevler");

        PlayScreen(form, session, "waystone-root-activation", folder, "26-kok-tasi", 0, resultName: "26b-kok-tasi-sonuc");
        Resolve(session, "waystone-sun-activation", "share");
        form.ShowStory(StoryCatalog.Find("broken-archive-message")!);
        Scene(form).ShowFrameForQa(1);
        Capture(form, folder, "27-kirik-arsiv-mesaji");
        Scene(form).ShowResultForQa(0, 0);
        form.FinishStory(StoryCatalog.Find("broken-archive-message")!);

        form.ShowStory(StoryCatalog.Find("waystone-sky-activation")!);
        Capture(form, folder, "28-gok-tasi");
        // Savaş seçimi uyanış karelerini oynatmaz; sonuç seçim karesinde kalır.
        Scene(form).ShowResultForQa(2, 0);
        Capture(form, folder, "28a-gok-tasi-savas-secimi");
        form.FinishStory(StoryCatalog.Find("waystone-sky-activation")!);
        Capture(form, folder, "28b-zirve-muhafizi-savasi");
        var guardian = (CombatStoryEvent)StoryCatalog.Find("sky-guardian-fight")!;
        guardian.Victory(session);
        form.FinishStory(guardian);
        Scene(form).ShowFrameForQa(2);
        Capture(form, folder, "28c-gok-tasi-uyanis");
        Scene(form).ShowResultForQa(0, 0);
        form.FinishStory(StoryCatalog.Find("sky-waystone-seized")!);
        Scene(form).ShowFrameForQa(1);
        Capture(form, folder, "29-mor-kapi");
        Scene(form).ShowResultForQa(0, 0);
        form.FinishStory(StoryCatalog.Find("purple-gate-opening")!);
        ((MapScreen)form.CurrentScreen!).ShowGridForQa();
        ((MapScreen)form.CurrentScreen!).PreviewRouteForQa(HexMap.EyeTower);
        Capture(form, folder, "29b-harita-mor-kapi-acik");

        PlayScreen(form, session, "corrupted-refugees", folder, "30-mor-bolge-multeciler", 0);
        PlayScreen(form, session, "erased-patrol", folder, "30b-silinmis-devriye", 0);
        PlayScreen(form, session, "memory-echo", folder, "30c-ani-yankisi", 0);
        form.ShowStory(StoryCatalog.Find("eye-bishop-confrontation")!);
        Capture(form, folder, "31-goz-piskoposu");
        Scene(form).ShowResultForQa(2, 0);
        form.FinishStory(StoryCatalog.Find("eye-bishop-confrontation")!);
        Capture(form, folder, "31b-piskopos-savasi");
        var bishop = (CombatStoryEvent)StoryCatalog.Find("eye-bishop-fight")!;
        bishop.Victory(session);
        StoryDirector.Finish(session, bishop);

        PlayScreen(form, session, "tower-gate-rest", folder, "32-kule-kapisi", 0);
        PlayScreen(form, session, "tower-archive", folder, "32b-vaelor-arsivi", 0);
        PlayScreen(form, session, "tower-prison", folder, "32c-unutulanlar-hapishanesi", 2);
        form.ShowStory(StoryCatalog.Find("fate-lens-chamber")!);
        Capture(form, folder, "32d-kader-mercegi-odasi");
        Scene(form).ShowResultForQa(0, 0);
        form.FinishStory(StoryCatalog.Find("fate-lens-chamber")!);
        Capture(form, folder, "33-vaelor-ilk-savas");
        ((CombatScreen)form.CurrentScreen!).ShowFateStrikeForQa();
        Capture(form, folder, "33b-kaderin-hukmu");

        form.FinishStory(StoryCatalog.Find("vaelor-first-fight")!);
        Scene(form).ShowFrameForQa(1);
        Capture(form, folder, "34-kuleden-dusus");
        Scene(form).ShowFrameForQa(5);
        Capture(form, folder, "34b-kuleden-dusus-son");
        form.FinishStory(StoryCatalog.Find("tower-fall")!);
        Scene(form).ShowChoiceForQa();
        Capture(form, folder, "35-suvari-diriltme");
        Scene(form).ShowResultForQa(0, 0);
        Capture(form, folder, "35b-suvari-zirh");
        form.FinishStory(StoryCatalog.Find("rider-revival")!);
        form.ShowInventory();
        ((InventoryScreen)form.CurrentScreen!).SelectItemForQa(session.Player.Inventory.Find<FatebreakerArmor>()!);
        Capture(form, folder, "36-envanter-kaderbozan");

        form.ShowStory(StoryCatalog.Find("fatebreaker-return")!);
        Scene(form).ShowFrameForQa(2);
        Capture(form, folder, "37-kaderbozan-donus");
        Scene(form).ShowResultForQa(0, 0);
        Capture(form, folder, "37b-kaderin-hukmu-kirildi");
        form.FinishStory(StoryCatalog.Find("fatebreaker-return")!);
        Scene(form).ShowFrameForQa(2);
        Capture(form, folder, "38-vaelor-ikinci-form");
        form.FinishStory(StoryCatalog.Find("vaelor-second-form")!);
        Capture(form, folder, "39-vaelor-final-savasi");

        var finalFight = (CombatStoryEvent)StoryCatalog.Find("vaelor-final-fight")!;
        finalFight.Victory(session);
        form.FinishStory(finalFight);
        Capture(form, folder, "40-final-vaelor-karari");
        GameSession beforeEnding = GameSession.FromSaveData(session.ToSaveData());
        ((EndingScreen)form.CurrentScreen!).ChooseFateForQa(VaelorState.Persuaded);
        Capture(form, folder, "40b-final-secimi");

        string[] endings = [EndingRules.Restore, EndingRules.Share, EndingRules.Destroy, EndingRules.Watcher];
        for (int i = 0; i < endings.Length; i++)
        {
            GameSession copy = GameSession.FromSaveData(beforeEnding.ToSaveData());
            form.UseSessionForQa(copy);
            form.ShowStory(StoryCatalog.Endings[i]);
            var ending = (EndingScreen)form.CurrentScreen!;
            ending.ChooseFateForQa(i == 2 ? VaelorState.Imprisoned : i == 1 ? VaelorState.Persuaded : VaelorState.Killed);
            ending.ChooseEndingForQa(endings[i]);
            Capture(form, folder, $"41{(char)('a' + i)}-final-epilog-{endings[i]}");
        }
        form.UseSessionForQa(session);
        form.ShowJournal();
        ((JournalScreen)form.CurrentScreen!).SelectForQa(JournalFilter.Finished, 0);
        Capture(form, folder, "42-gunluk-biten-gorevler");
    }

    private static StorySceneScreen Scene(MainGameForm form) => (StorySceneScreen)form.CurrentScreen!;

    // Olayı ekranda açar, seçim karesini kaydeder, sabit zarla çözer ve sıradaki olaya geçer.
    private static void PlayScreen(MainGameForm form, GameSession session, string eventId, string folder, string name,
        int choiceIndex, int roll = 18, string? resultName = null)
    {
        StoryEvent story = StoryCatalog.Find(eventId)!;
        form.ShowStory(story);
        Scene(form).ShowChoiceForQa();
        Capture(form, folder, name);
        Scene(form).ShowResultForQa(choiceIndex, roll);
        if (resultName is not null) Capture(form, folder, resultName);
        StoryDirector.Finish(session, story);
    }

    // Ekran açmadan bir hikâye olayını sabit zarla çözer.
    private static StoryEvent? Resolve(GameSession session, string eventId, string choiceId, int roll = 20)
    {
        StoryEvent story = StoryCatalog.Find(eventId)!;
        StoryChoice choice = story.Choices.First(x => x.Id == choiceId);
        D20Roll? d20 = choice.Check is StoryCheck check
            ? new FixedDiceRoller(roll).RollD20(SkillCheckStoryEvent.CheckBonus(session, check))
            : null;
        story.Resolve(session, choice, d20);
        return StoryDirector.Finish(session, story);
    }

    private static void Capture(MainGameForm form, string folder, string name)
    {
        Application.DoEvents();
        Control screen = form.CurrentScreen!;
        using var bitmap = new Bitmap(screen.Width, screen.Height);
        screen.DrawToBitmap(bitmap, new Rectangle(Point.Empty, screen.Size));
        bitmap.Save(Path.Combine(folder, name + ".png"));
    }
}
