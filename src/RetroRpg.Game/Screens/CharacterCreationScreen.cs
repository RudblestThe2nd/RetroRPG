using RetroRpg.Core.Characters;
using RetroRpg.Core.Common;
using RetroRpg.Core.Game;
using RetroRpg.Core.Items;

namespace RetroRpg.Game.Screens;

// Kimlik, ırk ve sınıftan sonra geçmiş yedi ayrı seçimle kurulur.
// Hazır 800 x 800 çerçeveler kullanılır; metin ve etkileşim katmanı Türkçe çizilir.
internal sealed class CharacterCreationScreen : GameScreen
{
    private const string FrameAssetId = "ui.character-creation.wood-red-v1";
    private static readonly Rectangle Details = new(288, 508, 420, 150);
    private static readonly System.Globalization.CultureInfo Turkish = new("tr-TR");

    private int _step;
    private string _name = "Gezgin";
    private bool _nameEdited;
    private Gender _gender = Gender.Female;
    private int _portrait = 1;
    private int _hairStyle = 1;
    private int _facialStyle;
    private CharacterRace _race = RaceCatalog.All[0];
    private CharacterClass _mainClass = ClassCatalog.Find("fighter");
    private CharacterClass _subclass = ClassCatalog.SubclassesOf("fighter")[0];
    private readonly Dictionary<BackgroundStage, BackgroundStageChoice> _backgroundSelections =
        BackgroundCatalog.DefaultPath().ToDictionary(x => x.Stage);

    private const int FirstBackgroundStep = 3;
    private static int LastStep => FirstBackgroundStep + BackgroundCatalog.Stages.Count - 1;
    private CharacterBackground Background => BackgroundCatalog.Create(_backgroundSelections.Values);

    public CharacterCreationScreen(MainGameForm host, CharacterCreationDraft? draft = null) : base(host)
    {
        if (draft is not null)
        {
            _name = draft.Name;
            _nameEdited = true;
            _gender = draft.Gender;
            _portrait = draft.PortraitIndex;
            _hairStyle = draft.HairStyle;
            _facialStyle = draft.FacialStyle;
            _race = draft.Race;
            _mainClass = ClassCatalog.Find(draft.CharacterClass.Id);
            _subclass = draft.CharacterClass;
            _backgroundSelections.Clear();
            foreach (BackgroundStageChoice choice in draft.BackgroundChoices) _backgroundSelections[choice.Stage] = choice;
            _step = LastStep;
        }
        BuildButtons();
    }

    // Ekran görselindeki soldaki beş seçenek düğmesinin yeri.
    private static Rectangle Option(int index) => new(96, 165 + index * 67, 142, 48);

    // Önizleme panelindeki 5 x 3 yuvanın yeri.
    private static Rectangle Slot(int index) => new(297 + index % 5 * 84, 165 + index / 5 * 90, 68, 68);

