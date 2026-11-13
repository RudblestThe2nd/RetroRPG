using RetroRpg.Core.Game;
using RetroRpg.Core.Items;
using RetroRpg.Core.Story;

namespace RetroRpg.Game.Screens;

// Üç hikâye yuvası. İlk yuva Kader Yolları: Göz Kulesi'dir; seçimden sonra harita değil prolog açılır.
internal sealed class StorySelectionScreen : GameScreen
{
    private static readonly Rectangle[] Cards = [new(92, 212, 190, 330), new(305, 212, 190, 330), new(518, 212, 190, 330)];
    private readonly CharacterCreationDraft _draft;
    private int _selected;

    public StorySelectionScreen(MainGameForm host, CharacterCreationDraft draft) : base(host)
    {
        _draft = draft;
        for (int i = 0; i < Cards.Length; i++)
        {
            int index = i;
            AddButton(new PixelButton(Cards[i], "", () => Select(index)) { Enabled = !StorySlotCatalog.Slots[i].IsLocked });
        }
        AddButton(new PixelButton(new Rectangle(105, 690, 220, 58), "GERİ", () => Host.ShowCharacterSummary(_draft)) { Skin = PixelButton.MedievalLarge });
        AddButton(new PixelButton(new Rectangle(475, 690, 220, 58), "MACERAYA BAŞLA", Start) { Skin = PixelButton.MedievalLarge });
    }

    private void Select(int index) { _selected = index; Invalidate(); }

    private void Start()
    {
        var player = _draft.CreatePlayer();
        string storySlot = StorySlotCatalog.Slots[_selected].Id;
        ItemCatalog.GiveStartingKit(player, storySlot);
        var session = new GameSession(player) { PortraitIndex = _draft.PortraitIndex, StorySlotId = storySlot };
        session.Flags["appearance:hair"] = _draft.HairStyle.ToString();
        session.Flags["appearance:facial"] = _draft.FacialStyle.ToString();
        // Hikâye seçimi autosave alınır; StartGame prologu açar.
        Host.StartGame(session);
    }

    public override void HandleKey(Keys key)
    {
        if (key == Keys.Escape) Host.ShowCharacterSummary(_draft);
        else if (key == Keys.Enter) Start();
    }

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get(AssetPath("ui.main-menu.background.wood-red-v1")), CanvasRectangle);
        UiTheme.DrawTextCentered(g, "BAŞLANGIÇ HİKÂYESİ", UiTheme.Title, UiTheme.Gold, new Rectangle(170, 88, 460, 48));
        for (int i = 0; i < Cards.Length; i++)
        {
            StoryDefinition slot = StorySlotCatalog.Slots[i];
            DrawCard(g, Cards[i], slot.Title.ToUpper(new System.Globalization.CultureInfo("tr-TR")),
                slot.IsLocked ? "HENÜZ HAZIR DEĞİL" : "AÇIK", !slot.IsLocked, _selected == i);
        }
        UiTheme.DrawWrappedCentered(g, StorySlotCatalog.Slots[_selected].Description, UiTheme.Small, UiTheme.Text, new Rectangle(160, 584, 480, 44));
    }

    private void DrawCard(Graphics g, Rectangle area, string title, string state, bool enabled, bool selected)
    {
        UiTheme.DrawPanel(g, Images, PanelStyle.StoneWood, area);
        UiTheme.DrawWrappedCentered(g, title, UiTheme.Heading, enabled ? UiTheme.Gold : UiTheme.Muted, new Rectangle(area.X + 14, area.Y + 26, area.Width - 28, 56));
        UiTheme.DrawImageFit(g, Images.Get(enabled ? "items/item-pack-02/icons/quest-relics/01-eye-tower-key.png" : "ui/assets/icons/system/icon-lock_normal.png"), new Rectangle(area.X + 45, area.Y + 100, 100, 100));
        UiTheme.DrawTextCentered(g, state, UiTheme.Small, enabled ? UiTheme.Success : UiTheme.Muted, new Rectangle(area.X + 15, area.Bottom - 72, area.Width - 30, 38));
        if (selected)
        {
            using var pen = new Pen(UiTheme.Gold, 4);
            g.DrawRectangle(pen, Rectangle.Inflate(area, 3, 3));
        }
    }
}
