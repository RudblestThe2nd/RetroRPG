namespace RetroRpg.Game.Screens;

// Şimdilik yalnız görüntü ayarı vardır. F11 her ekrandan aynı davranışı çağırır.
internal sealed class SettingsScreen : GameScreen
{
    private readonly PixelButton _fullscreenButton;

    public SettingsScreen(MainGameForm host) : base(host)
    {
        _fullscreenButton = AddButton(new PixelButton(new Rectangle(304, 326, 192, 68), "", ToggleFullscreen)
        {
            Skin = PixelButton.MedievalLarge,
            Font = UiTheme.Body
        });
        AddButton(new PixelButton(new Rectangle(304, 426, 192, 68), "GERİ", () => Host.ShowMainMenu())
        {
            Skin = PixelButton.MedievalLarge,
            Font = UiTheme.Heading
        });
        UpdateLabel();
    }

    private void ToggleFullscreen()
    {
        Host.ToggleFullscreen();
        UpdateLabel();
        Invalidate();
    }

    private void UpdateLabel() =>
        _fullscreenButton.Text = Host.IsFullscreen ? "TAM EKRAN: AÇIK" : "TAM EKRAN: KAPALI";

    public override void HandleKey(Keys key)
    {
        if (key == Keys.Escape) Host.ShowMainMenu();
        else if (key is Keys.Enter or Keys.Space) ToggleFullscreen();
    }

    protected override void Draw(Graphics g)
    {
        UpdateLabel();
        UiTheme.DrawImage(g, Images.Get(AssetPath("ui.main-menu.background.wood-red-v1")), CanvasRectangle);
        UiTheme.DrawTextCentered(g, "AYARLAR", UiTheme.Title, UiTheme.Gold, new Rectangle(274, 204, 240, 42));
        UiTheme.DrawTextCentered(g, "F11: TAM EKRAN AÇ / KAPAT", UiTheme.Small, UiTheme.Text,
            new Rectangle(250, 520, 300, 34));
    }
}
