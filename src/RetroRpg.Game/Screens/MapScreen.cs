using RetroRpg.Core.Characters;
using RetroRpg.Core.Combat;
using RetroRpg.Core.Common;
using RetroRpg.Core.Content;
using RetroRpg.Core.Game;
using RetroRpg.Core.Items;
using RetroRpg.Core.Story;
using RetroRpg.Core.World;
using System.Drawing.Drawing2D;

namespace RetroRpg.Game.Screens;

// Üstten bakışlı parşömen dünya resmi, görünmez tıklanabilir hex ızgarasının altında korunur.
// Seçim, rota ve görevler ayrı katmanlardır; tehlike ile düşman haritada gösterilmez.
internal sealed class MapScreen : GameScreen
{
    // Batı, merkez ve doğu aynı sürekli hex ızgarasındadır.
    private const float StepX = 48f;
    private const float StepY = 39f;
    private const float HexRadiusX = 24f;
    private const float HexRadiusY = 26f;
    private const int MovementFrames = 7;
    private const int EncounterDc = 10;
    private static readonly PointF MapCenter = new(400, 372);
    protected override bool WideCanvas => true;
    private Rectangle WorldMapBounds => new(VisibleCanvas.Left, 84, VisibleCanvas.Width, 533);
    private float _cameraX;
    private int? _dragX;
    private float ScrollX
    {
        get
        {
            float limit = Math.Max(0, (2240 - WorldMapBounds.Width) / 2f);
            return Math.Clamp(_cameraX, -limit, limit);
        }
    }
    private void Pan(float dx)
    {
        if (_isTraveling || ModalOpen || _encounterRoll is not null) return;
        _cameraX = ScrollX + dx;
        _cameraX = ScrollX;
        _hovered = null;
        Invalidate();
    }

    private readonly GameSession _session;
    private readonly Dictionary<string, string> _biomeNames;
    private readonly System.Windows.Forms.Timer _messageTimer = new() { Interval = 2500 };
    private readonly System.Windows.Forms.Timer _travelTimer = new() { Interval = 45 };
    private readonly DiceRoller _dice = new();
    private readonly PixelButton _saveButton;
    private readonly PixelButton _menuButton;
    private readonly PixelButton _confirmButton;
    private readonly PixelButton _cancelButton;
    private readonly PixelButton _gridButton;
    private readonly PixelButton _inventoryButton;
    private readonly PixelButton _characterButton;
    private readonly PixelButton _journalButton;
    private readonly PixelButton _campButton;
    private readonly PixelButton _exploreButton;
    private readonly PixelButton _enterLocationButton;
    private readonly PixelButton _closeLocationButton;

    private List<HexCoordinate> _route = [];
    private HexCoordinate? _selectedTarget;
    private HexCoordinate? _hovered;
    private HexCoordinate? _movementFrom;
    private HexCoordinate? _movementTo;
    private int _movementFrame;
    private int _routeIndex;
    private bool _isTraveling;
    private bool _showHexGrid;
    private string _message = "";

    private D20Roll? _encounterRoll;
    private HexTile? _pendingEncounter;
    private int _encounterAnimationTicks;
    private WorldLocation? _pendingLocation;

