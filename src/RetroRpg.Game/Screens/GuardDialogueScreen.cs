using RetroRpg.Core.Combat;
using RetroRpg.Core.Game;
using RetroRpg.Core.World;

namespace RetroRpg.Game.Screens;

internal sealed class GuardDialogueScreen : GameScreen
{
    private readonly GameSession _session;
    private readonly WorldLocation _location;
    private readonly LocationService _service;
    private readonly DiceRoller _dice = new();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 90 };
    private readonly List<PixelButton> _approachButtons = [];
    private readonly PixelButton _continue;
    private D20Roll? _roll;
    private AccessApproach? _approach;
    private int _ticks;
    private bool _success;
    private string _message;

    public GuardDialogueScreen(MainGameForm host, GameSession session, WorldLocation location, LocationService service) : base(host)
    {
        _session = session;
        _location = location;
        _service = service;
        _message = service == LocationService.WarRoom
            ? "Muhafız savaş odasının yalnız yetkililere açık olduğunu söylüyor."
            : "Nöbetçi kışlaya sivillerin giremeyeceğini söylüyor.";
        _timer.Tick += (_, _) => Tick();
        for (int i = 0; i < AccessApproachCatalog.All.Count; i++)
        {
            AccessApproach approach = AccessApproachCatalog.All[i];
            var button = AddButton(new PixelButton(new Rectangle(110, 470 + i * 64, 270, 52), approach.Label, () => Attempt(approach))
            { Skin = PixelButton.MedievalLarge, Font = UiTheme.Small });
            button.Enabled = approach.CanAttempt(_session.Player, service);
            _approachButtons.Add(button);
        }
        AddButton(new PixelButton(new Rectangle(420, 598, 270, 52), "GERİ DÖN", () => Host.ShowLocation(_location))
        { Skin = PixelButton.MedievalLarge });
        _continue = AddButton(new PixelButton(new Rectangle(420, 470, 270, 72), "İÇERİ GİR", EnterService)
        { Skin = PixelButton.MedievalLarge, Visible = false });
    }

    private void Attempt(AccessApproach approach)
    {
        if (_roll is not null || _success) return;
        _approach = approach;
        foreach (PixelButton button in _approachButtons) button.Enabled = false;
        if (approach.AutomaticSuccess(_session.Player, _service))
        {
            Resolve(true, "Kimliğin kabul edildi; muhafız kenara çekildi.");
            return;
        }
        _roll = _dice.RollD20(approach.Bonus(_session.Player));
        _ticks = 22;
        _timer.Start();
        Invalidate();
    }

    private void Tick()
    {
        _ticks--;
        if (_ticks > 0) { Invalidate(); return; }
        _timer.Stop();
        D20Roll roll = _roll!;
        bool success = roll.IsNaturalTwenty || (!roll.IsNaturalOne && roll.Total >= AccessApproachCatalog.Difficulty(_service));
        Resolve(success, success ? "Muhafız ikna oldu ve geçiş izni verdi." : "Muhafız reddetti; aynı yaklaşımı şimdi tekrar deneyemezsin.");
    }

    private void Resolve(bool success, string message)
    {
        _success = success;
        _message = message;
        _continue.Visible = success;
        if (success)
        {
            _session.Flags[AccessKey] = "granted";
            Host.Autosave();
        }
        Invalidate();
    }

    private string AccessKey => $"access:{_location.LocationId}:{_service}";

    private void EnterService()
    {
        if (_success) Host.ShowLocation(_location, _service, "Muhafızların izniyle içeri girdin.");
    }

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get(_location.SceneAsset), CanvasRectangle);
        UiTheme.Shade(g, CanvasRectangle, 145);
        UiTheme.DrawPanel(g, Images, PanelStyle.StoneWood, new Rectangle(70, 90, 660, 610));
        UiTheme.DrawTextCentered(g, _service == LocationService.WarRoom ? "SAVAŞ ODASI MUHAFIZI" : "KIŞLA NÖBETÇİSİ",
            UiTheme.Title, UiTheme.Gold, new Rectangle(110, 120, 580, 50));
        UiTheme.DrawWrappedCentered(g, _message, UiTheme.Body, _success ? UiTheme.Success : UiTheme.Text, new Rectangle(140, 190, 520, 86));
        if (_approach is not null)
            UiTheme.DrawWrappedCentered(g, _approach.Description, UiTheme.Small, UiTheme.Muted, new Rectangle(150, 285, 500, 45));

        int dc = AccessApproachCatalog.Difficulty(_service);
        if (_roll is not null)
        {
            bool reveal = _ticks <= 5;
            UiTheme.DrawImageFit(g, Images.Get("assets/final-production-v2/combat/dice/runtime-256/d20-icon-v1.png"), new Rectangle(320, 330, 160, 120));
            if (reveal || _ticks <= 0)
                UiTheme.DrawDiceValue(g, new Rectangle(355, 360, 90, 55), _roll.RawValue);
            else UiTheme.DrawDiceQuestion(g, new Rectangle(355, 360, 90, 55));
            UiTheme.DrawTextCentered(g, $"d20 {_roll.Bonus:+#;-#;0} · ZORLUK {dc}", UiTheme.Small, UiTheme.Gold, new Rectangle(290, 430, 220, 30));
        }
        else if (!_success)
            UiTheme.DrawTextCentered(g, $"İKNA: KARİZMA · GÖZDAĞI: GÜÇ · ZORLUK {dc}", UiTheme.Small, UiTheme.Gold, new Rectangle(190, 400, 420, 30));
    }

    protected override void Dispose(bool disposing) { if (disposing) _timer.Dispose(); base.Dispose(disposing); }
}
