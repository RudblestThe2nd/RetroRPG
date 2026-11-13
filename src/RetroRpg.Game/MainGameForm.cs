using RetroRpg.Core.Characters;
using RetroRpg.Core.Assets;
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
    private GameScreen? _currentScreen;
    private Rectangle _windowedBounds;

    // saveDirectory yalnız ekran görüntüsü QA'sında verilir; böylece QA oyuncunun gerçek kaydına yazmaz.
    public MainGameForm(string? saveDirectory = null)
    {
        GameDesignRoot = GameDesignRootLocator.Locate();
        UiTheme.Initialize(GameDesignRoot);
        Text = "Retro RPG";
        AutoScaleMode = AutoScaleMode.None;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        ClientSize = new Size(ScreenSize, ScreenSize);
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiTheme.Background;
        KeyPreview = true;

        Assets = DefaultAssetCatalog.Create(GameDesignRoot);
        string dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RetroRpg");
        Saves = new SaveGameService(saveDirectory ?? Path.Combine(dataFolder, "Saves"));
        Images = new UiImages(GameDesignRoot, Path.Combine(dataFolder, "missing-assets.log"));
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
            _currentScreen?.HandleKey(e.KeyCode);
        };
        KeyPress += (_, e) => _currentScreen?.HandleChar(e.KeyChar);
        ShowMainMenu();
    }

    public string GameDesignRoot { get; }
    public AssetCatalog Assets { get; }
    public SaveGameService Saves { get; }
    public UiImages Images { get; }
    public HexTilesetCatalog Tilesets { get; }
    public EnemyCatalog Enemies { get; }
    public CinematicLibrary Cinematics { get; }
    public Cursor GameCursor { get; }
    public GameSession? Session { get; private set; }
    public GameScreen? CurrentScreen => _currentScreen;
    public bool IsFullscreen { get; private set; }

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

    public void ShowMainMenu() => Switch(new MainMenuScreen(this));
    public void ShowSettings() => Switch(new SettingsScreen(this));
    public void ShowCharacterCreation(CharacterCreationDraft? draft = null) => Switch(new CharacterCreationScreen(this, draft));
    public void ShowAttributeAllocation(CharacterCreationDraft draft) => Switch(new AttributeAllocationScreen(this, draft));
    public void ShowCharacterSummary(CharacterCreationDraft draft) => Switch(new CharacterSummaryScreen(this, draft));
    public void ShowStorySelection(CharacterCreationDraft draft) => Switch(new StorySelectionScreen(this, draft));
    public void ShowMap() => Switch(new MapScreen(this, RequireSession()));
    public void ShowCombat(HexTile tile) => Switch(new CombatScreen(this, RequireSession(), tile));
    public void ShowInventory() => Switch(new InventoryScreen(this, RequireSession()));
    public void ShowCharacterRecord() => Switch(new CharacterRecordScreen(this, RequireSession()));
    public void ShowJournal() => Switch(new JournalScreen(this, RequireSession()));
    public void ShowCamp() => Switch(new CampScreen(this, RequireSession()));
    public void ShowEncounter(WorldEncounter encounter) => Switch(new EncounterScreen(this, RequireSession(), encounter));
    public void ShowLocation(WorldLocation location, LocationService? openedService = null, string? message = null) =>
        Switch(new LocationScreen(this, RequireSession(), location, openedService, message));
    public void ShowShop(WorldLocation location, LocationService service) => Switch(new ShopScreen(this, RequireSession(), location, service));
    public void ShowInn(WorldLocation location) => Switch(new InnScreen(this, RequireSession(), location));
    public void ShowGuardDialogue(WorldLocation location, LocationService service) =>
        Switch(new GuardDialogueScreen(this, RequireSession(), location, service));
    public void ShowDeath(string cause) => Switch(new DeathScreen(this, RequireSession(), cause));

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
            _windowedBounds = Bounds;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            IsFullscreen = true;
        }
        else
        {
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Bounds = _windowedBounds;
            IsFullscreen = false;
        }
        ResumeLayout();
        _currentScreen?.Invalidate();
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
        if (disposing) Images.Dispose();
        base.Dispose(disposing);
    }
}