    public MapScreen(MainGameForm host, GameSession session) : base(host)
    {
        _session = session;
        _cameraX = StepX * (session.CurrentHex.Q + session.CurrentHex.R / 2f);
        AddButton(new PixelButton(new Rectangle(6,620,102,38), "‹ BATI", () => Pan(-288)) { Skin = PixelButton.Medium });
        AddButton(new PixelButton(new Rectangle(692,620,102,38), "DOĞU ›", () => Pan(288)) { Skin = PixelButton.Medium });
        _session.Map.RevealAll();
        ContentDefinitionDocument content = new ContentDefinitionLoader().Load(AssetPath("content.game-data"));
        _biomeNames = content.Biomes.ToDictionary(x => x.Id, x => x.Name, StringComparer.OrdinalIgnoreCase);
        _messageTimer.Tick += (_, _) => { _message = ""; _messageTimer.Stop(); Invalidate(); };
        _travelTimer.Tick += (_, _) => TravelTick();

        _inventoryButton = AddButton(new PixelButton(new Rectangle(12, 666, 150, 52), "ENVANTER", () => Host.ShowInventory()) { Skin = PixelButton.Medium });
        _characterButton = AddButton(new PixelButton(new Rectangle(168, 666, 150, 52), "KARAKTER", () => Host.ShowCharacterRecord()) { Skin = PixelButton.Medium });
        _journalButton = AddButton(new PixelButton(new Rectangle(324, 666, 150, 52), "GÜNLÜK", () => Host.ShowJournal()) { Skin = PixelButton.Medium });
        _campButton = AddButton(new PixelButton(new Rectangle(480, 666, 150, 52), "KAMP", () => Host.ShowCamp()) { Skin = PixelButton.Medium });
        _exploreButton = AddButton(new PixelButton(new Rectangle(636, 666, 152, 52), "ÇEVREYİ ARA", Explore) { Skin = PixelButton.Medium });

        _saveButton = AddButton(new PixelButton(new Rectangle(12, 736, 126, 52), "KAYDET", Save) { Skin = PixelButton.Medium });
        _menuButton = AddButton(new PixelButton(new Rectangle(142, 736, 146, 52), "ANA MENÜ", () => Host.ShowMainMenu()) { Skin = PixelButton.Medium });
        _confirmButton = AddButton(new PixelButton(new Rectangle(292, 736, 216, 52), "ROTAYI ONAYLA", ConfirmRoute)
        { Skin = PixelButton.Medium, Enabled = false });
        _cancelButton = AddButton(new PixelButton(new Rectangle(512, 736, 132, 52), "İPTAL", CancelRoute)
        { Skin = PixelButton.Medium, Enabled = false });
        _gridButton = AddButton(new PixelButton(new Rectangle(648, 736, 140, 52), "HEX: KAPALI", ToggleGrid)
        { Skin = PixelButton.Medium });
        _enterLocationButton = AddModalButton(new PixelButton(new Rectangle(102, 586, 274, 62), "İÇERİ GİR", EnterLocation)
        { Skin = PixelButton.MedievalLarge, Visible = false });
        _closeLocationButton = AddModalButton(new PixelButton(new Rectangle(424, 586, 274, 62), "VAZGEÇ", CloseLocation)
        { Skin = PixelButton.MedievalLarge, Visible = false });
    }

    // Gizli encounter aynı hexte her zaman aynı düşmanı üretir.
    public static EnemyDefinition EncounterFor(MainGameForm host, HexTile tile) =>
        host.Enemies.PickEncounter(tile.Biome, tile.Coordinate.DistanceTo(new HexCoordinate(0, 0)),
            tile.Coordinate.Q * 31 + tile.Coordinate.R * 17);

    private void Save()
    {
        Host.Saves.Save(_session.ToSaveData());
        ShowMessage("Oyun kaydedildi.");
    }

    private void ShowMessage(string message)
    {
        _message = message;
        _messageTimer.Stop();
        _messageTimer.Start();
        Invalidate();
    }

    private PointF CenterOf(HexCoordinate hex) =>
        new(MapCenter.X - ScrollX + StepX * (hex.Q + hex.R / 2f), MapCenter.Y + StepY * hex.R);

    private PointF[] HexPolygon(HexCoordinate hex)
    {
        PointF center = CenterOf(hex);
        return
        [
            new(center.X, center.Y - HexRadiusY),
            new(center.X + HexRadiusX, center.Y - HexRadiusY / 2f),
            new(center.X + HexRadiusX, center.Y + HexRadiusY / 2f),
            new(center.X, center.Y + HexRadiusY),
            new(center.X - HexRadiusX, center.Y + HexRadiusY / 2f),
            new(center.X - HexRadiusX, center.Y - HexRadiusY / 2f)
        ];
    }

