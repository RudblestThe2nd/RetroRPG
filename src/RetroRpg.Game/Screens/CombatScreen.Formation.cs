using RetroRpg.Core.Characters;
using RetroRpg.Core.Combat;
using RetroRpg.Core.Items;
using RetroRpg.Core.Story;
using RetroRpg.Core.World;

namespace RetroRpg.Game.Screens;

internal sealed partial class CombatScreen
{
    private sealed class Member(EnemyNpc npc, EnemyDefinition? definition, bool rear)
    {
        public EnemyNpc Npc { get; } = npc;
        public EnemyDefinition? Definition { get; } = definition;
        public bool Rear { get; } = rear;
        public bool Fled { get; set; }
        public bool Present => Npc.IsAlive && !Fled;
        public EnemyAction Intent { get; set; } = new("SALDIRI");
    }

    private readonly Member[] _members;
    private readonly int[] _order; // -1 oyuncu, diğer değerler sabit sahne yuvaları.
    private int _cursor, _selected, _acting = -1, _victoryGold;
    private bool _rangedPower;
    private bool Group => _members.Length > 1;
    private int FocusSlot => _acting >= 0 ? _acting : _selected;
    private bool WeaponRanged => Player.EquippedWeapon?.Technique is RangedTechnique;
    private Member? Blocker => _members[_selected].Rear ? _members.FirstOrDefault(x => !x.Rear && x.Present) : null;
    private bool CanReach(bool ranged) => _members[_selected].Present && (ranged || Blocker is null);
    private bool PowerAllowed(ClassPower power) => power is not AttackClassPower attack || CanReach(attack.IsRanged);

    private Member[] MakeFormation(MainGameForm host, HexTile tile, CombatStoryEvent? battle)
    {
        if (battle != null) return [new(battle.CreateEnemy(_session), null, false)];
        var lead = MapScreen.EncounterFor(host, tile);
        int distance = tile.Coordinate.DistanceTo(default);
        int seed = Math.Abs(tile.Coordinate.Q * 31 + tile.Coordinate.R * 17);
        if (distance < 3 || seed % 3 == 0 || tile.Coordinate == HexMap.EyeTower)
            return [new(EnemyCatalog.Create(lead), lead, false)];

        var local = host.Enemies.Enemies.Where(e => e.Biome == tile.Biome).ToList();
        static bool Ranged(EnemyDefinition e) => e.ClassId == "ranger" || e.Id is "bandit-scout" or "highway-bandit" ||
            EnemyBehaviorCatalog.For(e) is CasterBehavior;
        var front = Ranged(lead) ? local.First(e => !Ranged(e) && e.Tier == "common") : lead;
        var rear = Ranged(lead) ? lead : local.Where(Ranged).OrderBy(e => e.Tier == "rare" ? 2 : e.Tier == "elite" ? 1 : 0).First();
        var formation = new List<Member> { new(EnemyCatalog.Create(front), front, false) };
        if (distance >= 5 && seed % 2 == 0)
        {
            var middle = local.First(e => e.Id != front.Id && e.Tier == "common" && !Ranged(e));
            formation.Add(new(EnemyCatalog.Create(middle), middle, false));
        }
        var shooter = EnemyCatalog.Create(rear);
        if (shooter.Behavior is not CasterBehavior)
            shooter.Weapon = new Weapon("light-crossbow", "Arbalet", 0, 0, shooter.DamageDie);
        formation.Add(new(shooter, rear, true));
        return formation.ToArray();
    }

    private void NextGroupActor(Action playerReady)
    {
        do { _cursor = (_cursor + 1) % _order.Length; }
        while (_order[_cursor] >= 0 && !_members[_order[_cursor]].Present);
        _acting = _order[_cursor];
        if (_acting >= 0)
        {
            RunEnemyAction(() => NextGroupActor(playerReady));
            return;
        }
        if (!_members[_selected].Present) _selected = Array.FindIndex(_members, x => x.Present);
        bool skip = AdvanceStatuses(Player);
        if (!Player.IsAlive) { Host.ShowDeath("Savaş yaraları"); return; }
        if (skip)
        {
            _turn++;
            PlayBeat(CombatBeat.PlayerStun, () => NextGroupActor(playerReady));
        }
        else playerReady();
    }

    private void ResolveDeparture(bool fled, Action resume)
    {
        _members[FocusSlot].Fled = fled;
        if (!_members.Any(x => x.Present)) { BeginVictory(fled); return; }
        void ReturnToFight()
        {
            _deathTicks = -1;
            if (!_members[_selected].Present) _selected = Array.FindIndex(_members, x => x.Present);
            resume();
        }
        if (fled) ReturnToFight();
        else { _deathTicks = 0; PlayBeat(CombatBeat.Dissolve, ReturnToFight); }
    }

    private Rectangle SlotBounds(int slot)
    {
        if (!Group) return new(340, 384, 256, 256);
        if (_members[slot].Rear) return new(447, 285, 154, 154);
        return slot == 0 ? new(250, 435, 200, 200) : new(354, 366, 172, 172);
    }

