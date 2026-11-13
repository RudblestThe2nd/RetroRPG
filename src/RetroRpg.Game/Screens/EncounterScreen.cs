using RetroRpg.Core.Characters;
using RetroRpg.Core.Combat;
using RetroRpg.Core.Game;
using RetroRpg.Core.Items;
using RetroRpg.Core.World;

namespace RetroRpg.Game.Screens;

// WorldEncounter taban sınıfının tüm alt türlerini aynı akışla oynatır.
internal sealed class EncounterScreen : GameScreen
{
    private readonly GameSession _session;
    private readonly WorldEncounter _encounter;
    private readonly DiceRoller _dice = new();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 90 };
    private readonly PixelButton[] _choiceButtons;
    private readonly PixelButton _leave;
    private D20Roll? _roll;
    private EncounterChoice? _selectedChoice;
    private int _ticks;
    private string _result = "";

    public EncounterScreen(MainGameForm host, GameSession session, WorldEncounter encounter) : base(host)
    {
        _session = session;
        _encounter = encounter;
        _timer.Tick += (_, _) => AnimateRoll();
        _choiceButtons = encounter.Choices.Select((choice, index) =>
            AddButton(new PixelButton(new Rectangle(32 + index * 254, 714, 240, 58), choice.Label, () => BeginCheck(choice))
            { Skin = PixelButton.MedievalLarge, Font = UiTheme.Small })).ToArray();
        _leave = AddButton(new PixelButton(new Rectangle(540, 714, 228, 58), "UZAKLAŞ", ReturnToMap)
        { Skin = PixelButton.MedievalLarge });
    }

    private void BeginCheck(EncounterChoice choice)
    {
        if (_roll is not null || _result.Length > 0) return;
        _selectedChoice = choice;
        _roll = _dice.RollD20(WorldEncounter.CheckBonus(_session.Player, choice.CheckStat));
        _ticks = 24;
        foreach (PixelButton button in _choiceButtons) button.Enabled = false;
        _leave.Enabled = false;
        _timer.Start();
        Invalidate();
    }

    private void AnimateRoll()
    {
        _ticks--;
        if (_ticks > 0) { Invalidate(); return; }
        _timer.Stop();
        D20Roll roll = _roll!;
        EncounterChoice choice = _selectedChoice!;
        bool success = roll.IsNaturalTwenty || (!roll.IsNaturalOne && roll.Total >= choice.Difficulty);
        EncounterOutcome outcome = success ? choice.Success : choice.Failure;
        Apply(outcome);
        _result = $"{(success ? "BAŞARI" : "BAŞARISIZLIK")}: {outcome.Summary}";
        _session.Flags[$"encounter:{_encounter.EncounterId}"] = $"{choice.Id}:{(success ? "success" : "failure")}";
        Host.Autosave();
        _leave.Text = _session.Player.IsAlive ? "YOLUNA DEVAM ET" : "DEVAM ET";
        _leave.Bounds = new Rectangle(270, 714, 260, 58);
        _leave.Enabled = true;
        foreach (PixelButton button in _choiceButtons) button.Visible = false;
        Invalidate();
    }

    private void Apply(EncounterOutcome outcome)
    {
        _session.Player.Gold = Math.Max(0, _session.Player.Gold + outcome.GoldDelta);
        if (outcome.HealthDelta > 0) _session.Player.Heal(outcome.HealthDelta);
        else if (outcome.HealthDelta < 0) _session.Player.ReceiveDamage(-outcome.HealthDelta);
        if (outcome.AddPotion)
            _session.Player.Inventory.Add(new Consumable("İyileştirme İksiri", 10, .5, 6));
        if (outcome.Stat is BackgroundStat stat && outcome.StatDelta != 0)
            ChangeStat(stat, outcome.StatDelta);
        if (outcome.UnlockFlag is string flag)
            _session.ApplyUnlock(flag);
        if (outcome.ItemId is string itemId)
            _session.Player.Inventory.Add(CreateRewardItem(itemId));
    }

    private void ChangeStat(BackgroundStat stat, int amount)
    {
        var stats = _session.Player.Stats;
        switch (stat)
        {
            case BackgroundStat.Strength: stats.Strength += amount; break;
            case BackgroundStat.Dexterity: stats.Dexterity += amount; break;
            case BackgroundStat.Constitution: stats.Constitution += amount; break;
            case BackgroundStat.Intelligence: stats.Intelligence += amount; break;
            case BackgroundStat.Wisdom: stats.Wisdom += amount; break;
            case BackgroundStat.Charisma: stats.Charisma += amount; break;
        }
    }

    private static Item CreateRewardItem(string id) => id switch
    {
        "ration" => new Consumable("Yol Azığı", 4, .4, 3),
        "honey" => new Consumable("Yabani Bal", 5, .3, 4),
        "herb" => new Consumable("Şifalı Ot", 6, .2, 4),
        "lockpicks" => new QuestItem("Maymuncuk Takımı", "tool:lockpicks"),
        "torch" => new QuestItem("Maceracı Meşalesi", "tool:torch"),
        _ => new QuestItem("Keşif Hatırası", $"encounter:{id}")
    };

    private void ReturnToMap()
    {
        if (!_session.Player.IsAlive) Host.ShowDeath(_encounter.Name);
        else Host.ShowMap();
    }

    public override void HandleKey(Keys key) { if (key == Keys.Escape && _leave.Enabled) ReturnToMap(); }

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get(_encounter.AssetPath), CanvasRectangle);
        UiTheme.DrawPanel(g, Images, PanelStyle.PurpleHeader, new Rectangle(26, 448, 748, 254));
        UiTheme.DrawTextCentered(g, _encounter.Name.ToUpperInvariant(), UiTheme.Title, UiTheme.Gold,
            new Rectangle(55, 460, 690, 38));
        UiTheme.DrawTextCentered(g, _encounter.Description, UiTheme.Small, UiTheme.Text, new Rectangle(60, 505, 680, 36));
        if (_result.Length == 0)
        {
            DrawChoice(g, _encounter.Choices[0], 550);
            DrawChoice(g, _encounter.Choices[1], 616);
        }
        else UiTheme.DrawWrappedCentered(g, _result, UiTheme.Body,
            _session.Player.IsAlive ? UiTheme.Success : UiTheme.Danger, new Rectangle(80, 565, 640, 88));
    }

    private static void DrawChoice(Graphics g, EncounterChoice choice, int y)
    {
        string rule = $"{choice.Label} · d20 + {EncounterReport.StatName(choice.CheckStat)} · Zorluk {choice.Difficulty}";
        UiTheme.DrawText(g, rule, UiTheme.Small, UiTheme.Gold, 66, y);
        UiTheme.DrawWrappedCentered(g, $"Başarı: {choice.Success.Summary}", UiTheme.Small, UiTheme.Success, new Rectangle(58, y + 18, 322, 44));
        UiTheme.DrawWrappedCentered(g, $"Başarısızlık: {choice.Failure.Summary}", UiTheme.Small, UiTheme.Danger, new Rectangle(420, y + 18, 322, 44));
    }

    protected override void DrawTop(Graphics g)
    {
        if (_roll is null || (_ticks <= 0 && _result.Length == 0)) return;
        if (_ticks <= 0) return;
        UiTheme.Shade(g, CanvasRectangle, 165);
        UiTheme.DrawImage(g, Images.Get("assets/final-production-v5/ui/modals/runtime-800/dice-result-panel-v1.png"),
            new Rectangle(170, 115, 460, 570));
        bool reveal = _ticks <= 6;
        string die = reveal ? "assets/final-production-v2/combat/dice/runtime-256/d20-icon-v1.png"
            : $"assets/final-production-v2/combat/dice/d20-roll-v1/d20-roll-{_ticks % 8:00}-v1.png";
        UiTheme.DrawImageFit(g, Images.Get(die), new Rectangle(300, 245, 200, 200));
        if (reveal) UiTheme.DrawDiceValue(g, new Rectangle(340, 310, 120, 70), _roll.RawValue);
        else UiTheme.DrawDiceQuestion(g, new Rectangle(340, 310, 120, 70));
        UiTheme.DrawTextCentered(g, reveal ? $"TOPLAM {_roll.Total} / ZORLUK {_selectedChoice!.Difficulty}" : "ZAR ATILIYOR...", UiTheme.Heading,
            reveal ? UiTheme.Success : UiTheme.Gold, new Rectangle(245, 468, 310, 45));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _timer.Dispose();
        base.Dispose(disposing);
    }
}
