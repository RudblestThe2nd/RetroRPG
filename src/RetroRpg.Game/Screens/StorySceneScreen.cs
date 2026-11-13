using System.Globalization;
using RetroRpg.Core.Combat;
using RetroRpg.Core.Game;
using RetroRpg.Core.Story;
using RetroRpg.Core.World;

namespace RetroRpg.Game.Screens;

internal enum StoryScenePhase { Intro, Choosing, Rolling, Outro, Result }

// Hikâye olaylarını oynatır: manifestteki 800x800 PNG kareleri sırayla, altta ahşap metin kutusu,
// seçenekler, d20 ve sonuç. GIF kullanılmaz; dizi loop etmez ve seçim/son karede bekler.
// Tıklama veya Enter bir sonraki kareye geçer.
internal sealed class StorySceneScreen : GameScreen
{
    private const int MinimumPageMilliseconds = 2600;
    private const int RollTicks = 24;
    private static readonly CultureInfo Turkish = new("tr-TR");
    private static readonly Rectangle TitleBar = new(140, 14, 520, 48);
    private static readonly Rectangle TextBox = new(16, 540, 768, 252);

    private readonly GameSession _session;
    private readonly StoryEvent _event;
    private readonly IReadOnlyList<string> _frames;
    private readonly int _fps;
    private readonly IDiceRoller _dice = new DiceRoller();
    private readonly System.Windows.Forms.Timer _frameTimer = new();
    private readonly System.Windows.Forms.Timer _rollTimer = new() { Interval = 90 };
    private readonly List<(StoryChoice Choice, PixelButton Button)> _choiceButtons = [];
    private readonly PixelButton _continueButton;
    private readonly PixelButton? _postponeButton;
    private StoryScenePhase _phase = StoryScenePhase.Intro;
    private int _frame;
    private StoryChoice? _choice;
    private D20Roll? _roll;
    private int _rollTicks;
    private StoryOutcome? _outcome;

    public StorySceneScreen(MainGameForm host, GameSession session, StoryEvent storyEvent) : base(host)
    {
        _session = session;
        _event = storyEvent;
        _frames = host.Cinematics.FramesFor(storyEvent.Scene);
        _fps = storyEvent.Scene.SequenceId is string id ? host.Cinematics.Get(id).Fps : 1;
        _frameTimer.Tick += (_, _) => AdvanceFrame();
        _rollTimer.Tick += (_, _) => AnimateRoll();

        IReadOnlyList<StoryChoice> choices = storyEvent.Choices;
        for (int i = 0; i < choices.Count; i++)
        {
            StoryChoice choice = choices[i];
            var button = AddButton(new PixelButton(ChoiceBounds(i, choices.Count), ChoiceText(choice), () => Choose(choice))
            { Skin = PixelButton.MedievalLarge, Font = UiTheme.Small, Visible = false });
            _choiceButtons.Add((choice, button));
        }
        _continueButton = AddButton(new PixelButton(new Rectangle(250, 700, 300, 52), "DEVAM ET", Continue)
        { Skin = PixelButton.MedievalLarge, Visible = false });
        if (storyEvent.CanPostpone)
            _postponeButton = AddButton(new PixelButton(new Rectangle(670, 14, 118, 48), "UZAKLAŞ", Postpone) { Skin = PixelButton.Medium });

        if (ChoiceFrame == 0) _phase = StoryScenePhase.Choosing;
        else StartFrameTimer();
        UpdateButtons();
    }

    private int ChoiceFrame => Math.Min(_event.Scene.ChoiceFrame, _frames.Count - 1);

    private static Rectangle ChoiceBounds(int index, int count)
    {
        const int width = 350, height = 52;
        int top = count <= 2 ? 680 : 640;
        bool alone = count == 1 || (count % 2 == 1 && index == count - 1);
        int x = alone ? 225 : index % 2 == 0 ? 42 : 408;
        return new Rectangle(x, top + index / 2 * 60, width, height);
    }

    // Zar kontrolü olan olaylarda kuralı, kilitli seçenekte gerekçeyi ikinci satırda gösterir.
    private string ChoiceText(StoryChoice choice)
    {
        if (!choice.IsAvailable(_session)) return choice.Label + "\n" + choice.RequirementText;
        if (choice.Check is StoryCheck check) return choice.Label + "\n" + RuleText(check);
        return _event is SkillCheckStoryEvent ? choice.Label + "\nZar yok" : choice.Label;
    }

    private string RuleText(StoryCheck check)
    {
        int bonus = SkillCheckStoryEvent.CheckBonus(_session, check);
        string stat = EncounterReport.StatName(check.Stat) +
                      (check.AlternateStat is { } alternate ? "/" + EncounterReport.StatName(alternate) : "");
        return $"d20 {bonus:+#;-#;+0} · {stat} · Zorluk {check.Difficulty}";
    }

