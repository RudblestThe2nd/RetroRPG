using RetroRpg.Core.Characters;
using RetroRpg.Core.Common;
using RetroRpg.Core.Game;

namespace RetroRpg.Game.Screens;

internal sealed class CharacterRecordScreen : GameScreen
{
    private readonly GameSession _session;

    public CharacterRecordScreen(MainGameForm host, GameSession session) : base(host)
    {
        _session = session;
        AddButton(new PixelButton(new Rectangle(285, 710, 230, 56), "HARİTAYA DÖN", () => Host.ShowMap()) { Skin = PixelButton.MedievalLarge });
    }

    public override void HandleKey(Keys key) { if (key == Keys.Escape) Host.ShowMap(); }

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get("assets/final-production-v5/ui/screens/runtime-800/character-record-wood-red-v1.png"), CanvasRectangle);
        UiTheme.DrawTextCentered(g, "KARAKTER", UiTheme.Title, UiTheme.Gold, new Rectangle(210, 32, 380, 48));
        var p = _session.Player;
        UiTheme.DrawImageFit(g, Images.Get(CharacterArt.PortraitPath(p.Race.Id, p.Gender, _session.PortraitIndex)), new Rectangle(88, 130, 142, 152));
        UiTheme.DrawTextCentered(g, p.Name, UiTheme.Heading, UiTheme.Gold, new Rectangle(82, 334, 154, 34));
        UiTheme.DrawWrappedCentered(g, $"{p.Race.DisplayName}\n{p.Class.SubclassName}", UiTheme.Small, UiTheme.Text, new Rectangle(82, 418, 154, 62));

        (string Name, int Value)[] stats = [("GÜÇ", p.Stats.Strength), ("ÇEV", p.Stats.Dexterity),
            ("DAY", p.Stats.Constitution), ("ZEKÂ", p.Stats.Intelligence),
            ("BİL", p.Stats.Wisdom), ("KAR", p.Stats.Charisma)];
        for (int i = 0; i < stats.Length; i++)
        {
            int y = 116 + i * 60;
            UiTheme.DrawTextCentered(g, stats[i].Name, UiTheme.Small, UiTheme.Text, new Rectangle(277, y, 55, 48));
            UiTheme.DrawTextCentered(g, stats[i].Value.ToString(), UiTheme.Heading, UiTheme.Gold, new Rectangle(343, y, 181, 48));
        }
        string[] derived = [$"CAN {p.Health}/{p.MaxHealth}", $"AC {p.ArmorClass}", $"ATAK {p.AttackBonus:+#;-#;0}",
            $"HASAR d{p.DamageDie}+{p.DamageBonus}", $"İNİS. {p.InitiativeBonus:+#;-#;0}", $"ALTIN {p.Gold}"];
        for (int i = 0; i < derived.Length; i++)
            UiTheme.DrawTextCentered(g, derived[i], UiTheme.Small, i == 5 ? UiTheme.Gold : UiTheme.Text, new Rectangle(563, 116 + i * 60, 154, 48));

        string xp = p.Level >= PlayerCharacter.MaximumLevel ? "MAKS." : $"{p.Experience}/{p.ExperienceToNextLevel}";
        UiTheme.DrawTextCentered(g, $"SV {p.Level}", UiTheme.Small, UiTheme.Gold, new Rectangle(557, 470, 50, 42));
        UiTheme.DrawTextCentered(g, $"XP {xp}", UiTheme.Small, UiTheme.Text, new Rectangle(610, 470, 54, 42));
        UiTheme.DrawTextCentered(g, $"GÜÇ {p.PowerPoints}/{p.MaxPowerPoints}", UiTheme.Small, UiTheme.Success, new Rectangle(667, 470, 54, 42));

        for (int i = 0; i < p.Background.Choices.Count; i++)
        {
            BackgroundStageChoice choice = p.Background.Choices[i];
            string folder = choice.Stage switch
            {
                BackgroundStage.Origin => "origin",
                BackgroundStage.Family => "family",
                BackgroundStage.Childhood => "childhood",
                BackgroundStage.Education => "education",
                BackgroundStage.Profession => "profession",
                BackgroundStage.TurningPoint => "turning-point",
                _ => "goal"
            };
            UiTheme.DrawImageFit(g, Images.Get($"assets/final-production-v5/ui/icons/background/{folder}/{choice.Id}-v1.png"), new Rectangle(96 + i * 83, 576, 70, 76));
        }
        UiTheme.DrawTextCentered(g, p.Background.DisplayName, UiTheme.Small, UiTheme.Gold, new Rectangle(86, 536, 628, 28));
    }
}
