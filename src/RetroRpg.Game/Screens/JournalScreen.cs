using System.Globalization;
using RetroRpg.Core.Common;
using RetroRpg.Core.Game;
using RetroRpg.Core.Story;

namespace RetroRpg.Game.Screens;

internal enum JournalFilter { Active, Finished, All }

// Günlük, StoryProgress içindeki gerçek görevleri gösterir: sol listede başlıklar, parşömende açıklama,
// aktif hedef ve not; alt panelde hedefler, sağ altta üç Yol Taşı'nın durumu.
internal sealed class JournalScreen : GameScreen
{
    private const int RowsPerPage = 7;
    private static readonly CultureInfo Turkish = new("tr-TR");
    private static readonly Color Ink = Color.FromArgb(65, 48, 35);
    private readonly GameSession _session;
    private readonly List<PixelButton> _rows = [];
    private readonly PixelButton[] _tabs;
    private readonly PixelButton _previous;
    private readonly PixelButton _next;
    private JournalFilter _filter = JournalFilter.Active;
    private int _page;
    private int _selected;

    public JournalScreen(MainGameForm host, GameSession session) : base(host)
    {
        _session = session;
        for (int i = 0; i < RowsPerPage; i++)
        {
            int index = i;
            _rows.Add(AddButton(new PixelButton(new Rectangle(90, 215 + i * 59, 214, 45), "", () => { _selected = _page * RowsPerPage + index; Invalidate(); })));
        }
        string[] tabNames = ["AKTİF", "BİTEN", "TÜMÜ"];
        _tabs = new PixelButton[tabNames.Length];
        for (int i = 0; i < tabNames.Length; i++)
        {
            var filter = (JournalFilter)i;
            _tabs[i] = AddButton(new PixelButton(new Rectangle(93 + i * 73, 140, 62, 60), "", () => SetFilter(filter)));
        }
        _previous = AddButton(new PixelButton(new Rectangle(62, 688, 156, 48), "ÖNCEKİ", () => Turn(-1)) { Skin = PixelButton.Medium });
        _next = AddButton(new PixelButton(new Rectangle(237, 688, 156, 48), "SONRAKİ", () => Turn(1)) { Skin = PixelButton.Medium });
        AddButton(new PixelButton(new Rectangle(584, 688, 156, 48), "HARİTAYA DÖN", () => Host.ShowMap()) { Skin = PixelButton.Medium, Font = UiTheme.Small });
        RefreshRows();
    }

    // Aktif görevler önce ana görevler gelecek biçimde, katalog sırasıyla listelenir.
    private IReadOnlyList<Quest> ListedQuests => _session.Story.Quests.Values.Where(x => _filter switch
    {
        JournalFilter.Active => x.Status == QuestStatus.Active,
        JournalFilter.Finished => x.Status is QuestStatus.Completed or QuestStatus.Failed,
        _ => x.Status != QuestStatus.NotStarted
    }).ToArray();

    private void SetFilter(JournalFilter filter)
    {
        _filter = filter;
        _page = 0;
        _selected = 0;
        RefreshRows();
    }

    private void Turn(int step)
    {
        int pages = Math.Max(1, (ListedQuests.Count + RowsPerPage - 1) / RowsPerPage);
        _page = Math.Clamp(_page + step, 0, pages - 1);
        _selected = _page * RowsPerPage;
        RefreshRows();
    }

    private void RefreshRows()
    {
        int count = ListedQuests.Count;
        for (int i = 0; i < _rows.Count; i++) _rows[i].Visible = _page * RowsPerPage + i < count;
        for (int i = 0; i < _tabs.Length; i++) _tabs[i].Selected = (int)_filter == i;
        _previous.Enabled = _page > 0;
        _next.Enabled = (_page + 1) * RowsPerPage < count;
        Invalidate();
    }

