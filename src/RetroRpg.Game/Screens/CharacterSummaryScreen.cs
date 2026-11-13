using RetroRpg.Core.Characters;
using RetroRpg.Core.Common;
using RetroRpg.Core.Items;

namespace RetroRpg.Game.Screens;

internal sealed class CharacterSummaryScreen : GameScreen
{
    private readonly CharacterCreationDraft _draft;

    public CharacterSummaryScreen(MainGameForm host, CharacterCreationDraft draft) : base(host)
    {
        _draft = draft;
        AddButton(new PixelButton(new Rectangle(72, 680, 198, 56), "GERİ", () => Host.ShowAttributeAllocation(_draft)) { Skin = PixelButton.MedievalLarge });
        AddButton(new PixelButton(new Rectangle(301, 680, 198, 56), "HİKÂYE SEÇ", () => Host.ShowStorySelection(_draft)) { Skin = PixelButton.MedievalLarge });
        AddButton(new PixelButton(new Rectangle(530, 680, 198, 56), "ONAYLA", () => Host.ShowStorySelection(_draft)) { Skin = PixelButton.MedievalLarge });
    }

    public override void HandleKey(Keys key)
    {
        if (key == Keys.Escape) Host.ShowAttributeAllocation(_draft);
        else if (key == Keys.Enter) Host.ShowStorySelection(_draft);
    }

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get("assets/final-production-v5/ui/screens/runtime-800/character-summary-wood-red-v1.png"), CanvasRectangle);
        UiTheme.DrawTextCentered(g, "KARAKTER ÖZETİ", UiTheme.Title, UiTheme.Gold, new Rectangle(180, 34, 440, 46));
        PlayerCharacter player = _draft.CreatePlayer();

        UiTheme.DrawImageFit(g, Images.Get(CharacterArt.PortraitPath(_draft.Race.Id, _draft.Gender, _draft.PortraitIndex)), new Rectangle(98, 138, 176, 170));
        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, new Rectangle(92, 332, 190, 172));
        UiTheme.DrawWrappedCentered(g, _draft.Background.Description, UiTheme.Small, UiTheme.Text, new Rectangle(108, 348, 158, 132));
        UiTheme.DrawTextCentered(g, player.Name, UiTheme.Heading, UiTheme.Gold, new Rectangle(80, 520, 216, 30));
        UiTheme.DrawTextCentered(g, player.Race.DisplayName, UiTheme.Small, UiTheme.Text, new Rectangle(80, 554, 216, 20));
        UiTheme.DrawTextCentered(g, player.Class.SubclassName, UiTheme.Small, UiTheme.Success, new Rectangle(80, 578, 216, 20));

        UiTheme.DrawTextCentered(g, player.Race.DisplayName, UiTheme.Small, UiTheme.Text, new Rectangle(359, 149, 70, 68));
        UiTheme.DrawTextCentered(g, player.Class.DisplayName, UiTheme.Small, UiTheme.Text, new Rectangle(486, 149, 70, 68));
        UiTheme.DrawWrappedCentered(g, player.Class.SubclassName, UiTheme.Small, UiTheme.Text, new Rectangle(615, 149, 70, 68));

        (string Name, int Value)[] stats =
        [
            ("GÜÇ", player.Stats.Strength), ("ÇEV", player.Stats.Dexterity),
            ("DAY", player.Stats.Constitution), ("ZEKÂ", player.Stats.Intelligence),
            ("BİL", player.Stats.Wisdom), ("KAR", player.Stats.Charisma)
        ];
        for (int i = 0; i < stats.Length; i++)
        {
            int col = i % 2, row = i / 2;
            int x = 337 + col * 200, y = 258 + row * 54;
            UiTheme.DrawText(g, stats[i].Name, UiTheme.Small, UiTheme.Text, x + 12, y + 12);
            UiTheme.DrawTextCentered(g, stats[i].Value.ToString(), UiTheme.Heading, UiTheme.Gold, new Rectangle(x + 116, y, 48, 42));
        }
        UiTheme.DrawTextCentered(g, $"CAN {player.MaxHealth} · AC {player.ArmorClass} · ATAK {player.AttackBonus:+#;-#;0} · İNİSİYATİF {player.InitiativeBonus:+#;-#;0}",
            UiTheme.Small, UiTheme.Text, new Rectangle(338, 421, 366, 30));

        for (int i = 0; i < _draft.BackgroundChoices.Count; i++)
        {
            BackgroundStageChoice choice = _draft.BackgroundChoices[i];
            string stage = StageFolder(choice.Stage);
            string path = $"assets/final-production-v5/ui/icons/background/{stage}/{choice.Id}-v1.png";
            UiTheme.DrawImageFit(g, Images.Get(path), new Rectangle(337 + i * 54, 459, 42, 44));
        }
        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, new Rectangle(330, 535, 380, 94));
        UiTheme.DrawWrappedCentered(g, _draft.Background.Description, UiTheme.Small, UiTheme.Text, new Rectangle(344, 548, 352, 48));
        UiTheme.DrawWrappedCentered(g, $"BAŞLANGIÇ: {StartingKitCatalog.SummaryFor(player)}", UiTheme.Small, UiTheme.Gold, new Rectangle(342, 593, 356, 34));
    }

    private static string StageFolder(BackgroundStage stage) => stage switch
    {
        BackgroundStage.Origin => "origin",
        BackgroundStage.Family => "family",
        BackgroundStage.Childhood => "childhood",
        BackgroundStage.Education => "education",
        BackgroundStage.Profession => "profession",
        BackgroundStage.TurningPoint => "turning-point",
        _ => "goal"
    };
}