    private HexCoordinate? HexAt(Point point)
    {
        if (!WorldMapBounds.Contains(point)) return null;
        // Altıgenin eğik kenarı da tıklama alanına dahildir.
        foreach (HexTile tile in _session.Map.Tiles.Values)
        {
            PointF center = CenterOf(tile.Coordinate);
            float dx = Math.Abs(center.X - point.X), dy = Math.Abs(center.Y - point.Y);
            if (dx <= HexRadiusX && dy <= HexRadiusY - dx * .5f * HexRadiusY / HexRadiusX)
                return tile.Coordinate;
        }
        return null;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragX is int previous && e.Button == MouseButtons.Right)
        {
            int x = ToLogical(e.Location).X;
            Pan(previous - x);
            _dragX = x;
            return;
        }
        HexCoordinate? hovered = !_isTraveling && ButtonAt(e.Location) is null ? HexAt(ToLogical(e.Location)) : null;
        if (hovered == _hovered) return;
        _hovered = hovered;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Right && WorldMapBounds.Contains(ToLogical(e.Location)))
        {
            _dragX = ToLogical(e.Location).X;
            Capture = true;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Right) { _dragX = null; Capture = false; }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (ModalOpen || _encounterRoll is not null || _isTraveling || e.Button != MouseButtons.Left || ButtonAt(e.Location) is not null ||
            HexAt(ToLogical(e.Location)) is not HexCoordinate target) return;
        SelectRoute(target);
    }

    private void SelectRoute(HexCoordinate target)
    {
        if (target == _session.CurrentHex)
        {
            CancelRoute();
            return;
        }

        IReadOnlyList<HexCoordinate> path = _session.Map.FindPath(_session.CurrentHex, target);
        if (path.Count == 0)
        {
            System.Media.SystemSounds.Beep.Play();
            HexTile? blocked = _session.Map.Get(target);
            // Göz Kulesi görünür ama mor kapı açılmadan ulaşılamaz; oyuncu nedenini görür.
            ShowMessage(blocked?.IsLocked == true
                ? blocked.LockReason ?? "Bu geçit hikâye veya keşif ilerlemesi gerektiriyor."
                : blocked?.Biome == Biome.Corrupted && _session.Map.Get(HexMap.PurpleGate) is { IsLocked: true, LockReason: string gateReason }
                    ? gateReason
                    : "Bu hedefe yürüyüş rotası yok.");
            return;
        }

        _route = path.Skip(1).ToList();
        _routeIndex = 0;
        _selectedTarget = target;
        UpdateButtons();
        ShowMessage($"Rota seçildi: {_route.Count} hex. Yürümek için onayla.");
    }

    // Yalnız otomatik 800 x 800 ekran görüntüsü QA akışı tarafından kullanılır.
    internal void PreviewRouteForQa(HexCoordinate target) => SelectRoute(target);
    internal void CheckProvinceForQa(int direction)
    {
        Pan(direction * 2000);
        var hex = new HexCoordinate(direction * 19, 0);
        var center = Point.Round(CenterOf(hex));
        if (HexAt(center) != hex) throw new InvalidOperationException("Dış bölgede hex seçilemedi.");
        SelectRoute(hex);
        if (_selectedTarget != hex) throw new InvalidOperationException("Dış bölgeye rota bulunamadı.");
    }
    internal void ShowGridForQa()
    {
        if (!_showHexGrid) ToggleGrid();
    }
    internal void OpenLocationForQa(WorldLocation location)
    {
        _pendingLocation = location;
        ModalOpen = true;
        _enterLocationButton.Visible = _closeLocationButton.Visible = true;
        Invalidate();
    }

    private void ConfirmRoute()
    {
        if (_route.Count == 0 || _isTraveling) return;
        _isTraveling = true;
        _routeIndex = 0;
        _movementFrom = null;
        _movementTo = null;
        UpdateButtons();
        _message = "Rota üzerinde ilerleniyor...";
        _messageTimer.Stop();
        _travelTimer.Interval = 45;
        _travelTimer.Start();
        Invalidate();
    }

    private void CancelRoute()
    {
        if (_isTraveling) return;
        _route = [];
        _selectedTarget = null;
        _routeIndex = 0;
        UpdateButtons();
        ShowMessage("Rota iptal edildi.");
    }

    private void UpdateButtons()
    {
        _saveButton.Enabled = !_isTraveling;
        _menuButton.Enabled = !_isTraveling;
        _confirmButton.Enabled = !_isTraveling && _route.Count > 0;
        _cancelButton.Enabled = !_isTraveling && _route.Count > 0;
        _gridButton.Enabled = !_isTraveling;
        _inventoryButton.Enabled = !_isTraveling;
        _characterButton.Enabled = !_isTraveling;
        _journalButton.Enabled = !_isTraveling;
        _campButton.Enabled = !_isTraveling;
        _exploreButton.Enabled = !_isTraveling;
    }

    private void Explore()
    {
        if (_isTraveling) return;
        // Hikâye olayı bu hexte bekliyorsa (kervan enkazı, ertelenmiş Yol Taşı) önce o açılır.
        if (StoryDirector.EventOnSearch(_session, _session.CurrentHex) is StoryEvent story)
        {
            Host.ShowStory(story);
            return;
        }
        HexTile tile = _session.Map.Get(_session.CurrentHex)!;
        _pendingLocation = WorldLocationCatalog.FindAt(tile);
        if (_pendingLocation is not null)
        {
            ModalOpen = true;
            _enterLocationButton.Visible = _closeLocationButton.Visible = true;
            Invalidate();
            return;
        }
        int seed = tile.Coordinate.Q * 997 + tile.Coordinate.R * 313 + _session.Flags.Count * 17;
        Host.ShowEncounter(EncounterCatalog.Pick(tile.Biome, seed));
    }

    private void EnterLocation()
    {
        if (_pendingLocation is null) return;
        WorldLocation location = _pendingLocation;
        CloseLocation();
        Host.ShowLocation(location);
    }

    private void CloseLocation()
    {
        ModalOpen = false;
        _enterLocationButton.Visible = _closeLocationButton.Visible = false;
        _pendingLocation = null;
        Invalidate();
    }

    private void ToggleGrid()
    {
        if (_isTraveling) return;
        _showHexGrid = !_showHexGrid;
        _gridButton.Text = _showHexGrid ? "HEX: AÇIK" : "HEX: KAPALI";
        _gridButton.Selected = _showHexGrid;
        Invalidate();
    }

    private void TravelTick()
    {
        if (_encounterRoll is not null)
        {
            AnimateEncounterRoll();
            return;
        }

        if (_movementTo is null)
        {
            if (_routeIndex >= _route.Count)
            {
                FinishRoute();
                return;
            }
            _movementFrom = _session.CurrentHex;
            _movementTo = _route[_routeIndex];
            _movementFrame = 0;
        }

        _movementFrame++;
        float screenX = AnimatedPlayerCenter().X;
        _cameraX = ScrollX + screenX - Math.Clamp(screenX, WorldMapBounds.Left + 100, WorldMapBounds.Right - 100);
        Invalidate();
        if (_movementFrame < MovementFrames) return;

        HexCoordinate destination = _movementTo.Value;
        _movementFrom = null;
        _movementTo = null;
        if (!_session.TryMove(destination))
        {
            StopTravel();
            ShowMessage("Rota beklenmedik biçimde kapandı.");
            return;
        }

        _routeIndex++;
        HexTile tile = _session.Map.Get(destination)!;
        // Sabit hikâye olayları rastgele encounter'dan önce gelir; Pass olayları geçerken, Arrive olayları rota bitince oynar.
        if (StoryDirector.EventOnStep(_session, destination, _routeIndex >= _route.Count) is StoryEvent story)
        {
            StopTravel();
            Host.Autosave();
            Host.ShowStory(story);
            return;
        }
        if (tile.IsForcedCombat && tile.IsDangerous)
        {
            StopTravel();
            Host.Autosave();
            Host.ShowCombat(tile);
            return;
        }
        if (tile.IsDangerous)
        {
            BeginEncounterCheck(tile);
            return;
        }
        if (_routeIndex >= _route.Count) FinishRoute();
    }

    private void BeginEncounterCheck(HexTile tile)
    {
        int wisdomBonus = CharacterStats.Modifier(_session.Player.Stats.Wisdom);
        _pendingEncounter = tile;
        _encounterRoll = _dice.RollD20(wisdomBonus);
        _encounterAnimationTicks = 20;
        _travelTimer.Interval = 80;
        _message = "Encounter kontrolü: d20 + Bilgelik";
        Invalidate();
    }

    private void AnimateEncounterRoll()
    {
        _encounterAnimationTicks--;
        Invalidate();
        if (_encounterAnimationTicks > 0) return;

        D20Roll roll = _encounterRoll!;
        HexTile tile = _pendingEncounter!;
        bool avoided = roll.IsNaturalTwenty || (!roll.IsNaturalOne && roll.Total >= EncounterDc);
        _encounterRoll = null;
        _pendingEncounter = null;
        _travelTimer.Interval = 45;

        if (!avoided)
        {
            StopTravel();
            Host.ShowCombat(tile);
            return;
        }

        _message = $"Encounter atlatıldı: {roll.RawValue} {FormatBonus(roll.Bonus)} = {roll.Total}.";
        _messageTimer.Stop();
        _messageTimer.Start();
        if (_routeIndex >= _route.Count) FinishRoute();
    }

    private static string FormatBonus(int bonus) => bonus >= 0 ? $"+ {bonus}" : $"- {Math.Abs(bonus)}";

    private void FinishRoute()
    {
        _travelTimer.Stop();
        _isTraveling = false;
        _route = [];
        _selectedTarget = null;
        _routeIndex = 0;
        _movementFrom = null;
        _movementTo = null;
        UpdateButtons();
        Host.Autosave();
        ShowMessage("Hedefe ulaşıldı.");
    }

    private void StopTravel()
    {
        _travelTimer.Stop();
        _isTraveling = false;
        _route = [];
        _selectedTarget = null;
        _routeIndex = 0;
        _movementFrom = null;
        _movementTo = null;
        _encounterRoll = null;
        _pendingEncounter = null;
        UpdateButtons();
    }

    public override void HandleKey(Keys key)
    {
        if (ModalOpen || _encounterRoll is not null) return;
        if (key == Keys.Left) Pan(-144);
        else if (key == Keys.Right) Pan(144);
        else if (key is Keys.Enter or Keys.Space) ConfirmRoute();
        else if (key == Keys.Escape) CancelRoute();
        else if (key == Keys.H) ToggleGrid();
        else if (key == Keys.I) Host.ShowInventory();
        else if (key == Keys.C) Host.ShowCharacterRecord();
        else if (key == Keys.J) Host.ShowJournal();
        else if (key == Keys.K) Host.ShowCamp();
        else if (key == Keys.E) Explore();
    }

    protected override void Draw(Graphics g)
    {
        g.Clear(Color.FromArgb(10, 9, 14));
        var state = g.Save();
        g.SetClip(WorldMapBounds);
        int scroll = (int)Math.Round(ScrollX);
        UiTheme.DrawImage(g, Images.Get(AssetPath("map.world.continuous")), new Rectangle(-720 - scroll, 84, 2240, 533));
        DrawTerrainObstacles(g);

        if (_showHexGrid) DrawHexGrid(g);
        DrawQuestLayer(g);
        DrawRoute(g);

        if (_hovered is HexCoordinate hovered) DrawHexOutline(g, hovered, Color.FromArgb(210, 245, 210, 92), 1.5f);

        PointF playerCenter = AnimatedPlayerCenter();
        using (var playerGlow = new SolidBrush(Color.FromArgb(105, 42, 224, 232)))
            g.FillEllipse(playerGlow, playerCenter.X - 20, playerCenter.Y - 14, 40, 28);
        using (var playerRing = new Pen(Color.FromArgb(235, 101, 245, 255), 2f))
            g.DrawEllipse(playerRing, playerCenter.X - 20, playerCenter.Y - 14, 40, 28);
        var player = _session.Player;
        UiTheme.DrawImageFit(g, Images.Get(CharacterArt.TokenPath(player.Race.Id, player.Gender)),
            new Rectangle((int)playerCenter.X - 18, (int)playerCenter.Y - 18, 36, 36));

        g.Restore(state);
        DrawHud(g);
    }

    private void DrawHexGrid(Graphics g)
    {
        using var shadow = new Pen(Color.FromArgb(105, 9, 8, 12), 2f);
        using var line = new Pen(Color.FromArgb(170, 221, 177, 72), 1f);
        using var gateLine = new Pen(Color.FromArgb(245, 227, 184, 95), 2f);
        using var gateFill = new SolidBrush(Color.FromArgb(82, 104, 72, 13));
        foreach (HexTile tile in _session.Map.Tiles.Values)
        {
            PointF[] points = HexPolygon(tile.Coordinate);
            // Mühürlü mor sınır açılabilir geçit değildir; yalnız gerçek geçitler işaret alır.
            bool gate = tile.IsLocked && !tile.IsPermanentObstacle && !tile.IsSealed;
            if (gate) g.FillPolygon(gateFill, points);
            g.DrawPolygon(shadow, points);
            g.DrawPolygon(gate ? gateLine : line, points);
            if (gate)
            {
                PointF center = CenterOf(tile.Coordinate);
                UiTheme.DrawTextCentered(g, "◆", UiTheme.Heading, UiTheme.Gold,
                    new Rectangle((int)center.X - 13, (int)center.Y - 15, 26, 30));
            }
        }
    }

    private void DrawTerrainObstacles(Graphics g)
    {
        Image? mountain = Images.Get(AssetPath("map.obstacle.corrupted-mountain"), trim: true);
        if (mountain is null) return;

        // X/şerit yerine, geçilmez hexin üstünde haritanın parçası gibi duran gerçek bir dağ kümesi çizilir.
        foreach (HexTile tile in _session.Map.Tiles.Values
                     .Where(x => x.IsPermanentObstacle && NeedsMountainOverlay(x))
                     .OrderBy(x => CenterOf(x.Coordinate).Y))
        {
            PointF center = CenterOf(tile.Coordinate);
            UiTheme.DrawImageFit(g, mountain,
                new Rectangle((int)center.X - 29, (int)center.Y - 29, 58, 51));
        }
    }

    private static bool NeedsMountainOverlay(HexTile tile) =>
        tile.Biome is not (Biome.Volcanic or Biome.Mountain or Biome.Snow);

    // Kızılburç ve Göz Kulesi her zaman görünür; aktif hikâye hedefleri aynı görev işaretini alır.
    private void DrawQuestLayer(Graphics g)
    {
        IEnumerable<HexCoordinate> targets = _session.Map.Tiles.Values.Where(x => x.HasQuest).Select(x => x.Coordinate)
            .Concat(StoryDirector.Markers(_session).Select(x => x.Hex)).Distinct();
        foreach (HexCoordinate target in targets)
        {
            PointF center = CenterOf(target);
            using var glow = new SolidBrush(Color.FromArgb(125, 255, 194, 41));
            using var ring = new Pen(Color.FromArgb(245, 255, 221, 93), 2f);
            g.FillEllipse(glow, center.X - 13, center.Y - 13, 26, 26);
            g.DrawEllipse(ring, center.X - 13, center.Y - 13, 26, 26);
            UiTheme.DrawTextCentered(g, "!", UiTheme.Heading, Color.White,
                new Rectangle((int)center.X - 12, (int)center.Y - 16, 24, 30));
        }
    }

    private void DrawRoute(Graphics g)
    {
        if (_route.Count == 0) return;

        var points = new List<PointF> { CenterOf(_session.CurrentHex) };
        points.AddRange(_route.Skip(_routeIndex).Select(CenterOf));
        if (points.Count > 1)
        {
            using var shadow = new Pen(Color.FromArgb(210, 5, 8, 15), 6f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            using var route = new Pen(Color.FromArgb(245, 77, 226, 241), 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            g.DrawLines(shadow, points.ToArray());
            g.DrawLines(route, points.ToArray());
        }

        foreach (PointF point in points.Skip(1))
        {
            using var dot = new SolidBrush(Color.FromArgb(235, 255, 221, 93));
            g.FillEllipse(dot, point.X - 3, point.Y - 3, 6, 6);
        }

        if (_selectedTarget is HexCoordinate target)
        {
            using var fill = new SolidBrush(Color.FromArgb(80, 77, 226, 241));
            g.FillPolygon(fill, HexPolygon(target));
            DrawHexOutline(g, target, Color.FromArgb(245, 255, 221, 93), 2f);
        }
    }

    private void DrawHexOutline(Graphics g, HexCoordinate hex, Color color, float width)
    {
        using var pen = new Pen(color, width);
        g.DrawPolygon(pen, HexPolygon(hex));
    }

    private PointF AnimatedPlayerCenter()
    {
        if (_movementFrom is not HexCoordinate from || _movementTo is not HexCoordinate to) return CenterOf(_session.CurrentHex);
        PointF a = CenterOf(from), b = CenterOf(to);
        float t = Math.Clamp((float)_movementFrame / MovementFrames, 0f, 1f);
        // Smoothstep; sprite başlangıç ve bitişte yavaşlar, hexler arasında ışınlanmaz.
        t = t * t * (3f - 2f * t);
        return new PointF(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
    }

    private void DrawHud(Graphics g)
    {
        var player = _session.Player;
        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, new Rectangle(VisibleCanvas.Left+6, 6, VisibleCanvas.Width-12, 76));
        UiTheme.DrawPanel(g, Images, PanelStyle.Stone, new Rectangle(15, 11, 66, 66));
        UiTheme.DrawImageFit(g, Images.Get(CharacterArt.PortraitPath(player.Race.Id, player.Gender, _session.PortraitIndex)), new Rectangle(18, 14, 60, 60));
        UiTheme.DrawText(g, player.Name, UiTheme.Heading, UiTheme.Gold, 88, 14);
        UiTheme.DrawText(g, $"{player.Race.DisplayName} · {player.Class.DisplayName} / {player.Class.SubclassName}", UiTheme.Small, UiTheme.Text, 88, 44);

        UiTheme.DrawText(g, "CAN", UiTheme.Small, UiTheme.Text, 360, 18);
        UiTheme.DrawBar(g, new Rectangle(396, 18, 150, 14), player.Health, player.MaxHealth, UiTheme.Danger);
        UiTheme.DrawText(g, $"{player.Health}/{player.MaxHealth}", UiTheme.Small, UiTheme.Text, 554, 18);
        int potions = player.Inventory.Items.OfType<Consumable>().Count();
        UiTheme.DrawText(g, $"ALTIN {player.Gold}   İKSİR {potions}   AC {player.ArmorClass}", UiTheme.Small, UiTheme.Gold, 360, 46);

        HexTile current = _session.Map.Get(_session.CurrentHex)!;
        UiTheme.DrawText(g, BiomeName(current.Biome), UiTheme.Body, UiTheme.Text, 650, 18);
        UiTheme.DrawText(g, $"{_session.CurrentHex.Q},{_session.CurrentHex.R}", UiTheme.Small, UiTheme.Muted, 650, 46);

        string text = _message.Length > 0 ? _message : HoverText();
        var info = new Rectangle(112, 620, 576, 38);
        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, info);
        UiTheme.DrawTextCentered(g, text, UiTheme.Small, _message.Length > 0 ? UiTheme.Success : UiTheme.Text, info);

        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, new Rectangle(VisibleCanvas.Left+6, 658, VisibleCanvas.Width-12, 138));
    }

    private string HoverText()
    {
        if (_isTraveling) return $"Yürüyor: {_routeIndex}/{_route.Count}";
        if (_route.Count > 0) return $"Rota: {_route.Count} hex · onay bekliyor";
        if (_hovered is not HexCoordinate hex || _session.Map.Get(hex) is not HexTile tile) return ActiveGoalText();
        StoryMarker? marker = StoryDirector.Markers(_session).FirstOrDefault(x => x.Hex == hex);
        string state = tile.IsLocked ? tile.LockReason ?? "geçit kilitli" : marker is not null ? $"görev: {marker.Label}" : tile.HasQuest ? "görev" : "hedef";
        return $"{BiomeName(tile.Biome)} · {state}";
    }

    // Boşta iken bilgi çubuğu aktif ana görev hedefini gösterir.
    private string ActiveGoalText()
    {
        Quest? main = _session.Story.Quests.Values.FirstOrDefault(x => x is MainQuest && x.Status == QuestStatus.Active && x.ActiveObjective is not null);
        return main is null || !StoryDirector.UsesMainStory(_session)
            ? "Hedef seç → rotayı onayla"
            : $"Görev: {main.Title} · {StoryText.Format(_session, main.ActiveObjective!.Label)}";
    }

    protected override void DrawTop(Graphics g)
    {
        if (_encounterRoll is null) return;
        UiTheme.Shade(g, VisibleCanvas, 155);
        var panel = new Rectangle(238, 208, 324, 330);
        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, panel);
        UiTheme.DrawTextCentered(g, "ENCOUNTER ZARI", UiTheme.Title, UiTheme.Gold, new Rectangle(250, 226, 300, 40));

        bool showResult = RollingD20.Landed(_encounterAnimationTicks);
        RollingD20.Draw(g, new(320, 270, 160, 160), _encounterRoll.RawValue, _encounterAnimationTicks, 20);
        UiTheme.DrawTextCentered(g, $"d20 {FormatBonus(_encounterRoll.Bonus)}  ·  ZORLUK {EncounterDc}", UiTheme.Body, UiTheme.Text, new Rectangle(260, 442, 280, 32));
        string result = showResult ? $"SONUÇ: {_encounterRoll.Total}" : "ZAR ATILIYOR...";
        UiTheme.DrawTextCentered(g, result, UiTheme.Heading, showResult ? UiTheme.Success : UiTheme.Gold, new Rectangle(260, 480, 280, 34));
    }

    protected override void DrawModal(Graphics g)
    {
        if (_pendingLocation is null) return;
        UiTheme.Shade(g, VisibleCanvas, 165);
        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, new(80, 118, 640, 546));
        UiTheme.DrawPanel(g, Images, PanelStyle.StoneWood, new(100, 232, 310, 310));
        UiTheme.DrawPanel(g, Images, PanelStyle.StoneWood, new(418, 232, 282, 310));
        UiTheme.DrawTextCentered(g, _pendingLocation.KindName, UiTheme.Title, UiTheme.Gold, new Rectangle(100, 126, 600, 82));
        UiTheme.DrawImageFit(g, Images.Get(_pendingLocation.SceneAsset), new Rectangle(108, 246, 294, 286));
        UiTheme.DrawWrappedCentered(g, _pendingLocation.Name.ToUpperInvariant(), UiTheme.Heading, UiTheme.Text, new Rectangle(435, 248, 258, 92));
        for (int i = 0; i < Math.Min(4, _pendingLocation.Services.Count); i++)
            UiTheme.DrawWrappedCentered(g, ServiceName(_pendingLocation.Services[i]), UiTheme.Small, UiTheme.Gold,
                new Rectangle(423 + i * 72, 380, 64, 62));
        string extra = _pendingLocation.Services.Count > 4
            ? $"\nDiğer: {string.Join(" · ", _pendingLocation.Services.Skip(4).Select(ServiceName))}"
            : "";
        UiTheme.DrawWrappedCentered(g, _pendingLocation.Description + extra, UiTheme.Small, UiTheme.Text, new Rectangle(432, 468, 264, 72));
    }

    private static string ServiceName(LocationService service) => service switch
    {
        LocationService.Inn => "HAN",
        LocationService.Market => "PAZAR",
        LocationService.Blacksmith => "DEMİRCİ",
        LocationService.Healer => "ŞİFACI",
        LocationService.Temple => "TAPINAK",
        LocationService.Barracks => "KIŞLA",
        LocationService.WarRoom => "SAVAŞ ODASI",
        _ => "ZİNDAN"
    };

    private string BiomeName(Biome biome) => _biomeNames.GetValueOrDefault(biome.ToString(), biome.ToString());

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _messageTimer.Dispose();
            _travelTimer.Dispose();
        }
        base.Dispose(disposing);
    }
}