    public override void HandleKey(Keys key) { if (key == Keys.Escape) Host.ShowMap(); }

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get("assets/final-production-v5/ui/screens/runtime-800/journal-wood-red-v1.png"), CanvasRectangle);
        UiTheme.DrawTextCentered(g, "GÜNLÜK", UiTheme.Title, UiTheme.Gold, new Rectangle(220, 30, 360, 48));
        string[] tabNames = ["AKTİF", "BİTEN", "TÜMÜ"];
        for (int i = 0; i < tabNames.Length; i++)
            UiTheme.DrawTextCentered(g, tabNames[i], UiTheme.Small, (int)_filter == i ? UiTheme.Gold : UiTheme.Text, _tabs[i].Bounds);

        IReadOnlyList<Quest> quests = ListedQuests;
        for (int i = 0; i < RowsPerPage; i++)
        {
            int index = _page * RowsPerPage + i;
            if (index >= quests.Count) break;
            Quest quest = quests[index];
            Color color = index == _selected ? UiTheme.Gold
                : quest.Status == QuestStatus.Completed ? UiTheme.Success
                : quest.Status == QuestStatus.Failed ? UiTheme.Danger : UiTheme.Text;
            UiTheme.DrawTextCentered(g, quest.Title, UiTheme.Small, color, new Rectangle(94, 215 + i * 59, 206, 45));
        }
        int pages = Math.Max(1, (quests.Count + RowsPerPage - 1) / RowsPerPage);
        UiTheme.DrawTextCentered(g, $"SAYFA {_page + 1}/{pages}", UiTheme.Small, UiTheme.Text, new Rectangle(410, 688, 156, 48));

        DrawWaystones(g);
        if (quests.Count == 0)
        {
            UiTheme.DrawInkCentered(g, _session.Story.PrologueCompleted ? "Bu listede görev yok." : "Hikâye henüz başlamadı.",
                UiTheme.Body, Ink, new Rectangle(360, 270, 330, 120));
            return;
        }
        DrawDetails(g, quests[Math.Clamp(_selected, 0, quests.Count - 1)]);
    }

    private void DrawDetails(Graphics g, Quest quest)
    {
        UiTheme.DrawImageFit(g, Images.Get(QuestIcon(quest)), new Rectangle(366, 189, 60, 60));
        UiTheme.DrawInkCentered(g, quest.Title.ToUpper(Turkish), UiTheme.Heading, Ink, new Rectangle(440, 182, 255, 68));
        UiTheme.DrawInkCentered(g, $"{quest.KindName} · {quest.StatusName}", UiTheme.Small,
            quest.Status == QuestStatus.Failed ? Color.FromArgb(150, 24, 24) : Color.FromArgb(120, 72, 20), new Rectangle(360, 258, 335, 22));
        UiTheme.DrawWrappedFit(g, Format(quest.Description), UiTheme.Small, UiTheme.Small, Ink, new Rectangle(362, 284, 331, 66), ink: true);

        string goal = quest.ActiveObjective is QuestObjective objective ? $"HEDEF: {Format(objective.Text)}"
            : quest.Status == QuestStatus.Completed ? "Görev tamamlandı." : quest.Status == QuestStatus.Failed ? "Görev başarısız oldu." : "";
        UiTheme.DrawWrappedFit(g, goal, UiTheme.Small, UiTheme.Small, Color.FromArgb(110, 30, 24), new Rectangle(362, 352, 331, 50), ink: true);
        if (quest.Note is string note)
            UiTheme.DrawWrappedFit(g, $"NOT: {Format(note)}", UiTheme.Small, UiTheme.Small, Ink, new Rectangle(362, 404, 331, 46), ink: true);

        // Alt sol paneldeki dört satır: hedefler ve tamamlanma durumu.
        IReadOnlyList<QuestObjective> objectives = quest.Objectives;
        if (quest is MainQuest { UsingAlternative: true, Alternative: QuestObjective alternative }) objectives = [.. objectives, alternative];
        for (int i = 0; i < Math.Min(4, objectives.Count); i++)
        {
            bool done = quest.IsObjectiveDone(objectives[i].Id);
            int y = 506 + i * 30;
            using var ring = new Pen(UiTheme.Gold, 2);
            using var fill = new SolidBrush(UiTheme.Gold);
            var circle = new Rectangle(358, y + 3, 14, 14);
            if (done) g.FillEllipse(fill, circle);
            else g.DrawEllipse(ring, circle);
            UiTheme.DrawTextCentered(g, Format(objectives[i].Label), UiTheme.Small, done ? UiTheme.Success : UiTheme.Text, new Rectangle(384, y, 154, 22));
        }
    }

    // Sağ alttaki üç kutu: Kök, Gök ve Güneş Taşı. Uyanmamış taş karartılır.
    private void DrawWaystones(Graphics g)
    {
        (string Quest, string Icon)[] stones =
        [
            (StoryCatalog.RootQuest, "items/item-pack-02/icons/quest-relics/18-forest-seed.png"),
            (StoryCatalog.SkyQuest, "items/item-pack-02/icons/quest-relics/13-storm-pearl.png"),
            (StoryCatalog.SunQuest, "items/item-pack-02/icons/quest-relics/09-sun-medallion.png")
        ];
        for (int i = 0; i < stones.Length; i++)
        {
            var area = new Rectangle(566 + i * 47, 536, 36, 46);
            UiTheme.DrawImageFit(g, Images.Get(stones[i].Icon), area);
            if (_session.Story.Quest(stones[i].Quest).Status != QuestStatus.Completed) UiTheme.Shade(g, area, 170);
        }
        UiTheme.DrawTextCentered(g, $"TAŞLAR {_session.Story.CompletedWaystones}/3", UiTheme.Small, UiTheme.Text, new Rectangle(556, 590, 150, 22));
    }

    private string Format(string text) => StoryText.Format(_session, text);

    internal static string QuestIcon(Quest quest) => quest switch
    {
        WaystoneQuest { StoneId: "root" } => "items/item-pack-02/icons/quest-relics/18-forest-seed.png",
        WaystoneQuest { StoneId: "sky" } => "items/item-pack-02/icons/quest-relics/13-storm-pearl.png",
        WaystoneQuest => "items/item-pack-02/icons/quest-relics/09-sun-medallion.png",
        SideQuest => "items/item-pack-02/icons/quest-relics/14-haunted-locket.png",
        _ when quest.Id == StoryCatalog.MainOne => "items/item-pack-02/icons/quest-relics/24-world-map-fragment.png",
        _ => "items/item-pack-02/icons/quest-relics/02-eye-sigil.png"
    };

    // Yalnız --screenshots QA akışı kullanır.
    internal void SelectForQa(JournalFilter filter, int index)
    {
        SetFilter(filter);
        _selected = index;
        Invalidate();
    }
}
