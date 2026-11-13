using RetroRpg.Core.Game;

namespace RetroRpg.Game.Screens;

internal sealed class DeathScreen : GameScreen
{
    private readonly GameSession _session;
    private readonly string _cause;

    public DeathScreen(MainGameForm host, GameSession session, string cause) : base(host)
    {
        _session = session;
        _cause = cause;
        AddButton(new PixelButton(new Rectangle(285, 470, 230, 60), "AUTOSAVE YÜKLE", Load) { Skin = PixelButton.MedievalLarge });
        AddButton(new PixelButton(new Rectangle(285, 548, 230, 60), "ANA MENÜ", () => Host.ShowMainMenu()) { Skin = PixelButton.MedievalLarge });
        AddButton(new PixelButton(new Rectangle(285, 626, 230, 60), "ÇIKIŞ", () => Host.Close()) { Skin = PixelButton.MedievalLarge });
    }

    private void Load()
    {
        try { Host.LoadSavedGame(); }
        catch (Exception ex) when (ex is IOException or InvalidDataException or InvalidOperationException)
        { MessageBox.Show(ex.Message, "Kayıt yüklenemedi", MessageBoxButtons.OK, MessageBoxIcon.Information); }
    }

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get("assets/final-production-v5/ui/screens/runtime-800/death-screen-wood-red-v2.png"), CanvasRectangle);
        UiTheme.DrawTextCentered(g, "YOLCULUK SONA ERDİ", UiTheme.Title, UiTheme.Danger, new Rectangle(150, 278, 500, 58));
        UiTheme.DrawTextCentered(g, $"{_session.Player.Name}, {_cause} nedeniyle yenildi.", UiTheme.Body, UiTheme.Text,
            new Rectangle(150, 356, 500, 70));
    }
}
