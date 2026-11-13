using RetroRpg.Core.Game;

namespace RetroRpg.Game.Screens;

// Ana menü düzeni: logosuz arka plan, başlık ve beş düğme ayrı katmanlarda çizilir.
internal sealed class MainMenuScreen : GameScreen
{
    private readonly List<PixelButton> _menu = [];
    private int _selected;

    public MainMenuScreen(MainGameForm host) : base(host)
    {
        bool hasSave = host.HasSave;
        AddMenuButton("YENİ OYUN", true, () => Host.ShowCharacterCreation());
        AddMenuButton("DEVAM ET", hasSave, LoadSave);
        AddMenuButton("OYUN YÜKLE", false, LoadSave);
        AddMenuButton("AYARLAR", true, () => Host.ShowSettings());
        AddMenuButton("ÇIKIŞ", true, () => Host.Close());
        Select(0);
    }

    private void AddMenuButton(string text, bool enabled, Action onClick)
    {
        int index = _menu.Count;
        var button = new PixelButton(new Rectangle(304, 252 + index * 83, 192, 68), text, onClick)
        {
            Skin = PixelButton.MedievalLarge,
            Enabled = enabled,
            Font = UiTheme.Heading
        };
        _menu.Add(AddButton(button));
    }

    private void LoadSave()
    {
        try { Host.StartGame(GameSession.FromSaveData(Host.Saves.Load())); }
        catch (Exception ex) when (ex is IOException or InvalidDataException or KeyNotFoundException or InvalidOperationException)
        {
            MessageBox.Show(ex.Message, "Kayıt yüklenemedi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void Select(int index)
    {
        _selected = index;
        for (int i = 0; i < _menu.Count; i++) _menu[i].Selected = i == index;
        Invalidate();
    }

    public override void HandleKey(Keys key)
    {
        int step = key switch { Keys.Up => -1, Keys.Down => 1, _ => 0 };
        if (step != 0)
        {
            int next = _selected;
            do next = (next + step + _menu.Count) % _menu.Count;
            while (!_menu[next].Enabled);
            Select(next);
        }
        else if (key is Keys.Enter or Keys.Space) _menu[_selected].OnClick();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        int hovered = _menu.FindIndex(x => x.IsHovered);
        if (hovered >= 0 && hovered != _selected) Select(hovered);
    }

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get(AssetPath("ui.main-menu.background.wood-red-v1")), CanvasRectangle);
        UiTheme.DrawTextCentered(g, "ANA MENÜ", UiTheme.Title, UiTheme.Gold, new Rectangle(274, 204, 240, 42));
        Rectangle selected = _menu[_selected].Bounds;
        int y = selected.Y + selected.Height / 2;
        using var arrow = new SolidBrush(UiTheme.Gold);
        g.FillPolygon(arrow, new Point[] { new(270, y), new(282, y - 8), new(282, y + 8) });
        g.FillPolygon(arrow, new Point[] { new(530, y), new(518, y - 8), new(518, y + 8) });
    }
}
