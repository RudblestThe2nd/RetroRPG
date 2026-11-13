using RetroRpg.Core.Characters;
using RetroRpg.Core.Common;

namespace RetroRpg.Game.Screens;

internal sealed class AttributeAllocationScreen : GameScreen
{
    private static readonly string[] Names = ["GÜÇ", "ÇEVİKLİK", "DAYANIKLILIK", "ZEKÂ", "BİLGELİK", "KARİZMA"];
    private readonly CharacterCreationDraft _draft;
    private readonly PixelButton _continue;

    public AttributeAllocationScreen(MainGameForm host, CharacterCreationDraft draft) : base(host)
    {
        _draft = draft;
        for (int i = 0; i < Names.Length; i++)
        {
            int index = i;
            AddButton(new PixelButton(new Rectangle(625, 184 + i * 56, 34, 34), "−", () => Change(index, -1)) { Skin = PixelButton.Medium });
            AddButton(new PixelButton(new Rectangle(671, 184 + i * 56, 34, 34), "+", () => Change(index, 1)) { Skin = PixelButton.Medium });
        }
        AddButton(new PixelButton(new Rectangle(158, 676, 226, 52), "GERİ", () => Host.ShowCharacterCreation(_draft)) { Skin = PixelButton.MedievalLarge });
        _continue = AddButton(new PixelButton(new Rectangle(414, 676, 226, 52), "ÖZETİ GÖR", Continue) { Skin = PixelButton.MedievalLarge });
        UpdateButtons();
    }

    private void Change(int index, int amount)
    {
        if (_draft.Adjust(index, amount))
        {
            UpdateButtons();
            Invalidate();
        }
        else System.Media.SystemSounds.Beep.Play();
    }

    private void UpdateButtons() => _continue.Enabled = _draft.PointsRemaining == 0;
    private void Continue() => Host.ShowCharacterSummary(_draft);

    public override void HandleKey(Keys key)
    {
        if (key == Keys.Escape) Host.ShowCharacterCreation(_draft);
        else if (key == Keys.Enter && _continue.Enabled) Continue();
    }

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get("assets/final-production-v5/ui/screens/runtime-800/attribute-allocation-wood-red-v1.png"), CanvasRectangle);
        UiTheme.DrawTextCentered(g, "ÖZELLİK PUANLARI", UiTheme.Title, UiTheme.Gold, new Rectangle(180, 34, 440, 46));
        UiTheme.DrawTextCentered(g, $"KALAN PUAN: {_draft.PointsRemaining}", UiTheme.Heading,
            _draft.PointsRemaining == 0 ? UiTheme.Success : UiTheme.Gold, new Rectangle(390, 116, 250, 42));

        UiTheme.DrawImageFit(g, Images.Get(CharacterArt.PortraitPath(_draft.Race.Id, _draft.Gender, _draft.PortraitIndex)), new Rectangle(104, 174, 164, 154));
        UiTheme.DrawTextCentered(g, _draft.Name, UiTheme.Heading, UiTheme.Gold, new Rectangle(96, 336, 180, 30));
        UiTheme.DrawTextCentered(g, _draft.Race.DisplayName, UiTheme.Small, UiTheme.Text, new Rectangle(96, 370, 180, 24));
        UiTheme.DrawTextCentered(g, _draft.CharacterClass.SubclassName, UiTheme.Small, UiTheme.Success, new Rectangle(92, 396, 188, 24));
        UiTheme.DrawWrappedCentered(g, _draft.Background.DisplayName + "\n" + _draft.Background.Description,
            UiTheme.Small, UiTheme.Muted, new Rectangle(102, 438, 168, 116));

        PlayerCharacter preview = _draft.CreatePlayer();
        int[] values = [preview.Stats.Strength, preview.Stats.Dexterity, preview.Stats.Constitution,
            preview.Stats.Intelligence, preview.Stats.Wisdom, preview.Stats.Charisma];
        for (int i = 0; i < Names.Length; i++)
        {
            int y = 179 + i * 56;
            UiTheme.DrawTextCentered(g, Names[i], UiTheme.Body, UiTheme.Text, new Rectangle(386, y, 122, 45));
            UiTheme.DrawTextCentered(g, values[i].ToString(), UiTheme.Heading, UiTheme.Gold, new Rectangle(548, y, 58, 45));
            int modifier = CharacterStats.Modifier(values[i]);
            UiTheme.DrawTextCentered(g, modifier >= 0 ? $"+{modifier}" : modifier.ToString(), UiTheme.Small, UiTheme.Success, new Rectangle(505, y, 38, 45));
        }
        UiTheme.DrawTextCentered(g, $"CAN {preview.MaxHealth} · AC {preview.ArmorClass}", UiTheme.Small, UiTheme.Text, new Rectangle(340, 550, 162, 38));
        UiTheme.DrawTextCentered(g, $"ATAK {preview.AttackBonus:+#;-#;0} · İNİS. {preview.InitiativeBonus:+#;-#;0}", UiTheme.Small, UiTheme.Text, new Rectangle(520, 550, 170, 38));
        UiTheme.DrawTextCentered(g, $"HAREKET {_draft.Race.Movement}", UiTheme.Small, UiTheme.Text, new Rectangle(340, 598, 162, 38));
        UiTheme.DrawTextCentered(g, $"ALTIN {preview.Gold}", UiTheme.Small, UiTheme.Gold, new Rectangle(520, 598, 170, 38));
    }

}
