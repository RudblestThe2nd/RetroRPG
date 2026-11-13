using System.Globalization;
using RetroRpg.Core.Game;
using RetroRpg.Core.Story;

namespace RetroRpg.Game.Screens;

// Final kararı: önce Vaelor'un kaderi, sonra dört finalden biri, en son endings-v1 karesiyle dinamik epilog.
// Kilitli final pasif görünür ve gerekçesini gösterir; seçim tek bir zara bağlı değildir.
internal sealed class EndingScreen : GameScreen
{
    private static readonly CultureInfo Turkish = new("tr-TR");
    private static readonly Rectangle TitleBar = new(140, 14, 520, 48);
    private static readonly Rectangle TextBox = new(16, 470, 768, 322);

    private readonly GameSession _session;
    private readonly List<PixelButton> _fateButtons = [];
    private readonly List<PixelButton> _endingButtons = [];
    private readonly PixelButton _menuButton;
    private readonly PixelButton _mapButton;
    private VaelorState? _fate;
    private EndingStoryEvent? _chosen;
    private string _epilogue = "";

    public EndingScreen(MainGameForm host, GameSession session) : base(host)
    {
        _session = session;
        (VaelorState Fate, string Label, string Rule)[] fates =
        [
            (VaelorState.Killed, "ÖLDÜR", "Zar yok"),
            (VaelorState.Persuaded, "İKNA ET", EndingRules.CanPersuadeVaelor(session.Story) ? "Zar yok" : "Tanık veya uzlaşma gerekir"),
            (VaelorState.Imprisoned, "HAPSET", "Zar yok")
        ];
        for (int i = 0; i < fates.Length; i++)
        {
            VaelorState fate = fates[i].Fate;
            _fateButtons.Add(AddButton(new PixelButton(new Rectangle(42 + i * 243, 600, 230, 56), $"{fates[i].Label}\n{fates[i].Rule}", () => ChooseFate(fate))
            {
                Skin = PixelButton.MedievalLarge,
                Font = UiTheme.Small,
                Enabled = fate != VaelorState.Persuaded || EndingRules.CanPersuadeVaelor(session.Story)
            }));
        }
        for (int i = 0; i < StoryCatalog.Endings.Count; i++)
        {
            EndingStoryEvent ending = StoryCatalog.Endings[i];
            bool unlocked = ending.IsUnlocked(session.Story);
            _endingButtons.Add(AddButton(new PixelButton(new Rectangle(i % 2 == 0 ? 42 : 408, 590 + i / 2 * 64, 350, 56),
                $"{ending.Title}\n{(unlocked ? "Açık" : ending.RequirementText)}", () => ChooseEnding(ending))
            { Skin = PixelButton.MedievalLarge, Font = UiTheme.Small, Enabled = unlocked, Visible = false }));
        }
        _menuButton = AddButton(new PixelButton(new Rectangle(120, 724, 260, 52), "ANA MENÜ", () => Host.ShowMainMenu())
        { Skin = PixelButton.MedievalLarge, Visible = false });
        _mapButton = AddButton(new PixelButton(new Rectangle(420, 724, 260, 52), "HARİTADA KAL", () => Host.ShowMap())
        { Skin = PixelButton.MedievalLarge, Visible = false });
    }

    private void ChooseFate(VaelorState fate)
    {
        _fate = fate;
        foreach (PixelButton button in _fateButtons) button.Visible = false;
        foreach (PixelButton button in _endingButtons) button.Visible = true;
        Invalidate();
    }

    private void ChooseEnding(EndingStoryEvent ending)
    {
        if (_fate is not VaelorState fate || !ending.IsUnlocked(_session.Story)) return;
        _epilogue = StoryDirector.ChooseEnding(_session, fate, ending).Text;
        _chosen = ending;
        Host.Autosave();
        foreach (PixelButton button in _endingButtons) button.Visible = false;
        _menuButton.Visible = _mapButton.Visible = true;
        Invalidate();
    }

    private static string FateName(VaelorState fate) => fate switch
    {
        VaelorState.Killed => "öldürüldü",
        VaelorState.Persuaded => "ikna edildi",
        _ => "kendi kulesine hapsedildi"
    };

    protected override void Draw(Graphics g)
    {
        string background = _chosen is not null
            ? Host.Cinematics.FramesFor(_chosen.Scene)[0]
            : Host.Cinematics.Get("fate-lens-v1").Frames[^1];
        UiTheme.DrawImage(g, Images.Get(background), CanvasRectangle);
        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, TitleBar);
        string title = _chosen?.Title ?? (_fate is null ? "VAELOR DİZ ÇÖKTÜ" : "YOLLARIN KADERİ");
        UiTheme.DrawTextCentered(g, title.ToUpper(Turkish), UiTheme.Heading, UiTheme.Gold, TitleBar);

        Rectangle box = _chosen is null ? TextBox : new Rectangle(16, 540, 768, 252);
        UiTheme.DrawPanel(g, Images, PanelStyle.StoneWood, box);
        if (_chosen is not null)
            UiTheme.DrawWrappedFit(g, _epilogue, UiTheme.Body, UiTheme.Small, UiTheme.Text, new Rectangle(56, 572, 688, 142));
        else if (_fate is null)
            UiTheme.DrawWrappedFit(g, "Vaelor Mercek'in önünde diz çöktü; ikinci biçimi dağılıyor. Önce onun kaderine karar ver. " +
                $"Kurtarılan tanık: {(_session.Story.PrisonerRescued ? "var" : "yok")} · Uzlaşmacı taş kararı: {EndingRules.CompromiseCount(_session.Story)}/3",
                UiTheme.Body, UiTheme.Small, UiTheme.Text, new Rectangle(56, 502, 688, 86));
        else
            UiTheme.DrawWrappedFit(g, $"Vaelor {FateName(_fate.Value)}. Şimdi Kader Merceği'nin ve yolların kaderini seç.\n" +
                $"Yüksek güvenli bölge: {EndingRules.HighTrustRegions(_session.Story)} · Uzlaşmacı taş kararı: {EndingRules.CompromiseCount(_session.Story)}/3",
                UiTheme.Body, UiTheme.Small, UiTheme.Text, new Rectangle(56, 500, 688, 80));
    }

    // Yalnız --screenshots QA akışı kullanır.
    internal void ChooseFateForQa(VaelorState fate) => ChooseFate(fate);
    internal void ChooseEndingForQa(string endingId) => ChooseEnding(StoryCatalog.Endings.Single(x => x.EndingId == endingId));
}