    private Rectangle TurnCard(int index) => new(49 + index * 70, 140, 51, 42);
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (!Group || ModalOpen || _timer.Enabled || _acting >= 0 || e.Button != MouseButtons.Left) return;
        Point at = ToLogical(e.Location);
        for (int i = 0; i < _order.Length; i++)
            if (TurnCard(i).Contains(at)) { SelectTarget(_order[i]); return; }
        for (int i = 0; i < _members.Length; i++)
            if (_members[i].Present && SlotBounds(i).Contains(at)) { SelectTarget(i); return; }
    }

    private void SelectTarget(int slot)
    {
        if (slot >= 0 && _members[slot].Present) { _selected = slot; Invalidate(); }
    }

    private int Inspected => Group && _hoveredInitiative >= 0 && _hoveredInitiative < _order.Length
        ? _order[_hoveredInitiative] : FocusSlot;

    private void DrawGroupOrder(Graphics g)
    {
        using var ink = new SolidBrush(Color.FromArgb(34, 9, 45));
        g.FillRectangle(ink, 32, 130, 552, 70);
        for (int i = 0; i < _order.Length; i++)
        {
            int slot = _order[i];
            Rectangle card = TurnCard(i);
            Image? portrait = slot < 0
                ? Images.Get(CharacterArt.PortraitPath(Player.Race.Id, Player.Gender, _session.PortraitIndex), trim: true)
                : EnemyImage("idle", slot, trim: true);
            UiTheme.DrawImageFit(g, portrait, card);
            if (slot >= 0 && !_members[slot].Present) UiTheme.Shade(g, card, 200);
            g.DrawRectangle(i == _hoveredInitiative ? Pens.White : Pens.DarkGoldenrod, card);
            if (slot == _acting)
                g.FillPolygon(Brushes.Gold, new Point[] { new(card.X+18,131),new(card.X+32,131),new(card.X+25,139) });
        }
        var seen = Inspected;
        ICombatant actor = seen < 0 ? Player : _members[seen].Npc;
        UiTheme.DrawTextCentered(g, $"{actor.Name} · CAN {actor.Health}/{actor.MaxHealth}",
            UiTheme.Small, UiTheme.Text, new(45,182,315,17));
        UiTheme.DrawTextCentered(g, $"HEDEF · {_members[_selected].Npc.Name}", UiTheme.Small, UiTheme.Gold, new(352,148,224,36));
    }

    internal void CheckFormation(Action<string> capture)
    {
        static void Require(bool ok, string step)
        {
            if (!ok) throw new InvalidOperationException(step);
        }
        void Settle()
        {
            for (int i = 0; _timer.Enabled && i < 300; i++) Tick();
            Require(!_timer.Enabled && Player.IsAlive, "Savaş sırası oyuncuya dönemedi.");
        }
        Settle();
        Require(_members.Length == 3 && _order.Distinct().Count() == 4, "Diziliş veya inisiyatif eksik.");
        var sword = new Weapon("greatsword", "Büyük Kılıç", 0, 4, 12);
        Player.Equipment.Equip(sword, sword.Slot);
        int rear = _members.Length - 1;
        SelectTarget(rear);
        capture("01-arka-saf");
        OpenModal(CombatModal.Attack);
        int turn = _turn, hp = _enemy.Health, power = Player.PowerPoints;
        ChooseAttack("NORMAL", 0, 12, 0);
        UsePower();
        Require(_turn == turn && _enemy.Health == hp && Player.PowerPoints == power &&
            !_modalActions[0].Enabled && _modal == CombatModal.Attack, "Ön saf yakın saldırıyı engellemedi.");
        capture("02-korunan-hedef");
        CloseModal();
        var bow = new Weapon("light-crossbow", "Arbalet", 0, 1, 8);
        Player.Equipment.Equip(bow, bow.Slot);
        OpenModal(CombatModal.Attack);
        Require(_modalActions[0].Enabled && PowerAllowed(ClassPowerCatalog.For(new WizardClass())), "Menzilli hedef engellendi.");
        ChooseAttack("ATIŞ", 0, 2, 0);
        Require(_modal == CombatModal.Dice, "Arbalet atışı başlamadı.");
        Settle();
        CompletePlayerResolution();
        Settle();
        Require(_acting < 0 && _selected == rear && _turn == turn + 1, "Hedef veya sıra kayboldu.");
        Player.Equipment.Equip(sword, sword.Slot);
        int gold = Player.Gold;
        for (int i = 0; i < rear; i++)
        {
            SelectTarget(i);
            _enemy.ReceiveDamage(_enemy.Health);
            CompletePlayerResolution();
            Settle();
            SelectTarget(rear);
            Require(CanReach(false) == (i == rear - 1) && Player.Gold == gold, "Ön saf veya ara zafer hatalı.");
        }
        capture("03-acilan-arka-saf");
        _enemy.ReceiveDamage(_enemy.Health);
        CompletePlayerResolution();
        Settle();
        Require(_modal == CombatModal.Victory && Player.Gold == gold + _victoryGold, "Grup ödülü hatalı.");
    }
}