    private void BuildButtons()
    {
        ClearButtons();
        switch (_step)
        {
            case 0:
                AddOption(0, () => { _name = ""; _nameEdited = true; Invalidate(); });
                AddOption(1, () => { _gender = _gender == Gender.Female ? Gender.Male : Gender.Female; BuildButtons(); });
                AddOption(2, CycleHair);
                AddOption(3, CycleFacialStyle);
                AddOption(4, () => GoTo(1));
                for (int i = 0; i < CharacterArt.PortraitCount; i++)
                {
                    int portrait = i + 1;
                    AddSlot(i, () => _portrait = portrait, _portrait == portrait,
                        picture: CharacterArt.PortraitPath(_race.Id, _gender, portrait));
                }
                break;
            case 1:
                AddOption(0, () => { }, enabled: false);
                AddOption(1, () => { }, enabled: false);
                AddOption(2, () => { }, enabled: false);
                AddOption(3, () => GoTo(0));
                AddOption(4, () => GoTo(2));
                for (int i = 0; i < RaceCatalog.All.Count; i++)
                {
                    CharacterRace race = RaceCatalog.All[i];
                    AddSlot(i, () => _race = race, _race == race, icon: $"ui/assets/icons/races/race-{race.Id}");
                }
                break;
            case 2:
                AddOption(0, () => { }, enabled: false);
                AddOption(1, CycleSubclass);
                AddOption(2, () => { }, enabled: false);
                AddOption(3, () => GoTo(1));
                AddOption(4, () => GoTo(3));
                for (int i = 0; i < ClassCatalog.All.Count; i++)
                {
                    CharacterClass characterClass = ClassCatalog.All[i];
                    AddSlot(i, () =>
                    {
                        _mainClass = characterClass;
                        _subclass = ClassCatalog.SubclassesOf(characterClass.Id)[0];
                    }, _mainClass == characterClass, icon: $"ui/assets/icons/classes/class-{characterClass.Id}");
                }
                break;
            default:
                AddOption(0, () => { }, enabled: false);
                AddOption(1, () => { }, enabled: false);
                AddOption(2, () => { }, enabled: false);
                AddOption(3, () => GoTo(_step - 1));
                AddOption(4, _step == LastStep ? ContinueToAttributes : () => GoTo(_step + 1));
                BackgroundStage stage = BackgroundCatalog.Stages[_step - FirstBackgroundStep];
                IReadOnlyList<BackgroundStageChoice> choices = BackgroundCatalog.OptionsFor(stage);
                for (int i = 0; i < choices.Count; i++)
                {
                    BackgroundStageChoice choice = choices[i];
                    AddSlot(i, () => _backgroundSelections[stage] = choice, _backgroundSelections[stage] == choice,
                        picture: $"assets/final-production-v5/ui/icons/background/{StageFolder(stage)}/{choice.Id}-v1.png");
                }
                break;
        }
        Invalidate();
    }

    private void AddOption(int index, Action onClick, bool enabled = true) =>
        AddButton(new PixelButton(Option(index), "", onClick) { Enabled = enabled });

    private void AddSlot(int index, Action select, bool selected, string? icon = null, string? picture = null) =>
        AddButton(new PixelButton(Slot(index), "", () => { select(); BuildButtons(); })
        {
            Icon = icon,
            Picture = picture is null ? null : Images.Get(picture),
            Selected = selected
        });

    private void CycleSubclass()
    {
        IReadOnlyList<CharacterClass> subclasses = ClassCatalog.SubclassesOf(_mainClass.Id);
        int index = subclasses.ToList().FindIndex(x => x.SubclassId == _subclass.SubclassId);
        _subclass = subclasses[(index + 1) % subclasses.Count];
        Invalidate();
    }

    private void GoTo(int step)
    {
        if (step < 0)
        {
            Host.ShowMainMenu();
            return;
        }
        _step = step;
        BuildButtons();
    }

    private void ContinueToAttributes()
    {
        string name = string.IsNullOrWhiteSpace(_name) ? "Gezgin" : _name.Trim();
        Host.ShowAttributeAllocation(new CharacterCreationDraft(name, _gender, _race, _subclass,
            Background.Choices, _portrait, _hairStyle, _facialStyle));
    }

    private void CycleHair()
    {
        _hairStyle = _hairStyle % CharacterArt.PortraitCount + 1;
        _portrait = _hairStyle;
        BuildButtons();
    }

