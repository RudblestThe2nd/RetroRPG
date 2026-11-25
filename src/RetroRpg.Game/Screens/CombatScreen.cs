using System.Drawing.Drawing2D;
using RetroRpg.Core.Characters;
using RetroRpg.Core.Combat;
using RetroRpg.Core.Common;
using RetroRpg.Core.Game;
using RetroRpg.Core.Items;
using RetroRpg.Core.Story;
using RetroRpg.Core.World;

namespace RetroRpg.Game.Screens;

internal enum CombatModal { None, Attack, Power, Dice, Inventory, Escape, FleeDice, Victory, FateStrike }

// Eylemler önce kendi penceresini açar. Eşya saldırı yapmaz; kaçıştan önce düşman vurur.
// Hikâye savaşları (Göz Piskoposu, Vaelor) aynı ekranı CombatStoryEvent ile kullanır.
internal sealed partial class CombatScreen : GameScreen
{
    private const int FeedbackDuration = 12;
    private const int ImpactTick = 7;
    private const string DiceFolder = "assets/final-production-v2/combat/dice/";
    private static readonly Rectangle Scene = new(16, 203, 594, 454);
    private static readonly Color StatsInk = Color.FromArgb(14, 14, 14);
    private static readonly Color LogInk = Color.FromArgb(19, 19, 18);
    private static readonly int[] InitiativeCards = [50, 115, 189, 259, 331, 395, 464, 532];
    private static readonly System.Globalization.CultureInfo Turkish = new("tr-TR");
    private readonly GameSession _session;
    private readonly HexTile _tile;
    private readonly CombatStoryEvent? _battle;
    private EnemyNpc _enemy => _members[FocusSlot].Npc;
    private readonly CombatService _combat;
    private readonly DiceRoller _dice = new();
    private readonly List<string> _log = [];
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 90 };
    private readonly PixelButton[] _actions;
    private readonly List<PixelButton> _modalActions = [];
    private readonly bool _playerFirst;
    private EnemyAction _enemyIntent { get => _members[FocusSlot].Intent; set => _members[FocusSlot].Intent = value; }
    private CombatModal _modal;
    private int _turn = 1, _rollTicks, _feedbackTicks, _deathTicks = -1, _lastRoll = 20, _lastDamage, _hoveredInitiative = -1;
    private string _lastAttackName = "NORMAL SALDIRI";
    private string _impactArt = "slash";
    private bool _rapidHit;
    private AttackRoll? _lastAttack;
    private D20Roll? _fleeRoll;
    private int _fleeDamage;
    private bool _defending, _pendingPlayerFeedback;
    private bool _feedbackFromPlayer, _feedbackHit, _feedbackCritical, _feedbackBlocked, _feedbackSoundPlayed;
    private int _feedbackDamage;
    private Action? _afterFeedback;
    private int _victoryExperience, _levelsGained;
    private Item? _loot;
    private bool _enemyFled;

    public CombatScreen(MainGameForm host, GameSession session, HexTile tile, IDiceRoller? combatDice = null)
        : this(host, session, tile, null, combatDice) { }

    public CombatScreen(MainGameForm host, GameSession session, CombatStoryEvent battle)
        : this(host, session, session.Map.Get(session.CurrentHex)!, battle, null) { }

    private CombatScreen(MainGameForm host, GameSession session, HexTile tile, CombatStoryEvent? battle, IDiceRoller? combatDice) : base(host)
    {
        _session = session; _tile = tile; _battle = battle;
        _combat = new CombatService(combatDice ?? _dice);
        _members = MakeFormation(host, tile, battle);
        Player.StatusEffects.Clear();
        for (int i = 0; i < _members.Length; i++) { _selected = i; PrepareEnemyIntent(1); }
        _selected = 0;
        _timer.Tick += (_, _) => Tick();
        _flashTimer.Tick += (_, _) => { _flashTimer.Stop(); Invalidate(); };
        _visualTimer.Tick += (_, _) =>
        {
            _visualFrame = (_visualFrame + 1) % 240;
            if (!ModalOpen || _modal == CombatModal.Victory) Invalidate();
        };
        _actions =
        [
            AddButton(new PixelButton(new Rectangle(55, 682, 128, 86), "", () => OpenModal(CombatModal.Attack))),
            AddButton(new PixelButton(new Rectangle(195, 682, 126, 86), "", () => OpenModal(CombatModal.Power))),
            AddButton(new PixelButton(new Rectangle(332, 682, 126, 86), "", () => OpenModal(CombatModal.Inventory))),
            AddButton(new PixelButton(new Rectangle(470, 682, 128, 86), "", Defend)),
            AddButton(new PixelButton(new Rectangle(612, 682, 124, 86), "", () => OpenModal(CombatModal.Escape)))
        ];
        AddModal(AttackLabel("NORMAL", 0, Player.DamageDie, 0), new(210, 320, 180, 72), () => ChooseAttack("NORMAL SALDIRI", 0, Player.DamageDie, 0));
        AddModal(AttackLabel("KESİN", 2, Math.Max(4, Player.DamageDie - 2), 0), new(410, 320, 180, 72), () => ChooseAttack("KESİN VURUŞ", 2, Math.Max(4, Player.DamageDie - 2), 0, "projectile"));
        AddModal(AttackLabel("GÜÇLÜ", -2, Player.DamageDie + 2, 1), new(210, 410, 180, 72), () => ChooseAttack("GÜÇLÜ VURUŞ", -2, Player.DamageDie + 2, 1, "critical"));
        AddModal(AttackLabel("HIZLI", 1, Math.Max(4, Player.DamageDie - 2), 0), new(410, 410, 180, 72), () => ChooseAttack("HIZLI VURUŞ", 1, Math.Max(4, Player.DamageDie - 2), 0, "slash", rapid: true));
        AddModal("GERİ", new(304, 510, 192, 58), CloseModal);
        AddModal("DEVAM ET", new(304, 565, 192, 62), CompletePlayerResolution);
        AddModal("İKSİR KULLAN", new(210, 480, 180, 62), UsePotion);
        AddModal("VAZGEÇ", new(410, 480, 180, 62), CloseModal);
        AddModal("KAÇ", new(210, 480, 180, 62), ConfirmFlee);
        AddModal("SAVAŞA DÖN", new(410, 480, 180, 62), CloseModal);
        AddModal(battle is null ? "HARİTAYA DÖN" : "DEVAM ET", new(304, 520, 192, 68), LeaveAfterVictory);
        AddModal("KAÇIŞI TAMAMLA", new(285, 540, 230, 62), FinishFlee);
        AddModal("DEVAM ET", new(300, 528, 200, 62), () => Host.FinishStory(_battle!));
        AddModal("KULLAN", new(210, 500, 180, 62), UsePower);
        AddModal("GERİ", new(410, 500, 180, 62), CloseModal);
        _order = Enumerable.Range(-1, _members.Length + 1)
            .OrderByDescending(i => _combat.RollInitiative(i < 0 ? (ICombatant)Player : _members[i].Npc).Total).ToArray();
        if (battle?.FateStrikeEventId is not null) _order = [-1, 0];
        _playerFirst = _order[0] == -1;
        _cursor = _playerFirst ? 0 : -1;
        AddLog($"İLK SIRA: {(_playerFirst ? Player.Name : _members[_order[0]].Npc.Name)}");
        UpdateActions();
    }

    private PlayerCharacter Player => _session.Player;
    public override void OnShown()
    {
        base.OnShown();
        _visualTimer.Start();
        if (!Player.IsAlive) { BeginInvoke(() => Host.ShowDeath(_enemy.Name)); return; }
        void StartTurns()
        {
            if (!_playerFirst) EnemyTurn(UpdateActions);
            else UpdateActions();
        }
        if (_battle?.Id == "vaelor-final-fight") PlayBeat(CombatBeat.Unveil, StartTurns);
        else StartTurns();
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

    private Image? EnemyImage(string pose, int? slot = null, bool trim = false)
    {
        string path = _battle is null
            ? Host.Enemies.SpritePath(_members[slot ?? FocusSlot].Definition!, pose)
            : pose == "attack" ? _battle.EnemyAttackSprite : _battle.EnemySprite;
        // Biyom düşmanları ve eski boss atlasları sağa, Vaelor'un iki formu sola saldırır.
        return Images.Get(path, trim: trim || _battle is not null, mirror: !path.Contains("/vaelor/", StringComparison.OrdinalIgnoreCase));
    }

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
    internal void OpenPowerForQa() => OpenModal(CombatModal.Power);
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
    internal void ShowImpactForQa()
    {
        _log.Clear();
        _lastRoll = 20;
        _enemy.ReceiveDamage(Math.Min(8, _enemy.Health));
        AddLog($"SEN d20:20+{Player.AttackBonus}={20 + Player.AttackBonus}");
        AddLog($"AC {_enemy.ArmorClass}: KRİTİK -8");
        _modal = CombatModal.None;
        ModalOpen = false;
        _feedbackTicks = ImpactTick - 1;
        _feedbackFromPlayer = true;
        _feedbackHit = true;
        _feedbackCritical = true;
        _feedbackDamage = 8;
        _feedbackSoundPlayed = true;
        ConfigureModalButtons();
    }
    internal void ShowStatusesForQa()
    {
        _feedbackTicks = 0;
        _log.Clear();
        AddLog("DURUMLAR AKTİF");
        Player.StatusEffects.Add(new BlessingEffect());
        _enemy.StatusEffects.Add(new PoisonEffect());
        _enemyIntent = new EnemyAction("DURUM BÜYÜSÜ", EffectOnHit: () => new CurseEffect());
        _modal = CombatModal.None;
        ModalOpen = false;
        ConfigureModalButtons();
    }
    internal void ShowLootVictoryForQa()
    {
        _loot = LootGenerator.Create(ItemRarity.Epic, 8);
        _victoryExperience = 48;
        _levelsGained = 0;
        _enemyFled = false;
        _modal = CombatModal.Victory;
        ModalOpen = true;
        _rollTicks = 0;
        ConfigureModalButtons();
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
            case CombatModal.Attack:
                for (int i = 0; i <= 4; i++) { _modalActions[i].Visible = true; _modalActions[i].Enabled = i == 4 || CanReach(WeaponRanged); }
                break;
            case CombatModal.Power:
                _modalActions[13].Visible = _modalActions[14].Visible = true;
                var power = ClassPowerCatalog.For(Player.Class);
                _modalActions[13].Enabled = Player.PowerPoints >= power.Cost && PowerAllowed(power);
                break;
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
    private void ChooseAttack(string name, int attackAdjust, int damageDie, int damageAdjust, string? impactArt = null, bool rapid = false)
    {
        if (!CanReach(WeaponRanged)) return;
        _rangedPower = false;
        _impactArt = impactArt ?? (WeaponRanged ? "projectile" : "slash");
        _rapidHit = rapid;
        _lastAttackName = name;
        int before = _enemy.Health;
        _lastAttack = _combat.Attack(Player, _enemy, attackAdjust + StoryAttackBonus, damageDie,
            damageAdjust + StoryDamageBonus, Player.EquippedWeapon?.CriticalThreshold ?? 20);
        _lastDamage = before - _enemy.Health; _lastRoll = _lastAttack.RawValue;
        ApplyWeaponEffect(_lastAttack);
        LogRoll("SEN", _lastAttack, _lastDamage);
        _pendingPlayerFeedback = true;
        CombatAudio.Play(CombatSound.Dice);
        _modal = CombatModal.Dice; _rollTicks = 24; ConfigureModalButtons(); _timer.Start(); UpdateActions();
    }

    private void UsePower()
    {
        ClassPower power = ClassPowerCatalog.For(Player.Class);
        if (!PowerAllowed(power)) return;
        _rangedPower = power is AttackClassPower { IsRanged: true };
        if (!Player.TrySpendPowerPoint(power.Cost)) return;
        if (power is AttackClassPower attack)
        {
            _impactArt = attack.Id switch
            {
                "dragon-breath" or "hell-ray" => "fire",
                "dark-ray" or "chaos-bolt" => "dark",
                "thorn-burst" or "nature-magic" => "poison",
                "marked-shot" => "projectile",
                _ => attack.IsRanged ? "lightning" : "slash"
            };
            _rapidHit = attack.Id is "flurry" or "open-hand-flurry";
            _lastAttackName = attack.Name.ToUpper(Turkish);
            int before = _enemy.Health;
            _lastAttack = _combat.Attack(Player, _enemy,
                attack.AttackAdjustment + StoryAttackBonus,
                attack.DamageDie(Player),
                attack.DamageBonusAdjustment + StoryDamageBonus);
            _lastDamage = before - _enemy.Health;
            _lastRoll = _lastAttack.RawValue;
            LogRoll("SEN", _lastAttack, _lastDamage);
            _pendingPlayerFeedback = true;
            CombatAudio.Play(CombatSound.Dice);
            _modal = CombatModal.Dice;
            _rollTicks = 24;
            ConfigureModalButtons();
            _timer.Start();
            Host.Autosave();
            UpdateActions();
            return;
        }

        if (power is HealingClassPower healing)
        {
            int before = Player.Health;
            int amount = _dice.Roll(healing.DieSides) + healing.FixedBonus + Math.Max(0, PrimaryAbilityModifier());
            Player.Heal(amount);
            AddLog($"{power.Name.ToUpper(Turkish)}: +{Player.Health - before} CAN");
        }
        else if (power is GuardClassPower guard)
        {
            Player.DefenseBonus = guard.ArmorBonus;
            AddLog($"{power.Name.ToUpper(Turkish)}: AC +{guard.ArmorBonus}");
        }

        CombatAudio.Play(CombatSound.Heal);
        PlayBeat(power is HealingClassPower ? CombatBeat.Heal : CombatBeat.Bless, () => EnemyTurn(() =>
        {
            Player.DefenseBonus = 0;
            if (EnemyTurnEndedCombat()) return;
            Host.Autosave();
            _turn++;
            CloseModal();
        }));
    }

    private int PrimaryAbilityModifier() => Player.Class.PrimaryAbility switch
    {
        "Strength" => CharacterStats.Modifier(Player.Stats.Strength),
        "Dexterity" => CharacterStats.Modifier(Player.Stats.Dexterity),
        "Constitution" => CharacterStats.Modifier(Player.Stats.Constitution),
        "Intelligence" => CharacterStats.Modifier(Player.Stats.Intelligence),
        "Wisdom" => CharacterStats.Modifier(Player.Stats.Wisdom),
        "Charisma" => CharacterStats.Modifier(Player.Stats.Charisma),
        _ => 0
    };
    private void CompletePlayerResolution()
    {
        if (_rollTicks > 0) return;
        void EndAction() => EnemyTurn(() =>
        {
            if (EnemyTurnEndedCombat()) return;
            Player.DefenseBonus = 0; _turn++; CloseModal();
        });
        if (!_enemy.IsAlive) ResolveDeparture(false, EndAction);
        else EndAction();
    }
    private void UsePotion()
    {
        if (Player.Inventory.Find<Consumable>() is not Consumable potion) return;
        int before = Player.Health; potion.Use(Player); Player.Inventory.Remove(potion);
        AddLog($"İKSİR: +{Player.Health - before} CAN");
        CombatAudio.Play(CombatSound.Heal);
        EnemyTurn(() =>
        {
            if (EnemyTurnEndedCombat()) return;
            Host.Autosave();
            _turn++; CloseModal();
        });
    }
    private void Defend()
    {
        _defending = true; Player.DefenseBonus = 2; AddLog("SAVUNMA: AC +2");
        EnemyTurn(() =>
        {
            Player.DefenseBonus = 0; _defending = false;
            if (EnemyTurnEndedCombat()) return;
            _turn++; UpdateActions(); Invalidate();
        });
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
        _lastRoll = _fleeRoll.RawValue;
        _fleeDamage = 0;
        _modal = CombatModal.FleeDice;
        _rollTicks = 24;
        CombatAudio.Play(CombatSound.Dice);
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
        void LeaveCombat()
        {
            if (EnemyTurnEndedCombat()) return;
            Player.StatusEffects.Clear();
            _battle?.Flee(_session);
            Host.Autosave();
            Host.ShowMap();
        }
        if (!success && Player.IsAlive)
        {
            AddLog("BAŞARISIZ: SERBEST SALDIRI");
            if (Group) { _acting = _members.ToList().FindIndex(x => x.Present); RunEnemyAction(LeaveCombat); }
            else EnemyTurn(LeaveCombat);
            return;
        }
        LeaveCombat();
    }
    private void EnemyTurn(Action afterFeedback)
    {
        if (Group) NextGroupActor(afterFeedback);
        else RunEnemyAction(afterFeedback);
    }

    private void RunEnemyAction(Action afterFeedback)
    {
        if (_battle is null)
        {
            _enemy.DefenseBonus = 0;
            bool enemySkips = AdvanceStatuses(_enemy);
            if (!_enemy.IsAlive) { ResolveDeparture(false, afterFeedback); return; }
            if (enemySkips)
            {
                AddLog($"{_enemy.Name.ToUpper(Turkish)} TURU ATLADI");
                PrepareEnemyIntent(_turn + 1);
                PlayBeat(CombatBeat.EnemyStun, () => FinishEnemyAction(afterFeedback));
                return;
            }

            EnemyAction action = _enemyIntent;
            if (action.Kind == EnemyActionKind.Flee)
            {
                AddLog($"{_enemy.Name.ToUpper(Turkish)} KAÇTI");
                PlayBeat(CombatBeat.Retreat, () => ResolveDeparture(true, afterFeedback));
                return;
            }
            if (action.Kind == EnemyActionKind.Guard)
            {
                _enemy.DefenseBonus = 2;
                AddLog($"DÜŞMAN SAVUNMA: AC +2");
                PrepareEnemyIntent(_turn + 1);
                PlayBeat(CombatBeat.Guard, () => FinishEnemyAction(afterFeedback));
                return;
            }
        }

        int before = Player.Health, lastStand = LastStandCharges();
        AttackRoll roll;
        EnemyAction intent = _enemyIntent;
        if (_battle is null)
            roll = _combat.Attack(_enemy, Player, intent.AttackAdjustment,
                Math.Max(2, _enemy.DamageDie + intent.DamageDieAdjustment), intent.DamageBonusAdjustment,
                _enemy.Weapon?.CriticalThreshold ?? 20);
        else
        {
            EnemyTurnResult result = _battle.EnemyTurn(_session, _enemy, _combat);
            if (result.Kind == EnemyTurnKind.ScriptedDefeat)
            {
                AddLog("VAELOR: KADERİN HÜKMÜ");
                PlayBeat(CombatBeat.Judgment, OpenFateStrike);
                return;
            }
            roll = result.Roll!;
        }
        int damage = before - Player.Health;
        LogRoll("DÜŞMAN", roll, damage);
        if (_battle is null && roll.IsHit && intent.EffectOnHit?.Invoke() is StatusEffect effect)
        {
            Player.StatusEffects.Add(effect);
            AddLog($"DURUM: {effect.Name.ToUpper(Turkish)}");
        }
        _armorIntercept = LastStandCharges() < lastStand;
        if (_armorIntercept) AddLog("KADERBOZAN: 1 CAN'DA DURDU");
        _castAttack = _enemy.Behavior is CasterBehavior || _enemy.Weapon?.AssetId == "fate-ray";
        PrepareEnemyIntent(_turn + 1);
        BeginFeedback(false, roll, damage, () => FinishEnemyAction(afterFeedback));
    }

    private void PrepareEnemyIntent(int turn)
    {
        _enemyIntent = _battle is null
            ? _enemy.Behavior.Choose(_enemy, Player, Math.Max(1, turn))
            : new EnemyAction(_enemy is MiniBossNpc mini ? mini.SpecialAttack.ToUpper(Turkish) : "HİKÂYE SALDIRISI");
    }

    private void FinishEnemyAction(Action continuation)
    {
        if (Group)
        {
            if (!Player.IsAlive) Host.ShowDeath(_enemy.Name);
            else continuation();
            return;
        }
        bool playerSkips = AdvanceStatuses(Player);
        if (!Player.IsAlive) { Host.ShowDeath(_enemy.Name); return; }
        if (playerSkips)
        {
            AddLog("SERSEMLEDİN: TUR ATLADI");
            _turn++;
            PlayBeat(CombatBeat.PlayerStun, () => EnemyTurn(continuation));
            return;
        }
        continuation();
    }

    private bool AdvanceStatuses(ICombatant combatant)
    {
        bool skip = false;
        foreach (StatusTurnResult result in combatant.StatusEffects.AdvanceTurn(combatant))
        {
            AddLog(result.Text);
            skip |= result.SkipsTurn;
        }
        return skip;
    }

    private void ApplyWeaponEffect(AttackRoll roll)
    {
        if (!roll.IsHit || Player.EquippedWeapon?.Technique.EffectOnHit(roll) is not StatusEffect effect) return;
        _enemy.StatusEffects.Add(effect);
        AddLog($"DURUM: {effect.Name.ToUpper(Turkish)}");
    }

    private void BeginVictory(bool fled = false)
    {
        _enemyFled = fled;
        _victoryGold = _members.Sum(m => m.Fled ? m.Npc.RewardGold / 2 : m.Npc.RewardGold);
        Player.Gold += _victoryGold;
        _victoryExperience = _members.Sum(m => m.Fled ? Math.Max(1, m.Npc.RewardExperience / 2) : m.Npc.RewardExperience);
        _levelsGained = Player.AddExperience(_victoryExperience);
        if (_battle is null)
        {
            if (!fled)
            {
                _loot = LootGenerator.Roll(_enemy, Player.Level, StableLootSeed());
                if (_loot is not null) Player.Inventory.Add(_loot);
            }
            _session.ClearDanger(_tile.Coordinate);
            Host.Autosave();
        }
        else _battle.Victory(_session);
        Player.StatusEffects.Clear();
        _enemy.StatusEffects.Clear();
        bool kneels = _battle?.EnemyRemainsOnDefeat == true;
        _deathTicks = fled || kneels ? -1 : 0;
        if (!fled)
        {
            PlayBeat(kneels ? CombatBeat.Kneel : CombatBeat.Dissolve, OpenVictory);
            return;
        }
        OpenVictory();
    }

    private void OpenVictory()
    {
        _modal = CombatModal.Victory;
        ModalOpen = true;
        _lootTicks = 0;
        CombatAudio.Play(CombatSound.Victory);
        ConfigureModalButtons();
        _timer.Start();
        UpdateActions();
    }

    private int StableLootSeed()
    {
        int name = 17;
        foreach (char c in _enemy.Name) name = unchecked(name * 31 + c);
        return unchecked(name + _tile.Coordinate.Q * 397 + _tile.Coordinate.R * 997 + Player.Level * 53);
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
        if (_beat != CombatBeat.None)
        {
            _beatTick++;
            if (_beat == CombatBeat.Dissolve) _deathTicks = _beatTick;
            if (_beatTick >= 9)
            {
                _timer.Stop();
                _beat = CombatBeat.None;
                Action? done = _afterBeat;
                _afterBeat = null;
                done?.Invoke();
            }
        }
        else if (_rollTicks > 0)
        {
            _rollTicks--;
            if (_rollTicks == 0)
            {
                if (_pendingPlayerFeedback && _lastAttack is not null)
                {
                    _pendingPlayerFeedback = false;
                    BeginFeedback(true, _lastAttack, _lastDamage, ShowPlayerResult);
                }
                else { _timer.Stop(); ConfigureModalButtons(); }
            }
        }
        else if (_feedbackTicks > 0)
        {
            _feedbackTicks--;
            if (!_feedbackSoundPlayed && _feedbackTicks <= ImpactTick)
            {
                PlayImpactSound();
                if (_feedbackHit && _feedbackDamage > 0)
                {
                    _flashTimer.Start();
                    Refresh();
                }
            }
            if (_feedbackTicks == 0)
            {
                Action? finished = _afterFeedback;
                _afterFeedback = null;
                _timer.Stop();
                finished?.Invoke();
            }
        }
        else if (_modal == CombatModal.Victory && _lootTicks < 9) _lootTicks++;
        else _timer.Stop();
        Invalidate();
    }

    private void BeginFeedback(bool fromPlayer, AttackRoll roll, int damage, Action finished)
    {
        if (fromPlayer) _armorIntercept = false;
        _feedbackFromPlayer = fromPlayer;
        _feedbackHit = roll.IsHit;
        _feedbackCritical = roll.IsCritical;
        _feedbackBlocked = !fromPlayer && !roll.IsHit && _defending;
        _feedbackDamage = damage;
        _feedbackSoundPlayed = false;
        _feedbackTicks = FeedbackDuration;
        _afterFeedback = finished;
        _modal = CombatModal.None;
        ModalOpen = false;
        ConfigureModalButtons();
        CombatAudio.Play(CombatSound.Swing);
        _timer.Start();
        UpdateActions();
    }

    private void ShowPlayerResult()
    {
        _modal = CombatModal.Dice;
        ModalOpen = true;
        ConfigureModalButtons();
        UpdateActions();
    }

    private void PlayImpactSound()
    {
        _feedbackSoundPlayed = true;
        CombatAudio.Play(_feedbackBlocked ? CombatSound.Block
            : !_feedbackHit ? CombatSound.Miss
            : _feedbackCritical ? CombatSound.Critical
            : CombatSound.Hit);
    }
    private void UpdateActions()
    {
        bool ready = !_timer.Enabled && _enemy.IsAlive && Player.IsAlive && !ModalOpen;
        _actions[0].Enabled = ready;
        _actions[1].Enabled = ready && Player.PowerPoints > 0;
        _actions[2].Enabled = ready; _actions[3].Enabled = ready;
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
        bool animating = _feedbackTicks > 0;
        bool impact = animating && _feedbackTicks <= ImpactTick && _feedbackHit && _feedbackDamage > 0;
        int elapsed = FeedbackDuration - _feedbackTicks;
        bool falling = VisibleHealth(Player) <= 0 && (_beat != CombatBeat.Judgment || _beatTick >= 4);
        CharacterAnimation pose = falling ? CharacterAnimation.Death
            : animating && _feedbackFromPlayer ? CharacterAnimation.Attack
            : impact && !_feedbackFromPlayer ? CharacterAnimation.Hit
            : _defending ? CharacterAnimation.Defend : CharacterAnimation.Idle;
        bool swinging = animating && _feedbackFromPlayer;
        bool holding = pose == CharacterAnimation.Attack || (pose == CharacterAnimation.Idle && Player.EquippedWeapon is not null);
        GripPose grip = PlayerGripPose(elapsed, swinging);
        var playerWounds = WoundsFor(Player);
        var bodyId = $"{Player.Race.Id}-{Player.Gender.ToString().ToLowerInvariant()}";
        bool injuredIdle = pose == CharacterAnimation.Idle && playerWounds.Stage > 0;
        string frame = injuredIdle ? CharacterArt.InjuredFrame(bodyId, playerWounds.Stage) : holding
            ? CharacterArt.FramePath(Host, Player.Race.Id, Player.Gender, CharacterAnimation.Attack, grip.Direction, grip.Frame)
            : CharacterArt.FramePath(Host, Player.Race.Id, Player.Gender, pose, Direction.East, animating ? elapsed / 3 : 0);
        var playerRect = new Rectangle(Group ? 12 : 70, 392, 256, 256);
        var enemyRect = SlotBounds(FocusSlot);
        PositionFighters(ref playerRect, ref enemyRect);
        ref Rectangle defender = ref (_feedbackFromPlayer ? ref enemyRect : ref playerRect);
        Point dodge = DodgeOffset(defender);
        defender.Offset(dodge);
        var playerSprite = Images.Get(frame);
        if (Group)
        {
            UiTheme.Shade(g, Scene, 45);
            for (int i = _members.Length - 1; i >= 0; i--)
            {
                if (!_members[i].Present) continue;
                Rectangle area = i == FocusSlot ? enemyRect : SlotBounds(i);
                if (i == _selected)
                {
                    using var ring = new Pen(UiTheme.Gold, 3);
                    using var shadow = new Pen(Color.FromArgb(170, 26, 15, 5), 7);
                    g.DrawEllipse(shadow, area.X + area.Width / 5, area.Bottom - 17, area.Width * 3 / 5, 13);
                    g.DrawEllipse(ring, area.X + area.Width / 5, area.Bottom - 17, area.Width * 3 / 5, 13);
                    int x = area.X + area.Width / 2;
                    g.FillPolygon(Brushes.Black, new Point[] { new(x-10,area.Y-7), new(x+10,area.Y-7), new(x,area.Y+5) });
                    g.FillPolygon(Brushes.Gold, new Point[] { new(x-7,area.Y-6), new(x+7,area.Y-6), new(x,area.Y+2) });
                }
                if (i == FocusSlot) continue;
                var member = _members[i].Npc;
                DrawEnemyBody(g, "idle", i, area);
                DrawStatuses(g, member.StatusEffects, new(area.X + area.Width / 2, area.Y + area.Height / 2));
            }
        }
        DrawDodgeTrail(g, defender, dodge);
        UiTheme.DrawPixelatedSprite(g, playerSprite, playerRect, 4, injuredIdle ? new(0, playerWounds.Flash) : playerWounds);
        var enemyBody = enemyRect;
        if ((!_enemyFled || _beat == CombatBeat.Retreat) && (_enemy.IsAlive || _deathTicks < 0))
        {
            string enemyPose = (animating && !_feedbackFromPlayer) || _beat == CombatBeat.Judgment ? "attack" : "idle";
            enemyBody = DrawEnemyBody(g, enemyPose, FocusSlot, enemyRect);
        }
        else if (_deathTicks is >= 0 and < 8 && EnemyImage("idle") is Image fading)
            DrawDissolve(g, fading, _battle is null ? enemyRect : FitBottom(fading, enemyRect), _deathTicks);
        Point target = new(enemyBody.X + enemyBody.Width / 2, enemyBody.Y + enemyBody.Height / 2);
        if (holding && !(swinging && _rangedPower) && Player.EquippedWeapon is Weapon equipped)
        {
            if (injuredIdle)
            {
                var anchor = Host.WoundedHands[bodyId][playerWounds.Stage - 1];
                grip = new(0, anchor.X, anchor.Y);
            }
            Point hand = new(playerRect.X + grip.X, playerRect.Y + grip.Y);
            Point aim = target;
            if (swinging) aim.Offset(-dodge.X, -dodge.Y);
            DrawWeapon(g, equipped.AssetId, hand, aim, 1,
                swinging ? elapsed : -1, playerSprite as Bitmap, grip);
        }
        Point chest = new(playerRect.X + playerRect.Width / 2, playerRect.Y + playerRect.Height * 132 / 256);
        DrawBattleMotion(g, chest, target, enemyBody.Bottom, dodge);
        if (animating) DrawImpactFeedback(g, playerRect, enemyRect);
        // Sprite üstlerinde can barı yok; can yalnız HUD ve sıra incelemesindedir.
        g.Restore(state);
    }

    private Rectangle DrawEnemyBody(Graphics g, string pose, int slot, Rectangle area)
    {
        var member = _members[slot];
        var wounds = WoundsFor(member.Npc);
        bool injuredIdle = pose == "idle" && wounds.Stage > 0 && member.Npc.IsAlive;
        string bodyId = _battle is null
            ? $"{member.Definition!.Biome.ToString().ToLowerInvariant()}-{member.Definition.Id}"
            : Path.GetFileNameWithoutExtension(_battle.EnemySprite).Replace("-idle-v1", "");
        Image? sprite = injuredIdle
            ? Images.Get(CharacterArt.InjuredFrame(bodyId, wounds.Stage), mirror: true)
            : EnemyImage(pose, slot);
        if (_battle is not null && sprite is not null) area = FitBottom(sprite, area);
        UiTheme.DrawPixelatedSprite(g, sprite, area, 3, injuredIdle ? new(0, wounds.Flash) : wounds);
        return area;
    }

    private void DrawImpactFeedback(Graphics g, Rectangle playerRect, Rectangle enemyRect)
    {
        if (_feedbackTicks > ImpactTick) return;
        Rectangle target = _feedbackFromPlayer ? enemyRect : playerRect;
        int age = ImpactTick - _feedbackTicks;

        if (_feedbackHit && _feedbackCritical)
        {
            Point center = new(target.Left + target.Width / 2, target.Top + target.Height / 2);
            Color main = _feedbackCritical ? Color.FromArgb(255, 245, 95) : Color.White;
            Color edge = _feedbackCritical ? Color.FromArgb(230, 55, 30) : Color.FromArgb(255, 160, 55);
            int size = 52 + age * 8;
            using var edgePen = new Pen(edge, 10);
            using var mainPen = new Pen(main, 4);
            g.DrawLine(edgePen, center.X - size, center.Y - size / 2, center.X + size, center.Y + size / 2);
            g.DrawLine(mainPen, center.X - size, center.Y - size / 2, center.X + size, center.Y + size / 2);
            g.DrawLine(edgePen, center.X - size / 2, center.Y + size, center.X + size / 2, center.Y - size);
            g.DrawLine(mainPen, center.X - size / 2, center.Y + size, center.X + size / 2, center.Y - size);
            using var particle = new SolidBrush(edge);
            for (int i = 0; i < 8; i++)
            {
                double angle = i * Math.PI / 4;
                int distance = 28 + age * 10;
                int x = center.X + (int)(Math.Cos(angle) * distance);
                int y = center.Y + (int)(Math.Sin(angle) * distance);
                g.FillRectangle(particle, x - 3, y - 3, 7, 7);
            }
        }

        string result = _feedbackBlocked ? "BLOK"
            : !_feedbackHit ? "ISKA"
            : _feedbackCritical ? $"KRİTİK!  -{_feedbackDamage}"
            : $"-{_feedbackDamage} CAN";
        Color color = _feedbackBlocked ? Color.LightSteelBlue
            : !_feedbackHit ? UiTheme.Muted
            : _feedbackCritical ? Color.FromArgb(255, 230, 75) : Color.White;
        int rise = age * 4;
        UiTheme.DrawTextCentered(g, result, _feedbackCritical ? UiTheme.Title : UiTheme.Heading, color,
            new Rectangle(target.X - 25, target.Y - 34 - rise, target.Width + 50, 42));
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
        string playerStatuses = StatusText(Player.StatusEffects);
        if (playerStatuses.Length > 0)
        {
            using var statusInk = new SolidBrush(Color.FromArgb(220, 18, 14, 20));
            g.FillRectangle(statusInk, 188, 108, 370, 22);
            UiTheme.DrawTextCentered(g, Shorten(playerStatuses, 44), UiTheme.Small, UiTheme.Success, new Rectangle(190, 108, 366, 22));
        }
        using var turnInk = new SolidBrush(Color.FromArgb(20, 14, 10)); g.FillRectangle(turnInk, 579, 38, 24, 22); UiTheme.DrawText(g, _turn.ToString(), UiTheme.Heading, Color.FromArgb(250, 220, 60), 580, 37);
    }
    private void DrawInitiative(Graphics g)
    {
        if (Group) { DrawGroupOrder(g); return; }
        Image? player = Images.Get(CharacterArt.PortraitPath(Player.Race.Id, Player.Gender, _session.PortraitIndex));
        Image? enemy = EnemyImage("idle", trim: true);
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
        for (int i = 0; i < (Group ? _order.Length : InitiativeCards.Length); i++)
            if ((Group ? TurnCard(i) : new Rectangle(InitiativeCards[i], 140, 44, 42)).Contains(p)) { hit = i; break; }
        if (hit != _hoveredInitiative) { _hoveredInitiative = hit; Invalidate(); }
    }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hoveredInitiative = -1; Invalidate(); }
    private void DrawEnemyPanel(Graphics g)
    {
        int slot = Group && Inspected >= 0 ? Inspected : FocusSlot;
        var inspected = _members[slot];
        bool inspectingPlayer = Group && Inspected < 0;
        ICombatant npc = inspectingPlayer ? Player : inspected.Npc;
        using var nameInk = new SolidBrush(Color.FromArgb(44, 4, 64)); g.FillRectangle(nameInk, 644, 148, 130, 34);
        Font font = g.MeasureString(npc.Name.ToUpperInvariant(), UiTheme.Body).Width > 126 ? UiTheme.Small : UiTheme.Body;
        UiTheme.DrawTextCentered(g, npc.Name.ToUpper(Turkish), font, UiTheme.Text, new(644, 148, 130, 34));
        using var portraitInk = new SolidBrush(Color.FromArgb(30, 28, 34)); g.FillRectangle(portraitInk, 646, 204, 124, 108);
        Image? portrait = inspectingPlayer
            ? Images.Get(CharacterArt.PortraitPath(Player.Race.Id, Player.Gender, _session.PortraitIndex), trim: true)
            : EnemyImage("idle", slot, trim: true);
        UiTheme.DrawImageFit(g, portrait, new(650, 206, 116, 104));
        using var ink = new SolidBrush(LogInk); g.FillRectangle(ink, 662, 340, 118, 36); UiTheme.DrawText(g, "CAN", UiTheme.Small, UiTheme.Text, 666, 340);
        UiTheme.DrawBar(g, new(664, 358, 72, 12), npc.Health, npc.MaxHealth, UiTheme.Danger); UiTheme.DrawText(g, $"{npc.Health}/{npc.MaxHealth}", UiTheme.Small, UiTheme.Text, 738, 356);
        g.FillRectangle(ink, 740, 384, 40, 22); UiTheme.DrawText(g, npc.ArmorClass.ToString(), UiTheme.Body, UiTheme.Text, 752, 385);
        g.FillRectangle(ink, 636, 414, 144, 32); UiTheme.DrawImageFit(g, Images.Get($"{DiceFolder}runtime-256/d{npc.DamageDie}-icon-v1.png"), new(640, 416, 26, 26));
        UiTheme.DrawText(g, $"HASAR d{npc.DamageDie}", UiTheme.Small, UiTheme.Text, 672, 422);
        using var intentInk = new SolidBrush(Color.FromArgb(28, 24, 30)); g.FillRectangle(intentInk, 636, 447, 144, 28);
        string status = StatusText(npc.StatusEffects);
        if (Group && status.Length == 0 && !inspectingPlayer) status = inspected.Rear ? "ARKA SAF · MENZİLLİ" : "ÖN SAF";
        UiTheme.DrawTextCentered(g, inspectingPlayer ? "OYUNCU" : Shorten($"N: {inspected.Intent.Name}", 20), UiTheme.Small, UiTheme.Gold,
            new Rectangle(638, 447, 140, status.Length > 0 ? 15 : 28));
        if (status.Length > 0)
            UiTheme.DrawTextCentered(g, Shorten(status, 20), UiTheme.Small, UiTheme.Danger, new Rectangle(638, 461, 140, 15));
    }
    private void DrawLog(Graphics g)
    {
        using var ink = new SolidBrush(LogInk); g.FillRectangle(ink, 636, 478, 142, 170); var die = new Rectangle(682, 482, 52, 52);
        int ticks = _rollTicks;
        int duration = 24;
        if (!_feedbackFromPlayer && _feedbackTicks > ImpactTick)
        {
            ticks = 6 + _feedbackTicks - ImpactTick;
            duration = 6 + FeedbackDuration - ImpactTick;
        }
        RollingD20.Draw(g, die, _lastRoll, ticks, duration);
        if (!RollingD20.Landed(ticks)) return;
        for (int i = 0; i < _log.Count; i++) { string line = _log[i]; Color c = line.Contains("ISKA") ? UiTheme.Muted : line.Contains("İSABET") || line.Contains("KRİTİK") ? UiTheme.Success : UiTheme.Text; UiTheme.DrawText(g, "›", UiTheme.Small, UiTheme.Gold, 640, 538 + i * 18); UiTheme.DrawText(g, line, UiTheme.Small, c, 650, 538 + i * 18); }
    }
    protected override void DrawModal(Graphics g)
    {
        UiTheme.Shade(g, CanvasRectangle, 170);
        string asset = _modal switch
        {
            CombatModal.Attack or CombatModal.Power => "assets/final-production-v5/ui/modals/runtime-800/attack-selection-panel-v1.png",
            CombatModal.Inventory => "assets/final-production-v5/ui/modals/runtime-800/combat-inventory-panel-v1.png",
            CombatModal.Escape or CombatModal.FateStrike => "assets/final-production-v5/ui/modals/runtime-800/escape-confirm-panel-v1.png",
            _ => "assets/final-production-v5/ui/modals/runtime-800/dice-result-panel-v1.png"
        };
        UiTheme.DrawImage(g, Images.Get(asset), new(150, 100, 500, 570));
        if (_modal == CombatModal.Attack)
        {
            UiTheme.DrawTextCentered(g, Group ? $"HEDEF · {_enemy.Name}" : "SALDIRI SEÇ", Group ? UiTheme.Heading : UiTheme.Title, UiTheme.Gold, new(200, 185, 400, 50));
            if (Group) g.FillRectangle(Brushes.Black, 230, 251, 340, 52);
            UiTheme.DrawTextCentered(g, !CanReach(WeaponRanged) && Blocker is Member guard ? $"Ön safı aşmalısın · {guard.Npc.Name}" : "İsabet için d20, hasar için gösterilen zar atılır.", UiTheme.Small, UiTheme.Text, new(190, 249, 420, 30));
            if (Player.EquippedWeapon is Weapon weapon)
                UiTheme.DrawTextCentered(g, $"{weapon.Technique.Name.ToUpper(Turkish)} · {weapon.Technique.Description}",
                    UiTheme.Small, UiTheme.Gold, new(190, 275, 420, 28));
        }
        else if (_modal == CombatModal.Power)
        {
            ClassPower power = ClassPowerCatalog.For(Player.Class);
            UiTheme.DrawTextCentered(g, "SINIF GÜCÜ", UiTheme.Title, UiTheme.Gold, new(200, 175, 400, 50));
            UiTheme.DrawTextCentered(g, power.Name.ToUpper(Turkish), UiTheme.Heading, UiTheme.Success, new(205, 245, 390, 42));
            string description = !PowerAllowed(power) && Blocker is Member protector
                ? $"Ön safı aşmalısın · {protector.Npc.Name}" : power.Description;
            if (Group && power is AttackClassPower) description = $"Hedef: {_enemy.Name}\n{description}";
            UiTheme.DrawWrappedCentered(g, description, UiTheme.Body, UiTheme.Text, new(225, 305, 350, 90));
            string detail = power switch
            {
                AttackClassPower a => $"İSABET {Player.AttackBonus + a.AttackAdjustment + StoryAttackBonus:+#;-#;0} · HASAR d{a.DamageDie(Player)}",
                HealingClassPower h => $"İYİLEŞTİRME d{h.DieSides}+{h.FixedBonus + Math.Max(0, PrimaryAbilityModifier())}",
                GuardClassPower guard => $"BİR SALDIRI BOYUNCA AC +{guard.ArmorBonus}",
                _ => ""
            };
            UiTheme.DrawTextCentered(g, detail, UiTheme.Small, UiTheme.Gold, new(205, 410, 390, 34));
            UiTheme.DrawTextCentered(g, $"GÜÇ {Player.PowerPoints}/{Player.MaxPowerPoints} · BEDEL {power.Cost}",
                UiTheme.Small, Player.PowerPoints >= power.Cost ? UiTheme.Success : UiTheme.Danger, new(205, 450, 390, 30));
        }
        else if (_modal == CombatModal.Dice)
        {
            UiTheme.DrawTextCentered(g, _lastAttackName, UiTheme.Heading, UiTheme.Gold, new(291, 151, 218, 33));
            bool reveal = DrawRollingDie(g, _lastAttack?.RawValue);
            if (reveal && _lastAttack is { } roll)
            {
                DrawRollRow(g, 0, $"d20 {roll.RawValue} {roll.Bonus:+#;-#;+0}", roll.Total.ToString());
                DrawRollRow(g, 1, "HEDEF AC", roll.TargetArmorClass.ToString());
                DrawRollRow(g, 2, roll.IsCritical ? "KRİTİK" : roll.IsHit ? "İSABET" : "ISKA", $"{_lastDamage}");
            }
            else UiTheme.DrawTextCentered(g, "ZAR ATILIYOR...", UiTheme.Body, UiTheme.Gold, new(288, 532, 224, 37));
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
            UiTheme.DrawTextCentered(g, "KAÇIŞ KONTROLÜ", UiTheme.Heading, UiTheme.Gold, new(291, 151, 218, 33));
            bool reveal = DrawRollingDie(g, _fleeRoll?.RawValue);
            if (reveal && _fleeRoll is not null)
            {
                DrawRollRow(g, 0, $"d20 {_fleeRoll.RawValue} {_fleeRoll.Bonus:+#;-#;+0}", _fleeRoll.Total.ToString());
                DrawRollRow(g, 1, "ZORLUK", "12");
            }
            else UiTheme.DrawTextCentered(g, "ZAR ATILIYOR...", UiTheme.Body, UiTheme.Gold, new(288, 532, 224, 37));
        }
        else if (_modal == CombatModal.Victory)
        {
            bool kneels = _battle?.EnemyRemainsOnDefeat == true;
            UiTheme.DrawTextCentered(g, kneels ? "DİZ ÇÖKTÜ" : "ZAFER", UiTheme.Title, UiTheme.Gold, new(200, 190, 400, 50));
            string reward = _enemyFled
                ? $"{_enemy.Name} kaçtı · +{_enemy.RewardGold / 2} ALTIN · +{_victoryExperience} XP"
                : $"{_enemy.Name} yenildi · +{_enemy.RewardGold} ALTIN · +{_victoryExperience} XP";
            if (Group) reward = $"Çatışma bitti · +{_victoryGold} ALTIN · +{_victoryExperience} XP";
            if (kneels)
                UiTheme.DrawWrappedCentered(g, $"{_enemy.Name} sıfır cana indi ama ölmedi. Kaderine sen karar vereceksin.",
                    UiTheme.Body, UiTheme.Success, new(215, 300, 370, 120));
            else if (_loot is not null)
            {
                var iconArea = new Rectangle(350, 250, 100, 100);
                DrawLootGlow(g, iconArea, InventoryScreen.RarityColor(_loot.Rarity));
                iconArea.Offset(0, LootOffset[Math.Min(_lootTicks, LootOffset.Length - 1)]);
                UiTheme.DrawImageFit(g, Images.Get(InventoryScreen.ItemIcon(_loot)), Rectangle.Inflate(iconArea, -6, -6));
                using var rarityPen = new Pen(InventoryScreen.RarityColor(_loot.Rarity), 4);
                g.DrawRectangle(rarityPen, iconArea);
                UiTheme.DrawWrappedFit(g, $"{LootGenerator.RarityLabel(_loot.Rarity)} · {_loot.Name}", UiTheme.Body, UiTheme.Small,
                    InventoryScreen.RarityColor(_loot.Rarity), new Rectangle(230, 358, 340, 52));
                UiTheme.DrawTextCentered(g, reward, UiTheme.Small, UiTheme.Success, new Rectangle(220, 414, 360, 34));
            }
            else UiTheme.DrawWrappedCentered(g, reward, UiTheme.Body, UiTheme.Success, new(215, 300, 370, 120));
            if (_levelsGained > 0)
                UiTheme.DrawTextCentered(g, $"SEVİYE {Player.Level}! CAN VE SINIF GÜCÜ ARTTI.", UiTheme.Small, UiTheme.Gold,
                    new Rectangle(220, 455, 360, 30));
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
        EnemyTurn(() => { });
        if (_beat == CombatBeat.Judgment) { _beat = CombatBeat.None; _afterBeat = null; _timer.Stop(); OpenFateStrike(); }
    }
    private static string StatusText(StatusEffectCollection effects) => string.Join(" · ",
        effects.Active.Select(x => $"{x.Name.ToUpper(Turkish)} {x.RemainingTurns}"));
    private static string Shorten(string text, int length) => text.Length <= length ? text : text[..Math.Max(1, length - 1)] + "…";
    protected override void Dispose(bool disposing)
    {
        if (disposing) { _flashTimer.Dispose(); _visualTimer.Dispose(); _timer.Dispose(); }
        base.Dispose(disposing);
    }
}
