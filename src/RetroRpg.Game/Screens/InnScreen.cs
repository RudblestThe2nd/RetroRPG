using RetroRpg.Core.Game;
using RetroRpg.Core.Story;
using RetroRpg.Core.World;

namespace RetroRpg.Game.Screens;

internal sealed class InnScreen : GameScreen
{
    private readonly GameSession _session;
    private readonly WorldLocation _location;
    private readonly StoryEvent? _story;
    private string _message = "Hana girdin. Henüz hiçbir ücret ödemedin.";

    public InnScreen(MainGameForm host, GameSession session, WorldLocation location) : base(host)
    {
        _session = session;
        _location = location;
        // Yeşilgeçit hanında ana hikâyenin tanık konuşması bekliyorsa söylenti yerine o açılır.
        _story = StoryDirector.EventAtInn(session, location.LocationId);
        if (_story is not null) _message = "Kervanı görenler köşedeki masada toplanmış.";
        AddButton(new PixelButton(new Rectangle(70, 574, 200, 58), "DİNLEN · 3 ALTIN", Rest) { Skin = PixelButton.MedievalLarge, Font = UiTheme.Small });
        AddButton(new PixelButton(new Rectangle(300, 574, 200, 58), _story is null ? "ORTAMI DİNLE" : "TANIKLARI DİNLE", Listen)
        { Skin = PixelButton.MedievalLarge, Font = UiTheme.Small });
        AddButton(new PixelButton(new Rectangle(530, 574, 200, 58), "HANCIYLA KONUŞ", Talk) { Skin = PixelButton.MedievalLarge, Font = UiTheme.Small });
        AddButton(new PixelButton(new Rectangle(270, 710, 260, 58), "YERLEŞİME DÖN", () => Host.ShowLocation(_location)) { Skin = PixelButton.MedievalLarge });
    }

    private void Rest()
    {
        if (_session.Player.Gold < 3) _message = "Dinlenmek için 3 altın gerekiyor.";
        else
        {
            _session.Player.Gold -= 3;
            int before = _session.Player.Health;
            _session.Player.Heal(_session.Player.MaxHealth);
            _message = $"Bir oda tuttun ve dinlendin. +{_session.Player.Health - before} CAN.";
            Host.Autosave();
        }
        Invalidate();
    }

    private void Listen()
    {
        if (_story is not null)
        {
            Host.ShowStory(_story);
            return;
        }
        string key = $"inn:{_location.LocationId}:rumor";
        if (_session.Flags.ContainsKey(key)) _message = "Bu gece yeni bir söylenti duymadın.";
        else
        {
            _session.Flags[key] = "heard";
            string[] events =
            [
                "İki kervancı yakınlardaki güvenli geçidi tartışıyor. Günlüğüne rota notu düştün.",
                "Kapüşonlu bir yolcu, kalenin muhafız değişim saatini fısıldıyor.",
                "Hancı kayıp bir tüccardan söz ediyor; bu daha sonra göreve dönüşebilir.",
                "Köşedeki ozan bölgedeki yaratıkların zayıflığını anlatıyor."
            ];
            _message = events[Math.Abs(_location.LocationId.GetHashCode()) % events.Length];
            _session.Flags[$"knowledge:inn:{_location.LocationId}"] = "1";
            Host.Autosave();
        }
        Invalidate();
    }

    private void Talk()
    {
        _message = "Hancı: Oda istersen ücretini şimdi ödersin. Söylenti arıyorsan önce etrafı dinle.";
        Invalidate();
    }

    public override void HandleKey(Keys key) { if (key == Keys.Escape) Host.ShowLocation(_location); }

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get("assets/final-production-v5/scenes/settlements/04-inn-tavern-interior-v1.png"), CanvasRectangle);
        UiTheme.Shade(g, CanvasRectangle, 55);
        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, new Rectangle(28, 18, 744, 80));
        UiTheme.DrawText(g, $"HAN · {_location.Name}", UiTheme.Title, UiTheme.Gold, 52, 34);
        UiTheme.DrawTextCentered(g, $"ALTIN {_session.Player.Gold}", UiTheme.Heading, UiTheme.Gold, new Rectangle(610, 35, 130, 40));
        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, new Rectangle(42, 472, 716, 84));
        UiTheme.DrawWrappedCentered(g, _message, UiTheme.Body, UiTheme.Text, new Rectangle(62, 490, 676, 48));
        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, new Rectangle(42, 558, 716, 92));
    }
}