    private void CycleFacialStyle()
    {
        _facialStyle = (_facialStyle + 1) % 6;
        _portrait = _portrait % CharacterArt.PortraitCount + 1;
        BuildButtons();
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

    private PlayerCharacter BuildPlayer()
    {
        string name = string.IsNullOrWhiteSpace(_name) ? "Gezgin" : _name.Trim();
        return new PlayerCharacter(name, _gender, _race, _subclass, Background, new CharacterStats());
    }

    public override void HandleKey(Keys key)
    {
        if (key == Keys.Escape) GoTo(_step - 1);
        else if (key == Keys.Enter)
        {
            if (_step < LastStep) GoTo(_step + 1);
            else ContinueToAttributes();
        }
        else if (key == Keys.Back && _step == 0 && _name.Length > 0)
        {
            _name = _name[..^1];
            Invalidate();
        }
    }

    public override void HandleChar(char character)
    {
        if (_step != 0 || _name.Length >= 16 || (!char.IsLetterOrDigit(character) && character != ' ')) return;
        if (!_nameEdited)
        {
            _name = "";
            _nameEdited = true;
        }
        _name += character;
        Invalidate();
    }

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get(AssetPath(FrameAssetId)), CanvasRectangle);
        DrawTurkishChrome(g);
        PlayerCharacter player = BuildPlayer();
        switch (_step)
        {
            case 0: DrawIdentity(g, player); break;
            case 1: DrawRace(g); break;
            case 2: DrawClass(g, player); break;
            default: DrawBackground(g, player); break;
        }
        UiTheme.DrawTextCentered(g, "ENTER: İLERİ   ESC: GERİ", UiTheme.Small, UiTheme.Gold, new Rectangle(220, 735, 360, 30));
    }

    private void DrawIdentity(Graphics g, PlayerCharacter player)
    {
        UiTheme.DrawImage(g, Images.Get(CharacterArt.PortraitPath(_race.Id, _gender, _portrait)), new Rectangle(Details.X, Details.Y + 8, 108, 108));
        int x = Details.X + 124;
        UiTheme.DrawText(g, "AD", UiTheme.Small, UiTheme.Muted, x, Details.Y + 8);
        UiTheme.DrawText(g, _name + "_", UiTheme.Heading, UiTheme.Gold, x, Details.Y + 24);
        UiTheme.DrawText(g, $"BEDEN  {(_gender == Gender.Female ? "Kadın" : "Erkek")}", UiTheme.Body, UiTheme.Text, x, Details.Y + 60);
        UiTheme.DrawText(g, $"PORTRE {_portrait}/{CharacterArt.PortraitCount}", UiTheme.Body, UiTheme.Text, x, Details.Y + 84);
        UiTheme.DrawText(g, $"SAÇ {_hairStyle} · {(_gender == Gender.Male ? "SAKAL" : "YÜZ DETAYI")} {_facialStyle}", UiTheme.Small, UiTheme.Success, x, Details.Y + 104);
        UiTheme.DrawWrapped(g, "Adını klavyeden yaz.\nBEDEN: değiştir   ONAYLA: devam",
            UiTheme.Small, UiTheme.Muted, new Rectangle(x, Details.Y + 122, Details.Right - x, 28));
    }

    private void DrawRace(Graphics g)
    {
        var bonus = new CharacterStats();
        _race.ApplyBonuses(bonus);
        string bonusText = string.Join("  ", new[]
        {
            ("Güç", bonus.Strength), ("Çeviklik", bonus.Dexterity), ("Dayanıklılık", bonus.Constitution),
            ("Zekâ", bonus.Intelligence), ("Bilgelik", bonus.Wisdom), ("Karizma", bonus.Charisma)
        }.Where(x => x.Item2 != 10).Select(x => $"{x.Item1} +{x.Item2 - 10}"));

        UiTheme.DrawText(g, _race.DisplayName.ToUpper(Turkish), UiTheme.Heading, UiTheme.Gold, Details.X, Details.Y + 8);
        UiTheme.DrawText(g, bonusText, UiTheme.Body, UiTheme.Success, Details.X, Details.Y + 40);
        UiTheme.DrawText(g, $"Hareket: {_race.Movement}", UiTheme.Small, UiTheme.Text, Details.X, Details.Y + 68);
        UiTheme.DrawImageFit(g, Images.Get(CharacterArt.PortraitPath(_race.Id, _gender, _portrait)),
            new Rectangle(Details.Right - 100, Details.Y + 12, 88, 88));
    }

    private void DrawClass(Graphics g, PlayerCharacter player)
    {
        Weapon weapon = ItemCatalog.StartingWeaponFor(_mainClass.Id);
        UiTheme.DrawText(g, _mainClass.DisplayName.ToUpper(Turkish), UiTheme.Heading, UiTheme.Gold, Details.X, Details.Y + 8);
        UiTheme.DrawText(g, $"Alt sınıf: {_subclass.SubclassName} (ALT SINIF ile değiştir)", UiTheme.Small, UiTheme.Success, Details.X, Details.Y + 40);
        UiTheme.DrawText(g, $"Özellik: {_subclass.SubclassFeature}", UiTheme.Small, UiTheme.Text, Details.X, Details.Y + 62);
        UiTheme.DrawText(g, $"İmza hareketi: {_mainClass.SignatureAction}", UiTheme.Small, UiTheme.Text, Details.X, Details.Y + 84);
        UiTheme.DrawText(g, $"CAN {player.MaxHealth}   AC {player.ArmorClass}   ATAK +{player.AttackBonus}", UiTheme.Body, UiTheme.Text, Details.X, Details.Y + 112);
        UiTheme.DrawText(g, $"Silah: {weapon.Name} (d{weapon.DamageDie})", UiTheme.Small, UiTheme.Text, Details.X, Details.Y + 140);
    }

    private void DrawBackground(Graphics g, PlayerCharacter player)
    {
        int stageIndex = _step - FirstBackgroundStep;
        BackgroundStage stage = BackgroundCatalog.Stages[stageIndex];
        BackgroundStageChoice choice = _backgroundSelections[stage];
        UiTheme.DrawText(g, choice.DisplayName.ToUpper(Turkish), UiTheme.Heading, UiTheme.Gold, Details.X, Details.Y + 8);
        UiTheme.DrawWrapped(g, choice.Description, UiTheme.Small, UiTheme.Text, new Rectangle(Details.X, Details.Y + 36, 320, 42));
        UiTheme.DrawText(g, choice.EffectText, UiTheme.Small, UiTheme.Success, Details.X, Details.Y + 82);
        UiTheme.DrawText(g, $"Aşama {stageIndex + 1}/7 · Başlangıç altını: {Background.StartingGold}", UiTheme.Small, UiTheme.Gold, Details.X, Details.Y + 106);
        UiTheme.DrawText(g, $"{player.Name} · {_race.DisplayName} · {_subclass.SubclassName}", UiTheme.Small, UiTheme.Muted, Details.X, Details.Y + 120);
        UiTheme.DrawText(g, _step == LastStep ? "İLERİ ile özellik puanlarına geç." : "İLERİ ile sonraki geçmiş aşamasına geç.", UiTheme.Small, UiTheme.Muted, Details.X, Details.Y + 138);
        UiTheme.DrawImageFit(g, Images.Get(CharacterArt.PortraitPath(_race.Id, _gender, _portrait)),
            new Rectangle(Details.Right - 100, Details.Y + 12, 88, 88));
    }

    private void DrawTurkishChrome(Graphics g)
    {
        string title;
        string[] labels;
        if (_step == 0)
        {
            title = "KARAKTER KİMLİĞİ";
            labels = ["AD", "BEDEN", "SAÇ", _gender == Gender.Male ? "SAKAL" : "YÜZ DETAYI", "ONAYLA"];
        }
        else if (_step == 1)
        {
            title = "IRK SEÇİMİ";
            labels = ["", "", "", "GERİ", "İLERİ"];
        }
        else if (_step == 2)
        {
            title = "SINIF VE ALT SINIF";
            labels = ["", "ALT SINIF", "", "GERİ", "İLERİ"];
        }
        else
        {
            BackgroundStage stage = BackgroundCatalog.Stages[_step - FirstBackgroundStep];
            title = $"GEÇMİŞ · {BackgroundCatalog.StageName(stage).ToUpper(Turkish)}";
            labels = ["", "", "", "GERİ", "İLERİ"];
        }

        UiTheme.DrawTextCentered(g, title, UiTheme.Title, UiTheme.Gold, new Rectangle(175, 36, 450, 42));
        UiTheme.DrawTextCentered(g, "SEÇENEKLER", UiTheme.Body, UiTheme.Gold, new Rectangle(88, 118, 160, 30));
        UiTheme.DrawTextCentered(g, "ÖNİZLEME", UiTheme.Body, UiTheme.Gold, new Rectangle(284, 118, 430, 30));
        UiTheme.DrawTextCentered(g, "AYRINTILAR", UiTheme.Body, UiTheme.Gold, new Rectangle(284, 468, 430, 30));

        for (int i = 0; i < labels.Length; i++)
        {
            UiTheme.DrawTextCentered(g, labels[i], UiTheme.Body, UiTheme.Text, Option(i));
        }
    }

}
