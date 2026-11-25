using RetroRpg.Core.Characters;
using RetroRpg.Core.Assets;
using RetroRpg.Core.Common;
using RetroRpg.Core.Game;
using RetroRpg.Core.Persistence;
using RetroRpg.Core.Rendering;
using RetroRpg.Core.Story;
using RetroRpg.Core.World;
using RetroRpg.Game.Screens;

namespace RetroRpg.Game;

internal sealed class MainGameForm : Form
{
    public const int ScreenSize = 800;
    private static readonly int[] WindowSizes = [800, 960, 1200, 1600];
    private GameScreen? _currentScreen;
    private Rectangle _windowedBounds;
    private FormWindowState _windowedState;
    private readonly bool _qaMode;
    private readonly string _audioSettingsPath;

    // saveDirectory QA'nın oyuncunun gerçek kaydına yazmasını engeller; fixedLogicalSize 800x800 karşılaştırma üretir.
    public MainGameForm(string? saveDirectory = null, bool fixedLogicalSize = false)
    {
        _qaMode = fixedLogicalSize;
        GameDesignRoot = GameDesignRootLocator.Locate();
        UiTheme.Initialize(GameDesignRoot);
        Text = "Retro RPG";
        AutoScaleMode = AutoScaleMode.None;
        FormBorderStyle = _qaMode ? FormBorderStyle.FixedSingle : FormBorderStyle.Sizable;
        int initialSize = _qaMode ? ScreenSize : LargestComfortableWindowSize();
        ClientSize = new Size(initialSize, initialSize);
        MinimumSize = new Size(ScreenSize + 16, ScreenSize + 39);
        MaximizeBox = !_qaMode;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiTheme.Background;
        KeyPreview = true;

        Assets = DefaultAssetCatalog.Create(GameDesignRoot);
        string dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RetroRpg");
        _audioSettingsPath = Path.Combine(dataFolder, "audio-settings.json");
        AudioSettingsData audio = GameMusic.LoadSettings(_audioSettingsPath);
        CombatAudio.SuppressPlayback = saveDirectory is not null;
        CombatAudio.SetVolume(audio.EffectsVolume);
        Music = new GameMusic(Path.Combine(dataFolder, "AudioCache"), suppressPlayback: saveDirectory is not null);
        Music.SetVolume(audio.MusicVolume);
        Ambience = new GameAmbience(Path.Combine(dataFolder, "AudioCache"), suppressPlayback: saveDirectory is not null);
        Ambience.SetVolume(audio.MusicVolume);
        Saves = new SaveGameService(saveDirectory ?? Path.Combine(dataFolder, "Saves"));
        Images = new UiImages(GameDesignRoot, Path.Combine(dataFolder, "missing-assets.log"));
        WoundedHands = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Point[]>>(
            File.ReadAllText(Path.Combine(GameDesignRoot, CharacterArt.WoundFolder, "weapon-hands.json")))!;
        Tilesets = HexTilesetCatalog.Load(GameDesignRoot);
        Enemies = EnemyCatalog.Load(Path.Combine(GameDesignRoot, "Content", "definitions", "enemies-v1.json"));
        Cinematics = CinematicLibrary.Load(GameDesignRoot);
        GameCursor = Images.CreateCursor("ui/assets/cursors/cursor-default_f00.png");
        Cursor = GameCursor;

        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.F11)
            {
                ToggleFullscreen();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && e.KeyCode is Keys.Oemplus or Keys.Add)
            {
                ResizeWindow(1);
                e.Handled = true;
                return;
            }
            if (e.Control && e.KeyCode is Keys.OemMinus or Keys.Subtract)
            {
                ResizeWindow(-1);
                e.Handled = true;
                return;
            }
            _currentScreen?.HandleKey(e.KeyCode);
        };
        KeyPress += (_, e) => _currentScreen?.HandleChar(e.KeyChar);
        if (saveDirectory is null) Shown += (_, _) => ToggleFullscreen();
        ShowMainMenu();
    }

    public string GameDesignRoot { get; }
    public AssetCatalog Assets { get; }
    public SaveGameService Saves { get; }
    public UiImages Images { get; }
    public IReadOnlyDictionary<string, Point[]> WoundedHands { get; }
    public HexTilesetCatalog Tilesets { get; }
    public EnemyCatalog Enemies { get; }
    public CinematicLibrary Cinematics { get; }
    public Cursor GameCursor { get; }
    public GameMusic Music { get; }
    public GameAmbience Ambience { get; }
    public GameSession? Session { get; private set; }
    public GameScreen? CurrentScreen => _currentScreen;
    public bool IsFullscreen { get; private set; }
    public int WindowScalePercent => (int)Math.Round(Math.Min(ClientSize.Width, ClientSize.Height) * 100d / ScreenSize);
    public int MusicVolume => Music.Volume;
    public int EffectsVolume => CombatAudio.Volume;

    public bool HasSave => File.Exists(Path.Combine(Saves.SaveDirectory, "slot-1.json"));

    // Yeni oyun ve kayıt yükleme aynı yoldan geçer: yarım hikâye düğümü veya prolog varsa önce o açılır.
    public void StartGame(GameSession session)
    {
        Session = session;
        Autosave();
        ResumeStoryOrMap();
    }

    private void ResumeStoryOrMap()
    {
        if (StoryDirector.ResumeEvent(RequireSession()) is StoryEvent pending) ShowStory(pending);
        else ShowMap();
    }

    public void ShowStory(StoryEvent storyEvent)
    {
        GameSession session = RequireSession();
        StoryDirector.Begin(session, storyEvent);
        Music.Play(storyEvent is CombatStoryEvent ? MusicTrack.Combat : MusicTrack.Story);
        Ambience.Play(storyEvent is CombatStoryEvent ? CurrentBiomeAmbience(session) : AmbienceTrack.None);
        switch (storyEvent)
        {
            case CombatStoryEvent battle:
                Switch(new CombatScreen(this, session, battle));
                break;
            case EndingStoryEvent:
                Switch(new EndingScreen(this, session));
                break;
            default:
                Switch(new StorySceneScreen(this, session, storyEvent));
                break;
        }
    }

    // Olay sonucu yazıldıktan sonra çağrılır. Sinematik ortasında değil, olay bittiğinde autosave alınır.
    public void FinishStory(StoryEvent storyEvent)
    {
        GameSession session = RequireSession();
        StoryEvent? next = StoryDirector.Finish(session, storyEvent);
        if (storyEvent.SaveAfter) Autosave();
        if (next is null) ShowMap();
        else ShowStory(next);
    }

    public void ShowMainMenu() { Music.Play(MusicTrack.Menu); Ambience.Play(AmbienceTrack.None); Switch(new MainMenuScreen(this)); }
    public void ShowSettings() { Music.Play(MusicTrack.Menu); Ambience.Play(AmbienceTrack.None); Switch(new SettingsScreen(this)); }
    public void ShowCharacterCreation(CharacterCreationDraft? draft = null) { Music.Play(MusicTrack.Menu); Ambience.Play(AmbienceTrack.None); Switch(new CharacterCreationScreen(this, draft)); }
    public void ShowAttributeAllocation(CharacterCreationDraft draft) { Music.Play(MusicTrack.Menu); Ambience.Play(AmbienceTrack.None); Switch(new AttributeAllocationScreen(this, draft)); }
    public void ShowCharacterSummary(CharacterCreationDraft draft) { Music.Play(MusicTrack.Menu); Ambience.Play(AmbienceTrack.None); Switch(new CharacterSummaryScreen(this, draft)); }
    public void ShowStorySelection(CharacterCreationDraft draft) { Music.Play(MusicTrack.Story); Ambience.Play(AmbienceTrack.None); Switch(new StorySelectionScreen(this, draft)); }
    public void ShowMap() { GameSession s = RequireSession(); PlayMapMusic(s); Switch(new MapScreen(this, s)); }
    public void ShowCombat(HexTile tile, RetroRpg.Core.Combat.IDiceRoller? combatDice = null)
    {
        Music.Play(MusicTrack.Combat);
        Ambience.Play(GameAmbience.ForBiome(tile.Biome));
        Switch(new CombatScreen(this, RequireSession(), tile, combatDice));
    }
    public void ShowInventory() { PlayMapMusic(RequireSession()); Switch(new InventoryScreen(this, RequireSession())); }
    public void ShowCharacterRecord() { PlayMapMusic(RequireSession()); Switch(new CharacterRecordScreen(this, RequireSession())); }
    public void ShowJournal() { PlayMapMusic(RequireSession()); Switch(new JournalScreen(this, RequireSession())); }
    public void ShowCamp() { Music.Play(MusicTrack.Camp); Ambience.Play(AmbienceTrack.Camp); Switch(new CampScreen(this, RequireSession())); }
    public void ShowEncounter(WorldEncounter encounter) { PlayMapMusic(RequireSession()); Switch(new EncounterScreen(this, RequireSession(), encounter)); }
    public void ShowLocation(WorldLocation location, LocationService? openedService = null, string? message = null)
    { Music.Play(MusicTrack.Settlement); Ambience.Play(AmbienceTrack.Settlement); Switch(new LocationScreen(this, RequireSession(), location, openedService, message)); }
    public void ShowShop(WorldLocation location, LocationService service)
    { Music.Play(MusicTrack.Settlement); Ambience.Play(AmbienceTrack.Settlement); Switch(new ShopScreen(this, RequireSession(), location, service)); }
    public void ShowInn(WorldLocation location) { Music.Play(MusicTrack.Inn); Ambience.Play(AmbienceTrack.Inn); Switch(new InnScreen(this, RequireSession(), location)); }
    public void ShowGuardDialogue(WorldLocation location, LocationService service) =>
        ShowGuardDialogueWithMusic(location, service);
    public void ShowDeath(string cause) { Music.Play(MusicTrack.Death); Ambience.Play(AmbienceTrack.Death); Switch(new DeathScreen(this, RequireSession(), cause)); }

    private void ShowGuardDialogueWithMusic(WorldLocation location, LocationService service)
    { Music.Play(MusicTrack.Settlement); Ambience.Play(AmbienceTrack.Settlement); Switch(new GuardDialogueScreen(this, RequireSession(), location, service)); }

    private void PlayMapMusic(GameSession session)
    {
        Biome biome = session.Map.Get(session.CurrentHex)?.Biome ?? Biome.Plains;
        Music.Play(GameMusic.ForBiome(biome));
        Ambience.Play(GameAmbience.ForBiome(biome));
    }

    private static AmbienceTrack CurrentBiomeAmbience(GameSession session) =>
        GameAmbience.ForBiome(session.Map.Get(session.CurrentHex)?.Biome ?? Biome.Plains);

    public void CycleMusicVolume()
    {
        Music.SetVolume(NextVolume(Music.Volume));
        Ambience.SetVolume(Music.Volume);
        SaveAudioSettings();
    }

    public void CycleEffectsVolume()
    {
        CombatAudio.SetVolume(NextVolume(CombatAudio.Volume));
        SaveAudioSettings();
    }

    private void SaveAudioSettings() =>
        GameMusic.SaveSettings(_audioSettingsPath, new AudioSettingsData(Music.Volume, CombatAudio.Volume));

    private static int NextVolume(int volume) => volume switch
    {
        >= 100 => 75,
        >= 75 => 50,
        >= 50 => 25,
        >= 25 => 0,
        _ => 100
    };

    public void LoadSavedGame()
    {
        Session = GameSession.FromSaveData(Saves.Load());
        ResumeStoryOrMap();
    }

    public void Autosave()
    {
        if (Session is null) return;
        Session.Flags["autosave:utc"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        Saves.Save(Session.ToSaveData());
    }

    public void ToggleFullscreen()
    {
        SuspendLayout();
        if (!IsFullscreen)
        {
            _windowedState = WindowState;
            _windowedBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            Rectangle monitor = Screen.FromControl(this).Bounds;
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = Size.Empty;
            Bounds = monitor;
            IsFullscreen = true;
        }
        else
        {
            WindowState = FormWindowState.Normal;
            FormBorderStyle = _qaMode ? FormBorderStyle.FixedSingle : FormBorderStyle.Sizable;
            MinimumSize = new Size(Math.Min(816, _windowedBounds.Width), Math.Min(839, _windowedBounds.Height));
            Bounds = _windowedBounds;
            WindowState = _windowedState;
            IsFullscreen = false;
        }
        ResumeLayout();
        _currentScreen?.Invalidate();
    }

    public void ResizeWindow(int direction)
    {
        if (_qaMode || IsFullscreen || direction == 0) return;
        int current = Math.Min(ClientSize.Width, ClientSize.Height);
        int[] available = AvailableWindowSizes();
        int index = direction > 0
            ? Array.FindIndex(available, x => x > current)
            : Array.FindLastIndex(available, x => x < current);
        if (index < 0) return;
        int size = available[index];
        ClientSize = new Size(size, size);
        CenterToScreen();
        _currentScreen?.Invalidate();
    }

    public bool CanResizeWindow(int direction)
    {
        if (_qaMode || IsFullscreen || direction == 0) return false;
        int current = Math.Min(ClientSize.Width, ClientSize.Height);
        return direction > 0
            ? AvailableWindowSizes().Any(x => x > current)
            : AvailableWindowSizes().Any(x => x < current);
    }

    private int[] AvailableWindowSizes()
    {
        int maximum = MaximumClientSize();
        return WindowSizes.Where(x => x <= maximum).Append(maximum).Distinct().Order().ToArray();
    }

    private int LargestComfortableWindowSize()
    {
        int maximum = MaximumClientSize();
        int preset = WindowSizes.LastOrDefault(x => x <= maximum);
        // 1080p ekranda 800'e geri düşmek yerine kullanılabilir alanı dolduran yaklaşık 960px kullanılır.
        return preset > ScreenSize ? preset : Math.Max(ScreenSize, maximum);
    }

    private int MaximumClientSize()
    {
        Rectangle area = Screen.FromPoint(Cursor.Position).WorkingArea;
        return Math.Max(ScreenSize, Math.Min(1600, Math.Min(area.Width - 80, area.Height - 64)));
    }

    private GameSession RequireSession() => Session ?? throw new InvalidOperationException("Oturum yok.");

    // Yalnız --screenshots QA akışı: hazır hikâye durumunu autosave almadan etkin oturum yapar.
    internal void UseSessionForQa(GameSession session) => Session = session;

    private void Switch(GameScreen screen)
    {
        SuspendLayout();
        _currentScreen?.Dispose();
        Controls.Clear();
        _currentScreen = screen;
        Controls.Add(screen);
        ResumeLayout();
        screen.OnShown();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { Ambience.Dispose(); Music.Dispose(); Images.Dispose(); }
        base.Dispose(disposing);
    }
}
