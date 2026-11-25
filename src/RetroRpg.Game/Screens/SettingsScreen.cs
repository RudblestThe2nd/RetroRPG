namespace RetroRpg.Game.Screens;

// Pencere ölçeği, tam ekran, müzik/ortam ve efekt sesi tek yerde tutulur. F11 her ekrandan çalışır.
internal sealed class SettingsScreen : GameScreen
{
    private readonly PixelButton _fullscreenButton;
    private readonly PixelButton _soundButton;
    private readonly PixelButton _musicButton;
    private readonly PixelButton _smallerButton;
    private readonly PixelButton _largerButton;

    public SettingsScreen(MainGameForm host) : base(host)
    {
        _smallerButton = AddButton(new PixelButton(new Rectangle(224, 300, 170, 62), "KÜÇÜLT", () => ResizeWindow(-1))
        {
            Skin = PixelButton.MedievalLarge,
            Font = UiTheme.Body
        });
        _largerButton = AddButton(new PixelButton(new Rectangle(406, 300, 170, 62), "BÜYÜT", () => ResizeWindow(1))
        {
            Skin = PixelButton.MedievalLarge,
            Font = UiTheme.Body
        });
        _fullscreenButton = AddButton(new PixelButton(new Rectangle(270, 368, 260, 58), "", ToggleFullscreen)
        {
            Skin = PixelButton.MedievalLarge,
            Font = UiTheme.Body
        });
        _musicButton = AddButton(new PixelButton(new Rectangle(270, 438, 260, 58), "", ToggleMusic)
        {
            Skin = PixelButton.MedievalLarge,
            Font = UiTheme.Body
        });
        _soundButton = AddButton(new PixelButton(new Rectangle(270, 508, 260, 58), "", ToggleSound)
        {
            Skin = PixelButton.MedievalLarge,
            Font = UiTheme.Body
        });
        AddButton(new PixelButton(new Rectangle(304, 588, 192, 60), "GERİ", () => Host.ShowMainMenu())
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

    private void ResizeWindow(int direction)
    {
        Host.ResizeWindow(direction);
        UpdateLabel();
        Invalidate();
    }

    private void ToggleSound()
    {
        Host.CycleEffectsVolume();
        if (Host.EffectsVolume > 0) CombatAudio.Play(CombatSound.Hit);
        UpdateLabel();
        Invalidate();
    }

    private void ToggleMusic()
    {
        Host.CycleMusicVolume();
        UpdateLabel();
        Invalidate();
    }

    private void UpdateLabel()
    {
        _smallerButton.Enabled = Host.CanResizeWindow(-1);
        _largerButton.Enabled = Host.CanResizeWindow(1);
        _fullscreenButton.Text = Host.IsFullscreen ? "TAM EKRAN: AÇIK" : "TAM EKRAN: KAPALI";
        _musicButton.Text = $"MÜZİK: %{Host.MusicVolume}";
        _soundButton.Text = $"EFEKT: %{Host.EffectsVolume}";
    }

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
        UiTheme.DrawTextCentered(g, $"PENCERE BOYUTU: %{Host.WindowScalePercent}", UiTheme.Body, UiTheme.Text,
            new Rectangle(250, 258, 300, 34));
        UiTheme.DrawTextCentered(g, "CTRL + / -: BOYUT  ·  F11: TAM EKRAN", UiTheme.Small, UiTheme.Text,
            new Rectangle(220, 678, 360, 30));
    }
}