    private void StartFrameTimer()
    {
        bool hasText = CurrentPage().Length > 0;
        _frameTimer.Interval = hasText ? Math.Max(1000 / _fps, MinimumPageMilliseconds) : Math.Max(150, 1000 / _fps);
        _frameTimer.Start();
    }

    private void AdvanceFrame()
    {
        _frameTimer.Stop();
        if (_phase == StoryScenePhase.Intro)
        {
            if (_frame < ChoiceFrame) _frame++;
            if (_frame >= ChoiceFrame) _phase = StoryScenePhase.Choosing;
            else StartFrameTimer();
        }
        else if (_phase == StoryScenePhase.Outro)
        {
            // Son kare de kendi anlatımıyla gösterilir; sonuç yazısı ondan sonra gelir.
            if (_frame >= _frames.Count - 1)
            {
                EnterResult();
                return;
            }
            _frame++;
            StartFrameTimer();
        }
        UpdateButtons();
        Invalidate();
    }

    private void Choose(StoryChoice choice)
    {
        if (_phase != StoryScenePhase.Choosing || !choice.IsAvailable(_session)) return;
        _choice = choice;
        if (choice.Check is StoryCheck check)
        {
            // Ekranda gösterilen ve kurala uygulanan ham değer bu tek D20Roll nesnesinden gelir.
            _roll = _dice.RollD20(SkillCheckStoryEvent.CheckBonus(_session, check));
            _rollTicks = RollTicks;
            _phase = StoryScenePhase.Rolling;
            UpdateButtons();
            _rollTimer.Start();
            Invalidate();
            return;
        }
        Resolve();
    }

    private void AnimateRoll()
    {
        _rollTicks--;
        Invalidate();
        if (_rollTicks > 0) return;
        _rollTimer.Stop();
        Resolve();
    }

    private void Resolve()
    {
        _outcome = _event.Resolve(_session, _choice!, _roll);
        // Savaş başlatan seçim sonraki kareleri (taşın uyanışı) oynatmaz; sonuç seçim karesinde kalır.
        if (_choice!.PlaysAfterFrames && _frame < _frames.Count - 1)
        {
            _phase = StoryScenePhase.Outro;
            _frame++;
            UpdateButtons();
            StartFrameTimer();
            return;
        }
        EnterResult();
    }

    // Sonuç yazısı yoksa (yalnız anlatım) doğrudan sıradaki olaya geçilir.
    private void EnterResult()
    {
        _phase = StoryScenePhase.Result;
        if (_outcome is null || _outcome.Summary.Length == 0)
        {
            Host.FinishStory(_event);
            return;
        }
        UpdateButtons();
        Invalidate();
    }

    private void Continue()
    {
        if (_phase == StoryScenePhase.Result) Host.FinishStory(_event);
    }

    private void Postpone()
    {
        if (_phase is not (StoryScenePhase.Intro or StoryScenePhase.Choosing)) return;
        StoryDirector.Postpone(_session);
        Host.ShowMap();
    }

