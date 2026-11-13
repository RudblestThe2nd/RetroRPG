using System.Drawing.Drawing2D;
using RetroRpg.Core.Characters;
using RetroRpg.Core.Combat;
using RetroRpg.Core.Common;
using RetroRpg.Core.Game;
using RetroRpg.Core.Items;
using RetroRpg.Core.Story;
using RetroRpg.Core.World;

namespace RetroRpg.Game.Screens;

internal enum CombatModal { None, Attack, Dice, Inventory, Escape, FleeDice, Victory, FateStrike }

// Eylemler önce kendi penceresini açar. Eşya saldırı yapmaz; kaçıştan önce düşman vurur.
// Hikâye savaşları (Göz Piskoposu, Vaelor) aynı ekranı CombatStoryEvent ile kullanır.
internal sealed class CombatScreen : GameScreen
{
    private const string DiceFolder = "assets/final-production-v2/combat/dice/";
    private static readonly Rectangle Scene = new(16, 203, 594, 454);
    private static readonly Color StatsInk = Color.FromArgb(14, 14, 14);
    private static readonly Color LogInk = Color.FromArgb(19, 19, 18);
    private static readonly int[] InitiativeCards = [50, 115, 189, 259, 331, 395, 464, 532];
    private static readonly System.Globalization.CultureInfo Turkish = new("tr-TR");
    private readonly GameSession _session;
    private readonly HexTile _tile;
    private readonly EnemyDefinition? _definition;
    private readonly CombatStoryEvent? _battle;
    private readonly EnemyNpc _enemy;
    private readonly CombatService _combat = new(new DiceRoller());
    private readonly DiceRoller _dice = new();
    private readonly List<string> _log = [];
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 90 };
    private readonly PixelButton[] _actions;
    private readonly List<PixelButton> _modalActions = [];
    private readonly bool _playerFirst;
    private CombatModal _modal;
    private int _turn = 1, _rollTicks, _deathTicks = -1, _lastRoll = 20, _lastDamage, _hoveredInitiative = -1;
    private string _lastAttackName = "NORMAL SALDIRI";
    private AttackRoll? _lastAttack;
    private D20Roll? _fleeRoll;
    private int _fleeDamage;
    private bool _enemyAttacked, _defending;

    public CombatScreen(MainGameForm host, GameSession session, HexTile tile) : this(host, session, tile, null) { }

    public CombatScreen(MainGameForm host, GameSession session, CombatStoryEvent battle)
        : this(host, session, session.Map.Get(session.CurrentHex)!, battle) { }

    private CombatScreen(MainGameForm host, GameSession session, HexTile tile, CombatStoryEvent? battle) : base(host)
    {
        _session = session; _tile = tile; _battle = battle;
        if (battle is null) { _definition = MapScreen.EncounterFor(host, tile); _enemy = EnemyCatalog.Create(_definition); }
        else _enemy = battle.CreateEnemy(session);
        _timer.Tick += (_, _) => Tick();
        _actions =
        [
            AddButton(new PixelButton(new Rectangle(55, 682, 128, 86), "", () => OpenModal(CombatModal.Attack))),
            AddButton(new PixelButton(new Rectangle(195, 682, 126, 86), "", () => { }) { Enabled = false }),
            AddButton(new PixelButton(new Rectangle(332, 682, 126, 86), "", () => OpenModal(CombatModal.Inventory))),
            AddButton(new PixelButton(new Rectangle(470, 682, 128, 86), "", Defend)),
            AddButton(new PixelButton(new Rectangle(612, 682, 124, 86), "", () => OpenModal(CombatModal.Escape)))
        ];
        AddModal(AttackLabel("NORMAL", 0, Player.DamageDie, 0), new(210, 320, 180, 72), () => ChooseAttack("NORMAL SALDIRI", 0, Player.DamageDie, 0));
        AddModal(AttackLabel("KESİN", 2, Math.Max(4, Player.DamageDie - 2), 0), new(410, 320, 180, 72), () => ChooseAttack("KESİN VURUŞ", 2, Math.Max(4, Player.DamageDie - 2), 0));
        AddModal(AttackLabel("GÜÇLÜ", -2, Player.DamageDie + 2, 1), new(210, 410, 180, 72), () => ChooseAttack("GÜÇLÜ VURUŞ", -2, Player.DamageDie + 2, 1));
        AddModal(AttackLabel("HIZLI", 1, Math.Max(4, Player.DamageDie - 2), 0), new(410, 410, 180, 72), () => ChooseAttack("HIZLI VURUŞ", 1, Math.Max(4, Player.DamageDie - 2), 0));
        AddModal("GERİ", new(304, 510, 192, 58), CloseModal);
        AddModal("DEVAM ET", new(304, 565, 192, 62), CompletePlayerResolution);
        AddModal("İKSİR KULLAN", new(210, 480, 180, 62), UsePotion);
        AddModal("VAZGEÇ", new(410, 480, 180, 62), CloseModal);
        AddModal("KAÇ", new(210, 480, 180, 62), ConfirmFlee);
        AddModal("SAVAŞA DÖN", new(410, 480, 180, 62), CloseModal);
        AddModal(battle is null ? "HARİTAYA DÖN" : "DEVAM ET", new(304, 520, 192, 68), LeaveAfterVictory);
        AddModal("KAÇIŞI TAMAMLA", new(285, 540, 230, 62), FinishFlee);
        AddModal("DEVAM ET", new(300, 528, 200, 62), () => Host.FinishStory(_battle!));
        InitiativeRoll mine = _combat.RollInitiative(Player), theirs = _combat.RollInitiative(_enemy);
        // Scripted savaşta oyuncu en az bir gerçek eylem yapabilsin diye ilk sıra her zaman oyuncudadır.
        _playerFirst = battle?.FateStrikeEventId is not null || mine.Total >= theirs.Total;
        AddLog($"İNİSİYATİF {mine.Total}-{theirs.Total}");
        if (!_playerFirst) EnemyTurn();
        UpdateActions();
    }

    private PlayerCharacter Player => _session.Player;
    public override void OnShown()
    {
        base.OnShown();
        if (!Player.IsAlive) BeginInvoke(() => Host.ShowDeath(_enemy.Name));
    }
    private PixelButton AddModal(string text, Rectangle bounds, Action action)
    {
        var b = AddModalButton(new PixelButton(bounds, text, action) { Skin = PixelButton.MedievalLarge, Visible = false });
        _modalActions.Add(b); return b;
    }
    private string AttackLabel(string name, int attackAdjust, int damageDie, int damageAdjust)
    {
        int hit = Player.AttackBonus + attackAdjust + StoryAttackBonus;
        int bonus = Player.DamageBonus + damageAdjust + StoryDamageBonus;
        return $"{name}  {hit:+#;-#;0}\nHASAR {Math.Max(1, 1 + bonus)}-{damageDie + bonus}";
    }

    // Üç Yol Taşı'nın ışığı yalnız hikâye savaşlarında isabet ve hasar ekler; seçimden önce görünür.
    private int StoryAttackBonus => _battle?.PlayerAttackBonus?.Invoke(_session) ?? 0;
    private int StoryDamageBonus => _battle?.PlayerDamageBonus?.Invoke(_session) ?? 0;

    private string EnemySprite(string pose) => _battle is null
        ? Host.Enemies.SpritePath(_definition!, pose)
        : pose == "attack" ? _battle.EnemyAttackSprite : _battle.EnemySprite;

    // Boss görselleri atlastan kesildiği için kenar kırıntısı kırpılır ve oranı korunarak alta oturtulur.
    private Image? EnemyImage(string pose) => Images.Get(EnemySprite(pose), trim: _battle is not null);

    private static Rectangle FitBottom(Image image, Rectangle area)
    {
        float scale = Math.Min((float)area.Width / image.Width, (float)area.Height / image.Height);
        int width = (int)(image.Width * scale), height = (int)(image.Height * scale);
        return new Rectangle(area.X + (area.Width - width) / 2, area.Bottom - height, width, height);
    }

    private void LeaveAfterVictory()
    {
        if (_battle is null) Host.ShowMap();
        else Host.FinishStory(_battle);
    }
    private void OpenModal(CombatModal modal) { _modal = modal; ModalOpen = true; ConfigureModalButtons(); UpdateActions(); }
    internal void OpenAttackForQa() => OpenModal(CombatModal.Attack);
    internal void ShowDiceResultForQa()
    {
        _lastAttackName = "NORMAL SALDIRI";
        _lastAttack = new AttackRoll(18, Player.AttackBonus, _enemy.ArmorClass);
        _lastDamage = Math.Min(8, _enemy.Health);
        _lastRoll = 18;
        _rollTicks = 0;
        _modal = CombatModal.Dice;
        ModalOpen = true;
        ConfigureModalButtons();
        UpdateActions();
    }
    private void CloseModal()
    {
        if (_timer.Enabled) return;
        _modal = CombatModal.None; ModalOpen = false; ConfigureModalButtons(); UpdateActions();
    }
    private void ConfigureModalButtons()
    {
        foreach (var b in _modalActions) { b.Visible = false; b.Enabled = true; }
        switch (_modal)
        {
            case CombatModal.Attack: for (int i = 0; i <= 4; i++) _modalActions[i].Visible = true; break;
            case CombatModal.Dice: _modalActions[5].Visible = _rollTicks <= 0; break;
            case CombatModal.Inventory:
                _modalActions[6].Visible = _modalActions[7].Visible = true;
                _modalActions[6].Enabled = Player.Inventory.Find<Consumable>() is not null; break;
            case CombatModal.Escape: _modalActions[8].Visible = _modalActions[9].Visible = true; break;
            case CombatModal.FleeDice: _modalActions[11].Visible = _rollTicks <= 0; break;
            case CombatModal.Victory: _modalActions[10].Visible = true; break;
            case CombatModal.FateStrike: _modalActions[12].Visible = true; break;
        }
        Invalidate();
    }
    private void ChooseAttack(string name, int attackAdjust, int damageDie, int damageAdjust)
    {
        _lastAttackName = name; _enemyAttacked = false;
        int before = _enemy.Health;
        _lastAttack = _combat.Attack(Player, _enemy, attackAdjust + StoryAttackBonus, damageDie, damageAdjust + StoryDamageBonus);
        _lastDamage = before - _enemy.Health; _lastRoll = _lastAttack.RawValue;
        LogRoll("SEN", _lastAttack, _lastDamage);
        _modal = CombatModal.Dice; _rollTicks = 24; ConfigureModalButtons(); _timer.Start(); UpdateActions();
    }
    private void CompletePlayerResolution()
    {
        if (_rollTicks > 0) return;
        if (!_enemy.IsAlive)
        {
            Player.Gold += _enemy.RewardGold;
            // Hikâye zaferi sonucu yazar; autosave olay bitince FinishStory içinde alınır.
            if (_battle is null) { _session.ClearDanger(_tile.Coordinate); Host.Autosave(); }
            else _battle.Victory(_session);
            _modal = CombatModal.Victory; _deathTicks = _battle?.EnemyRemainsOnDefeat == true ? -1 : 0;
            ConfigureModalButtons(); _timer.Start(); return;
        }
        EnemyTurn();
        if (EnemyTurnEndedCombat()) return;
        Player.DefenseBonus = 0; _turn++; CloseModal();
    }
    private void UsePotion()
    {
        if (Player.Inventory.Find<Consumable>() is not Consumable potion) return;
        int before = Player.Health; potion.Use(Player); Player.Inventory.Remove(potion);
        AddLog($"İKSİR: +{Player.Health - before} CAN"); EnemyTurn();
        if (EnemyTurnEndedCombat()) return;
        Host.Autosave();
        _turn++; CloseModal();
    }
    private void Defend()
    {
        _defending = true; Player.DefenseBonus = 2; AddLog("SAVUNMA: AC +2"); EnemyTurn();
        Player.DefenseBonus = 0; _defending = false;
        if (EnemyTurnEndedCombat()) return;
        _turn++; UpdateActions(); Invalidate();
    }

    // Düşman sırasından sonra savaş bitti mi? Kaderin Hükmü hikâye penceresini, gerçek yenilgi ölüm ekranını açar.
    // Scripted yenilgi normal ölüm sayılmaz; DeathScreen açılmaz ve can 0 iken autosave alınmaz.
    private bool EnemyTurnEndedCombat()
    {
        if (_modal == CombatModal.FateStrike) return true;
        if (Player.IsAlive) return false;
        Host.ShowDeath(_enemy.Name);
        return true;
    }
    private void ConfirmFlee()
    {
        _fleeRoll = _dice.RollD20(CharacterStats.Modifier(Player.Stats.Constitution));
        _fleeDamage = 0;
        _modal = CombatModal.FleeDice;
        _rollTicks = 24;
        ConfigureModalButtons();
        _timer.Start();
        UpdateActions();
    }

    private void FinishFlee()
    {
        if (_fleeRoll is null || _rollTicks > 0) return;
        bool success = _fleeRoll.IsNaturalTwenty || (!_fleeRoll.IsNaturalOne && _fleeRoll.Total >= 12);
        _fleeDamage = success ? 1 : _dice.Roll(4);
        Player.ReceiveDamage(_fleeDamage);
        AddLog($"KAÇIŞ DAYANIKLILIK: {_fleeRoll.Total}/12");
        AddLog($"YORGUNLUK: -{_fleeDamage} CAN");
        if (!success && Player.IsAlive)
        {
            AddLog("BAŞARISIZ: SERBEST SALDIRI");
            EnemyTurn();
        }
        if (EnemyTurnEndedCombat()) return;
        // Hikâye savaşından kaçış olayı tamamlamaz; olay tekrar denenebilir.
        _battle?.Flee(_session);
        Host.Autosave();
        Host.ShowMap();
    }
    private void EnemyTurn()
    {
        int before = Player.Health, lastStand = LastStandCharges();
        AttackRoll roll;
        if (_battle is null) roll = _combat.Attack(_enemy, Player);
        else
        {
            EnemyTurnResult result = _battle.EnemyTurn(_session, _enemy, _combat);
            if (result.Kind == EnemyTurnKind.ScriptedDefeat)
            {
                AddLog("VAELOR: KADERİN HÜKMÜ");
                _enemyAttacked = true; _modal = CombatModal.FateStrike; ModalOpen = true; ConfigureModalButtons(); UpdateActions();
                return;
            }
            roll = result.Roll!;
        }
        LogRoll("DÜŞMAN", roll, before - Player.Health); _enemyAttacked = true;
        if (LastStandCharges() < lastStand) AddLog("KADERBOZAN: 1 CAN'DA DURDU");
    }
    private int LastStandCharges() => (Player.Equipment.Get(EquipmentSlot.StoryLayer) as FatebreakerArmor)?.LastStandCharges ?? 0;
    private void LogRoll(string who, AttackRoll roll, int damage)
    {
        _lastRoll = roll.RawValue; string result = roll.IsCritical ? "KRİTİK" : roll.IsHit ? "İSABET" : "ISKA";
        AddLog($"{who} d20:{roll.RawValue}+{roll.Bonus}={roll.Total}");
        AddLog($"AC {roll.TargetArmorClass}: {result}" + (roll.IsHit ? $" -{damage}" : ""));
    }
    private void AddLog(string line) { _log.Add(line); if (_log.Count > 6) _log.RemoveAt(0); }
    private void Tick()
    {
        if (_rollTicks > 0) { _rollTicks--; if (_rollTicks == 0) { _timer.Stop(); ConfigureModalButtons(); } }
        else if (_deathTicks >= 0 && _deathTicks < 8) _deathTicks++;
        else _timer.Stop();
        Invalidate();
    }
    private void UpdateActions()
    {
        bool ready = !_timer.Enabled && _enemy.IsAlive && Player.IsAlive && !ModalOpen;
        _actions[0].Enabled = ready; _actions[2].Enabled = ready; _actions[3].Enabled = ready;
        _actions[4].Enabled = ready && (_battle?.FleeAllowed ?? true); Invalidate();
    }

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get(AssetPath("ui.combat.simple")), CanvasRectangle);
        DrawScene(g); DrawPlayerPanel(g); DrawInitiative(g); DrawEnemyPanel(g); DrawLog(g);
    }
    private void DrawScene(Graphics g)
    {
        string biome = _tile.Biome.ToString().ToLowerInvariant();
        string background = _tile.Biome == Biome.Forest
            ? "assets/final-production-v4/scenes/combat-backgrounds/forest-combat-background-pixel-v2.png"
            : $"assets/final-production-v1/scenes/combat-backgrounds/{biome}-combat-background-v1.png";
        GraphicsState state = g.Save(); g.SetClip(Scene);
        if (Images.Get(background) is Image scene)
        { g.InterpolationMode = InterpolationMode.NearestNeighbor; g.DrawImage(scene, Scene, new Rectangle(10, 150, 780, 596), GraphicsUnit.Pixel); }
        else UiTheme.DrawMissingBox(g, Scene);
        bool animating = _rollTicks > 0;
        CharacterAnimation pose = !Player.IsAlive ? CharacterAnimation.Death : _defending ? CharacterAnimation.Defend : animating ? CharacterAnimation.Attack : CharacterAnimation.Idle;
        string frame = CharacterArt.FramePath(Host, Player.Race.Id, Player.Gender, pose, Direction.East, animating ? (24 - _rollTicks) / 6 : 0);
        UiTheme.DrawPixelatedSprite(g, Images.Get(frame), new Rectangle(70, 392, 256, 256), 4);
        var enemyRect = new Rectangle(340, 384, 256, 256);
        if (_enemy.IsAlive || _deathTicks < 0)
        {
            string enemyPose = _enemyAttacked && !animating ? "attack" : "idle";
            if (EnemyImage(enemyPose) is Image sprite)
                UiTheme.DrawPixelatedSprite(g, sprite, _battle is null ? enemyRect : FitBottom(sprite, enemyRect), 3);
        }
        else if (_deathTicks < 8)
            UiTheme.DrawImage(g, Images.Get($"assets/final-production-v2/effects/combat-reactions/enemy-death-dissolve-v1/enemy-death-dissolve-{_deathTicks:00}-v1.png"), enemyRect);
        // Sprite üstlerinde can barı yok; can yalnız HUD ve sıra incelemesindedir.
        g.Restore(state);
    }
    private void DrawPlayerPanel(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get(CharacterArt.PortraitPath(Player.Race.Id, Player.Gender, _session.PortraitIndex)), new(48, 20, 90, 88));
        using var ink = new SolidBrush(StatsInk); g.FillRectangle(ink, 188, 22, 274, 86); g.FillRectangle(ink, 160, 52, 27, 26);
        UiTheme.DrawImageFit(g, Images.Get("assets/final-production-v2/ui/combat-indicators/runtime-128/action-icon-v1.png"), new(161, 53, 24, 24));
        UiTheme.DrawText(g, "CAN", UiTheme.Body, UiTheme.Text, 194, 27); UiTheme.DrawBar(g, new(258, 29, 136, 15), Player.Health, Player.MaxHealth, UiTheme.Danger);
        UiTheme.DrawText(g, $"{Player.Health}/{Player.MaxHealth}", UiTheme.Body, UiTheme.Text, 402, 27);
        UiTheme.DrawText(g, "ATAK", UiTheme.Body, UiTheme.Text, 194, 55); UiTheme.DrawText(g, $"d20+{Player.AttackBonus}   HASAR d{Player.DamageDie}", UiTheme.Body, UiTheme.Success, 258, 55);
        UiTheme.DrawText(g, "AC", UiTheme.Body, UiTheme.Text, 194, 83); UiTheme.DrawBar(g, new(258, 85, 136, 15), Player.ArmorClass, 20, Color.FromArgb(170, 170, 170));
        UiTheme.DrawText(g, Player.ArmorClass.ToString(), UiTheme.Body, UiTheme.Text, 402, 83);
        using var turnInk = new SolidBrush(Color.FromArgb(20, 14, 10)); g.FillRectangle(turnInk, 579, 38, 24, 22); UiTheme.DrawText(g, _turn.ToString(), UiTheme.Heading, Color.FromArgb(250, 220, 60), 580, 37);
    }
    private void DrawInitiative(Graphics g)
    {
        Image? player = Images.Get(CharacterArt.PortraitPath(Player.Race.Id, Player.Gender, _session.PortraitIndex));
        Image? enemy = Images.Get(EnemySprite("idle"), trim: true);
        using var ink = new SolidBrush(Color.FromArgb(26, 3, 50)); int active = ActiveInitiativeCard();
        for (int i = 0; i < InitiativeCards.Length; i++)
        {
            var card = new Rectangle(InitiativeCards[i], 140, 44, 42); g.FillRectangle(ink, card);
            bool mine = (i % 2 == 0) == _playerFirst; UiTheme.DrawImageFit(g, mine ? player : enemy, card);
            if (i == active || i == _hoveredInitiative) { using var p = new Pen(i == _hoveredInitiative ? Color.White : UiTheme.Gold, 2); g.DrawRectangle(p, card); }
        }
        int inspected = _hoveredInitiative >= 0 ? _hoveredInitiative : active; bool isPlayer = (inspected % 2 == 0) == _playerFirst;
        string name = isPlayer ? Player.Name : _enemy.Name; int hp = isPlayer ? Player.Health : _enemy.Health, max = isPlayer ? Player.MaxHealth : _enemy.MaxHealth;
        var area = new Rectangle(210, 184, 260, 18); using var b = new SolidBrush(Color.FromArgb(235, 12, 10, 16)); g.FillRectangle(b, area);
        UiTheme.DrawTextCentered(g, $"{name.ToUpper(Turkish)} · CAN {hp}/{max}", UiTheme.Small, UiTheme.Text, area);
    }
    private int ActiveInitiativeCard() => ((_playerFirst ? 0 : 1) + (_turn - 1) * 2) % InitiativeCards.Length;
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e); Point p = ToLogical(e.Location); int hit = -1;
        for (int i = 0; i < InitiativeCards.Length; i++) if (new Rectangle(InitiativeCards[i], 140, 44, 42).Contains(p)) { hit = i; break; }
        if (hit != _hoveredInitiative) { _hoveredInitiative = hit; Invalidate(); }
    }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hoveredInitiative = -1; Invalidate(); }
    private void DrawEnemyPanel(Graphics g)
    {
        using var nameInk = new SolidBrush(Color.FromArgb(44, 4, 64)); g.FillRectangle(nameInk, 644, 148, 130, 34);
        Font font = g.MeasureString(_enemy.Name.ToUpperInvariant(), UiTheme.Body).Width > 126 ? UiTheme.Small : UiTheme.Body;
        UiTheme.DrawTextCentered(g, _enemy.Name.ToUpper(Turkish), font, UiTheme.Text, new(644, 148, 130, 34));
        using var portraitInk = new SolidBrush(Color.FromArgb(30, 28, 34)); g.FillRectangle(portraitInk, 646, 204, 124, 108);
        UiTheme.DrawImageFit(g, Images.Get(EnemySprite("idle"), trim: true), new(650, 206, 116, 104));
        using var ink = new SolidBrush(LogInk); g.FillRectangle(ink, 662, 340, 118, 36); UiTheme.DrawText(g, "CAN", UiTheme.Small, UiTheme.Text, 666, 340);
        UiTheme.DrawBar(g, new(664, 358, 72, 12), _enemy.Health, _enemy.MaxHealth, UiTheme.Danger); UiTheme.DrawText(g, $"{_enemy.Health}/{_enemy.MaxHealth}", UiTheme.Small, UiTheme.Text, 738, 356);
        g.FillRectangle(ink, 740, 384, 40, 22); UiTheme.DrawText(g, _enemy.ArmorClass.ToString(), UiTheme.Body, UiTheme.Text, 752, 385);
        g.FillRectangle(ink, 636, 414, 144, 32); UiTheme.DrawImageFit(g, Images.Get($"{DiceFolder}runtime-256/d{_enemy.DamageDie}-icon-v1.png"), new(640, 416, 26, 26));
        UiTheme.DrawText(g, $"HASAR d{_enemy.DamageDie}", UiTheme.Small, UiTheme.Text, 672, 422);
    }
    private void DrawLog(Graphics g)
    {
        using var ink = new SolidBrush(LogInk); g.FillRectangle(ink, 636, 478, 142, 170); var die = new Rectangle(682, 482, 52, 52);
        if (_rollTicks > 0)
        {
            UiTheme.DrawImageFit(g, Images.Get($"{DiceFolder}d20-roll-v1/d20-roll-{(24 - _rollTicks) % 8:00}-v1.png"), die);
            UiTheme.DrawDiceQuestion(g, die);
        }
        else { UiTheme.DrawImageFit(g, Images.Get($"{DiceFolder}runtime-256/d20-icon-v1.png"), die); UiTheme.DrawDiceValue(g, die, _lastRoll); }
        for (int i = 0; i < _log.Count; i++) { string line = _log[i]; Color c = line.Contains("ISKA") ? UiTheme.Muted : line.Contains("İSABET") || line.Contains("KRİTİK") ? UiTheme.Success : UiTheme.Text; UiTheme.DrawText(g, "›", UiTheme.Small, UiTheme.Gold, 640, 538 + i * 18); UiTheme.DrawText(g, line, UiTheme.Small, c, 650, 538 + i * 18); }
    }
    protected override void DrawModal(Graphics g)
    {
        UiTheme.Shade(g, CanvasRectangle, 170);
        string asset = _modal switch
        {
            CombatModal.Attack => "assets/final-production-v5/ui/modals/runtime-800/attack-selection-panel-v1.png",
            CombatModal.Inventory => "assets/final-production-v5/ui/modals/runtime-800/combat-inventory-panel-v1.png",
            CombatModal.Escape or CombatModal.FateStrike => "assets/final-production-v5/ui/modals/runtime-800/escape-confirm-panel-v1.png",
            _ => "assets/final-production-v5/ui/modals/runtime-800/dice-result-panel-v1.png"
        };
        UiTheme.DrawImage(g, Images.Get(asset), new(150, 100, 500, 570));
        if (_modal == CombatModal.Attack)
        {
            UiTheme.DrawTextCentered(g, "SALDIRI SEÇ", UiTheme.Title, UiTheme.Gold, new(200, 185, 400, 50));
            UiTheme.DrawTextCentered(g, "İsabet için d20, hasar için gösterilen zar atılır.", UiTheme.Small, UiTheme.Text, new(190, 250, 420, 36));
        }
        else if (_modal == CombatModal.Dice)
        {
            UiTheme.DrawTextCentered(g, _lastAttackName, UiTheme.Title, UiTheme.Gold, new(200, 170, 400, 50));
            bool reveal = _rollTicks <= 6; string die = reveal ? $"{DiceFolder}runtime-256/d20-icon-v1.png" : $"{DiceFolder}d20-roll-v1/d20-roll-{_rollTicks % 8:00}-v1.png";
            UiTheme.DrawImageFit(g, Images.Get(die), new(305, 245, 190, 190));
            if (reveal && _lastAttack is not null) UiTheme.DrawDiceValue(g, new Rectangle(345, 305, 110, 70), _lastAttack.RawValue);
            else UiTheme.DrawDiceQuestion(g, new Rectangle(345, 305, 110, 70));
            string result = _rollTicks > 6 ? "ZAR ATILIYOR..." : _lastAttack is { IsHit: true } ? $"İSABET · {_lastDamage} HASAR" : "ISKA";
            UiTheme.DrawTextCentered(g, result, UiTheme.Heading, _lastAttack is { IsHit: true } ? UiTheme.Success : UiTheme.Danger, new(220, 460, 360, 50));
        }
        else if (_modal == CombatModal.Inventory)
        {
            var potion = Player.Inventory.Find<Consumable>(); UiTheme.DrawTextCentered(g, "SAVAŞ ENVANTERİ", UiTheme.Title, UiTheme.Gold, new(200, 175, 400, 50));
            UiTheme.DrawTextCentered(g, potion is null ? "Kullanılabilir iksir yok." : $"{potion.Name} · +{potion.Healing} CAN", UiTheme.Body, potion is null ? UiTheme.Danger : UiTheme.Text, new(210, 330, 380, 80));
        }
        else if (_modal == CombatModal.Escape)
        {
            UiTheme.DrawTextCentered(g, "KAÇIŞ", UiTheme.Title, UiTheme.Gold, new(200, 190, 400, 50));
            UiTheme.DrawWrapped(g, "Dayanıklılık d20 kontrolü yapılır. Başarıda 1 CAN yorgunluk; başarısızlıkta d4 CAN ve düşmanın serbest saldırısı uygulanır.", UiTheme.Body, UiTheme.Text, new(235, 300, 330, 130));
        }
        else if (_modal == CombatModal.FleeDice)
        {
            UiTheme.DrawTextCentered(g, "KAÇIŞ KONTROLÜ", UiTheme.Title, UiTheme.Gold, new(200, 175, 400, 50));
            bool reveal = _rollTicks <= 5;
            UiTheme.DrawImageFit(g, Images.Get($"{DiceFolder}runtime-256/d20-icon-v1.png"), new(305, 245, 190, 190));
            if (reveal && _fleeRoll is not null)
                UiTheme.DrawDiceValue(g, new Rectangle(345, 305, 110, 70), _fleeRoll.RawValue);
            else UiTheme.DrawDiceQuestion(g, new Rectangle(345, 305, 110, 70));
            string result = reveal && _fleeRoll is not null
                ? $"TOPLAM {_fleeRoll.Total} / ZORLUK 12"
                : "ZAR ATILIYOR...";
            UiTheme.DrawTextCentered(g, result, UiTheme.Heading, reveal ? UiTheme.Success : UiTheme.Gold, new(220, 460, 360, 50));
        }
        else if (_modal == CombatModal.Victory)
        {
            bool kneels = _battle?.EnemyRemainsOnDefeat == true;
            UiTheme.DrawTextCentered(g, kneels ? "DİZ ÇÖKTÜ" : "ZAFER", UiTheme.Title, UiTheme.Gold, new(200, 190, 400, 50));
            string text = kneels ? $"{_enemy.Name} sıfır cana indi ama ölmedi. Kaderine sen karar vereceksin."
                : $"{_enemy.Name} yenildi. +{_enemy.RewardGold} altın";
            UiTheme.DrawWrappedCentered(g, text, UiTheme.Body, UiTheme.Success, new(215, 300, 370, 120));
        }
        else if (_modal == CombatModal.FateStrike)
        {
            // Kaçış paneli: kırmızı başlık, sol üst büyük kutuda Vaelor, ortada açıklama, altta can.
            UiTheme.DrawTextCentered(g, "KADERİN HÜKMÜ", UiTheme.Title, UiTheme.Gold, new(225, 174, 350, 44));
            UiTheme.DrawImageFit(g, EnemyImage("attack"), new(254, 243, 82, 84));
            UiTheme.DrawWrappedCentered(g, "AC, zırh ve savunma yok sayıldı.", UiTheme.Small, UiTheme.Text, new(352, 252, 214, 70));
            UiTheme.DrawWrappedCentered(g, "Vaelor'un darbesi bedenine değil yazgına indi. Bu bir zar değil, kaderin hükmü.",
                UiTheme.Body, UiTheme.Text, new(226, 356, 348, 92));
            UiTheme.DrawTextCentered(g, $"CAN {Player.Health}/{Player.MaxHealth}", UiTheme.Heading, UiTheme.Danger, new(236, 472, 328, 40));
        }
    }

    // Yalnız --screenshots QA akışı kullanır: ilk Vaelor savaşında oyuncu eylemi sonrası kader darbesi.
    internal void ShowFateStrikeForQa()
    {
        if (_battle?.FateStrikeEventId is null) return;
        EnemyTurn();
    }
    protected override void Dispose(bool disposing) { if (disposing) _timer.Dispose(); base.Dispose(disposing); }
}