    private void UpdateButtons()
    {
        foreach ((StoryChoice choice, PixelButton button) in _choiceButtons)
        {
            button.Visible = _phase == StoryScenePhase.Choosing;
            button.Enabled = choice.IsAvailable(_session);
        }
        _continueButton.Visible = _phase == StoryScenePhase.Result;
        if (_postponeButton is not null) _postponeButton.Visible = _phase is StoryScenePhase.Intro or StoryScenePhase.Choosing;
        Invalidate();
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button == MouseButtons.Left && ButtonAt(e.Location) is null && _phase is StoryScenePhase.Intro or StoryScenePhase.Outro)
            AdvanceFrame();
    }

    public override void HandleKey(Keys key)
    {
        if (key is Keys.Enter or Keys.Space)
        {
            if (_phase is StoryScenePhase.Intro or StoryScenePhase.Outro) AdvanceFrame();
            else if (_phase == StoryScenePhase.Choosing && _choiceButtons.Count == 1) Choose(_choiceButtons[0].Choice);
            else if (_phase == StoryScenePhase.Result) Continue();
        }
        else if (key == Keys.Escape && _postponeButton is not null) Postpone();
    }

    private string CurrentPage()
    {
        if (_phase == StoryScenePhase.Outro || (_phase == StoryScenePhase.Result && _outcome is null))
        {
            int index = _frame - ChoiceFrame - 1;
            return index >= 0 && index < _event.AfterPages.Count ? StoryText.Format(_session, _event.AfterPages[index]) : "";
        }
        if (_event.Pages.Count == 0) return "";
        return StoryText.Format(_session, _event.Pages[Math.Min(_frame, _event.Pages.Count - 1)]);
    }

    private (string Text, Color Color) CurrentText()
    {
        if (_phase != StoryScenePhase.Result || _outcome is null) return (CurrentPage(), UiTheme.Text);
        string prefix = _outcome.Roll is D20Roll roll
            ? $"{(_outcome.Success ? "BAŞARI" : "BAŞARISIZLIK")} · d20 {roll.RawValue} {roll.Bonus:+#;-#;+0} = {roll.Total}\n"
            : "";
        return (prefix + _outcome.Summary, _outcome.Success ? UiTheme.Success : UiTheme.Gold);
    }

    private Rectangle NarrationArea() => _phase switch
    {
        StoryScenePhase.Choosing or StoryScenePhase.Rolling when _choiceButtons.Count > 2 => new Rectangle(56, 572, 688, 62),
        StoryScenePhase.Choosing or StoryScenePhase.Rolling => new Rectangle(56, 574, 688, 100),
        StoryScenePhase.Result => new Rectangle(56, 574, 688, 118),
        _ => new Rectangle(56, 576, 688, 144)
    };

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get(_frames[_frame]), CanvasRectangle);
        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, TitleBar);
        UiTheme.DrawTextCentered(g, _event.Title.ToUpper(Turkish), UiTheme.Heading, UiTheme.Gold, TitleBar);
        UiTheme.DrawPanel(g, Images, PanelStyle.StoneWood, TextBox);
        (string text, Color color) = CurrentText();
        UiTheme.DrawWrappedFit(g, text, UiTheme.Body, UiTheme.Small, color, NarrationArea());
        if (_phase is StoryScenePhase.Intro or StoryScenePhase.Outro)
            UiTheme.DrawTextCentered(g, $"KARE {_frame + 1}/{_frames.Count} · TIKLA VEYA ENTER: SONRAKİ", UiTheme.Small, UiTheme.Muted,
                new Rectangle(56, 728, 688, 24));
    }

    protected override void DrawTop(Graphics g)
    {
        if (_phase != StoryScenePhase.Rolling || _roll is null || _choice?.Check is not StoryCheck check) return;
        UiTheme.Shade(g, CanvasRectangle, 165);
        UiTheme.DrawImage(g, Images.Get("assets/final-production-v5/ui/modals/runtime-800/dice-result-panel-v1.png"),
            new Rectangle(170, 115, 460, 570));
        bool reveal = _rollTicks <= 6;
        string die = reveal ? "assets/final-production-v2/combat/dice/runtime-256/d20-icon-v1.png"
            : $"assets/final-production-v2/combat/dice/d20-roll-v1/d20-roll-{_rollTicks % 8:00}-v1.png";
        UiTheme.DrawImageFit(g, Images.Get(die), new Rectangle(300, 245, 200, 200));
        if (reveal) UiTheme.DrawDiceValue(g, new Rectangle(340, 310, 120, 70), _roll.RawValue);
        else UiTheme.DrawDiceQuestion(g, new Rectangle(340, 310, 120, 70));
        UiTheme.DrawTextCentered(g, _choice.Label.ToUpper(Turkish), UiTheme.Heading, UiTheme.Gold, new Rectangle(220, 190, 360, 40));
        UiTheme.DrawTextCentered(g, reveal ? $"TOPLAM {_roll.Total} / ZORLUK {check.Difficulty}" : "ZAR ATILIYOR...", UiTheme.Heading,
            reveal ? UiTheme.Success : UiTheme.Gold, new Rectangle(245, 468, 310, 45));
    }

    // Yalnız --screenshots QA akışı kullanır.
    internal void ShowChoiceForQa()
    {
        _frameTimer.Stop();
        _frame = ChoiceFrame;
        _phase = StoryScenePhase.Choosing;
        UpdateButtons();
    }

    internal void ShowRollForQa(int choiceIndex, int raw)
    {
        ShowChoiceForQa();
        _choice = _choiceButtons[choiceIndex].Choice;
        _roll = new FixedDiceRoller(raw).RollD20(SkillCheckStoryEvent.CheckBonus(_session, _choice.Check!));
        _rollTicks = 3;
        _phase = StoryScenePhase.Rolling;
        UpdateButtons();
    }

    internal void ShowResultForQa(int choiceIndex, int raw)
    {
        ShowChoiceForQa();
        _choice = _choiceButtons[choiceIndex].Choice;
        _roll = _choice.Check is StoryCheck check ? new FixedDiceRoller(raw).RollD20(SkillCheckStoryEvent.CheckBonus(_session, check)) : null;
        _outcome = _event.Resolve(_session, _choice, _roll);
        _frame = _choice.PlaysAfterFrames ? _frames.Count - 1 : ChoiceFrame;
        _phase = StoryScenePhase.Result;
        UpdateButtons();
    }

    internal void ShowFrameForQa(int frame)
    {
        _frameTimer.Stop();
        _frame = Math.Clamp(frame, 0, _frames.Count - 1);
        _phase = _frame > ChoiceFrame ? StoryScenePhase.Outro : _frame == ChoiceFrame ? StoryScenePhase.Choosing : StoryScenePhase.Intro;
        UpdateButtons();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _frameTimer.Dispose();
            _rollTimer.Dispose();
        }
        base.Dispose(disposing);
    }
}
