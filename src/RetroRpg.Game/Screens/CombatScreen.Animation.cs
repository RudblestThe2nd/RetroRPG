using RetroRpg.Core.Combat;
using RetroRpg.Core.Items;

namespace RetroRpg.Game.Screens;

internal sealed partial class CombatScreen
{
    private enum CombatBeat { None, Guard, Retreat, Heal, Bless, EnemyStun, PlayerStun, Judgment, Unveil, Kneel, Dissolve }
    private readonly System.Windows.Forms.Timer _visualTimer = new() { Interval = 120 };
    private readonly System.Windows.Forms.Timer _flashTimer = new() { Interval = 10 };
    private CombatBeat _beat;
    private Action? _afterBeat;
    private int _beatTick, _visualFrame, _lootTicks = 9;
    private bool _armorIntercept, _castAttack;
    private static readonly int[] LootOffset = [-42, -39, -32, -21, -7, 0, -6, -3, 0];
    private static readonly int[] ReachPercent = [0, -5, -8, 14, 55, 100, 100, 88, 66, 42, 17, 4];
    private static readonly int[] HammerArc = [-35, -50, -66, -78, -44, 22, 34, 38, 34, 24, 10, 0];

    private void PlayBeat(CombatBeat beat, Action done)
    {
        _beat = beat;
        _beatTick = 0;
        _afterBeat = done;
        _modal = CombatModal.None;
        ModalOpen = false;
        ConfigureModalButtons();
        _timer.Start();
        UpdateActions();
    }

    private void OpenFateStrike()
    {
        _modal = CombatModal.FateStrike;
        ModalOpen = true;
        ConfigureModalButtons();
        UpdateActions();
    }

    // Saldırıların gerçek çizim ve Tick yolunu sabit bir isabetle kare kare denetler.
    internal void CaptureWeaponMotion(Action<string> capture)
    {
        _visualTimer.Stop();
        _beat = CombatBeat.None;
        _rollTicks = 0;
        string[] weapons = ["unarmed", "dagger", "rapier", "mace", "warhammer", "greatsword", "light-crossbow"];
        foreach (string id in weapons)
        {
            if (id != "unarmed")
            {
                var weapon = new Weapon(id, id, 0, 1, 6);
                Player.Equipment.Equip(weapon, weapon.Slot);
            }
            int completions = 0;
            BeginFeedback(true, new AttackRoll(14, 0, 10), 5, () => completions++);
            _timer.Stop();
            for (int i = 0; i < FeedbackDuration; i++)
            {
                capture($"{id}-{i:00}");
                Tick();
            }
            if (completions != 1 || !_feedbackSoundPlayed)
                throw new InvalidOperationException($"{id} saldırı animasyonunu tamamlamadı.");
        }
    }

    private string WeaponId => _feedbackFromPlayer ? Player.EquippedWeapon?.AssetId ?? "" : _enemy.Weapon?.AssetId ?? "";

    internal void CheckWounds(Action<string> capture)
    {
        for (int i = 0; _timer.Enabled && i < 300; i++) Tick();
        _visualTimer.Stop();
        SelectTarget(0);
        Player.Heal(Player.MaxHealth);
        var sword = new Weapon("greatsword", "Büyük Kılıç", 0, 4, 12);
        Player.Equipment.Equip(sword, sword.Slot);
        capture("00-saglikli");
        ICombatant front = _enemy, rear = _members[^1].Npc;
        void Check(ICombatant actor, int stage, int flash = 0)
        {
            if (WoundsFor(actor) != new BattleWounds(stage, flash))
                throw new InvalidOperationException($"{actor.Name}: yara veya darbe karesi hatalı.");
        }
        void Strike(bool outgoing, int remaining, int oldStage, int nextStage, string name)
        {
            ICombatant target = outgoing ? front : Player;
            int damage = target.Health - remaining;
            target.ReceiveDamage(damage);
            BeginFeedback(outgoing, new AttackRoll(19, 5, 10), damage, () => { });
            _timer.Stop();
            Check(target, oldStage);
            while (_feedbackTicks > ImpactTick) Tick();
            Check(target, nextStage, 3);
            capture(name + "-darbe");
            while (_feedbackTicks > 0) Tick();
            Check(target, nextStage);
            Check(rear, 0);
            capture(name + "-sonra");
        }
        Strike(true, front.MaxHealth * 3 / 4, 0, 1, "01-hafif");
        Strike(true, front.MaxHealth / 2, 1, 2, "02-orta");
        Strike(false, Player.MaxHealth * 3 / 4, 0, 1, "02b-oyuncu-hafif");
        Strike(false, Player.MaxHealth / 2, 1, 2, "02c-oyuncu-orta");
        Strike(false, Player.MaxHealth / 4, 2, 3, "03-agir");
        SelectTarget(_members.Length - 1);
        Check(front, 2);
        Check(rear, 0);
        BeginFeedback(false, new AttackRoll(1, 0, 10), 0, () => { });
        _timer.Stop();
        while (_feedbackTicks > ImpactTick) Tick();
        Check(Player, 3);
        while (_feedbackTicks > 0) Tick();
        Player.Heal(Player.MaxHealth);
        Check(Player, 0);
        capture("04-iyilesme");
    }
    private bool Casting => _feedbackFromPlayer ? _rangedPower : _castAttack;

    private int VisibleHealth(ICombatant actor)
    {
        // Zar sonucu önceden hesaplanır, yara ve düşme vuruşun temas karesinde görünür.
        if (_pendingPlayerFeedback && actor == _enemy) return actor.Health + _lastDamage;
        if (_feedbackTicks > ImpactTick && actor == (_feedbackFromPlayer ? (ICombatant)_enemy : Player))
            return actor.Health + _feedbackDamage;
        return actor.Health;
    }

    private BattleWounds WoundsFor(ICombatant actor)
    {
        bool struck = _feedbackHit && _feedbackDamage > 0 && actor == (_feedbackFromPlayer ? (ICombatant)_enemy : Player);
        int flash = struck && _flashTimer.Enabled && _feedbackTicks == ImpactTick ? 3 : 0;
        return BattleWounds.FromHealth(VisibleHealth(actor), actor.MaxHealth, flash);
    }


    // Soldan ve sağdan gelen saldırılar aynı kaçış yayını ters yönde kullanır.
    // Hazırlık, sıçrama, iniş ve başlangıç konumuna dönüş: her adım bir savaş karesi.
    private static readonly Point[] DodgeSteps =
    [
        new(0,0), new(0,0), new(-3,3), new(3,-4), new(19,-23), new(31,-40),
        new(34,-43), new(29,-35), new(19,-20), new(9,-8), new(3,1), new(0,0)
    ];

    internal void CheckDodge(Action<string> capture)
    {
        for (int i = 0; _timer.Enabled && i < 300; i++) Tick();
        _visualTimer.Stop();
        if (Group) SelectTarget(_members.Length - 1);
        var target = new Rectangle(0, 0, 256, 256);
        foreach (bool fromPlayer in new[] { true, false })
        {
            ICombatant defender = fromPlayer ? _enemy : Player;
            if (Group) defender.ReceiveDamage(Math.Max(0, defender.Health - defender.MaxHealth / 2));
            int hp = defender.Health;
            int completed = 0;
            BeginFeedback(fromPlayer, new AttackRoll(1, 0, 10), 0, () => completed++);
            _timer.Stop();
            for (int frame = 0; frame < FeedbackDuration; frame++)
            {
                Point offset = DodgeOffset(target);
                if (frame == FeedbackDuration - ImpactTick && (offset.Y >= 0 || (fromPlayer ? offset.X <= 0 : offset.X >= 0)))
                    throw new InvalidOperationException("Iskada hedef saldırı hattından çıkmadı.");
                if ((frame == 0 || frame == FeedbackDuration - 1) && offset != Point.Empty)
                    throw new InvalidOperationException("Kaçınma başlangıç noktasına dönmedi.");
                capture($"{(Group ? "grup" : "tek")}-{(fromPlayer ? "dusman" : "oyuncu")}-{frame:00}");
                Tick();
            }
            if (completed != 1 || defender.Health != hp) throw new InvalidOperationException("Iska çözümlemesi hatalı.");
        }
        _defending = true;
        BeginFeedback(false, new AttackRoll(1, 0, 10), 0, () => { });
        _timer.Stop();
        while (_feedbackTicks > 0)
        {
            if (DodgeOffset(target) != Point.Empty) throw new InvalidOperationException("Blokta kaçınma oynadı.");
            Tick();
        }
        _defending = false;
    }

    private Point DodgeOffset(Rectangle area)
    {
        if (_feedbackTicks <= 0 || _feedbackHit || _feedbackBlocked) return Point.Empty;
        Point step = DodgeSteps[FeedbackDuration - _feedbackTicks];
        int direction = _feedbackFromPlayer ? 1 : -1;
        return new(direction * step.X * area.Width / 256, step.Y * area.Height / 256);
    }

    private static void DrawDodgeTrail(Graphics g, Rectangle defender, Point offset)
    {
        if (offset.Y >= -4) return;
        int direction = Math.Sign(offset.X);
        int x = defender.X + defender.Width / 2 - direction * defender.Width / 7;
        int y = defender.Y + defender.Height * 3 / 5;
        using var streak = new Pen(Color.FromArgb(175, 226, 212, 179), 2);
        for (int i = 0; i < 3; i++)
        {
            int startY = y + i * 7;
            g.DrawLine(streak, x - offset.X / 2 - direction * i * 3, startY - offset.Y / 2, x, startY);
        }
    }

    private void PositionFighters(ref Rectangle player, ref Rectangle enemy)
    {
        if (_beat == CombatBeat.Retreat) enemy.Offset(_beatTick * _beatTick * 3, -(_beatTick % 3) * 3);
        if (_beat == CombatBeat.Guard || _enemy.DefenseBonus > 0)
            enemy = new Rectangle(enemy.X, enemy.Y + 14, enemy.Width, enemy.Height - 14);
        if (_beat == CombatBeat.Kneel || (_modal == CombatModal.Victory && _battle?.EnemyRemainsOnDefeat == true))
        {
            int drop = _beat == CombatBeat.Kneel ? _beatTick * 6 : 48;
            enemy = new Rectangle(enemy.X, enemy.Y + drop, enemy.Width, enemy.Height - drop);
        }
        if (_feedbackTicks <= 0) return;
        int age = FeedbackDuration - _feedbackTicks;
        int reach = Casting || WeaponId == "light-crossbow" ? 8 : WeaponId switch
        {
            "dagger" => 180, "rapier" => 141, "greatsword" => 123, "mace" or "warhammer" => 151, _ => 195
        };
        if (Group && !Casting && WeaponId != "light-crossbow")
        {
            int dx = enemy.X + enemy.Width / 2 - (player.X + player.Width / 2);
            reach = Math.Max(16, dx - (270 - reach));
            int dy = enemy.Y + enemy.Height / 2 - (player.Y + player.Height / 2);
            if (_feedbackFromPlayer) player.Offset(0, dy * ReachPercent[age] / 100);
            else enemy.Offset(0, -dy * ReachPercent[age] / 100);
        }
        int travel = reach * ReachPercent[age] / 100;
        int recoil = 0;
        if (_feedbackTicks <= ImpactTick)
            recoil = (!_feedbackHit || _feedbackDamage <= 0 ? 0 : _feedbackCritical ? 38 : 16) * _feedbackTicks / ImpactTick;
        player.Offset(_feedbackFromPlayer ? travel : -recoil, 0);
        enemy.Offset(_feedbackFromPlayer ? recoil : -travel, 0);
        if (_feedbackHit && _feedbackDamage > 0 && _feedbackTicks <= ImpactTick)
        {
            ref Rectangle target = ref (_feedbackFromPlayer ? ref enemy : ref player);
            int dip = _feedbackTicks switch { ImpactTick => 9, ImpactTick - 1 => 14, ImpactTick - 2 => 8, ImpactTick - 3 => 3, _ => 0 };
            int shake = _feedbackTicks switch { ImpactTick => 7, ImpactTick - 1 => -6, ImpactTick - 2 => 4, ImpactTick - 3 => -2, _ => 0 };
            target.X += _feedbackFromPlayer ? shake : -shake;
            target.Y += dip;
            target.Height -= dip;
        }
    }

    private GripPose PlayerGripPose(int age, bool attacking)
    {
        var poses = WeaponGrip.ForBody(Player.Race.Id, Player.Gender);
        if (!attacking || age <= 1 || age >= 10) return poses.Ready;
        if (Player.EquippedWeapon?.AssetId == "light-crossbow") return poses.Strike;
        return age <= 3 ? poses.Raised : poses.Strike;
    }

    private static void DrawDissolve(Graphics g, Image sprite, Rectangle area, int age)
    {
        for (int row = 0; row < 16; row++)
        {
            if ((row * 5 % 16) < age * 2) continue;
            int drift = row % 2 == 0 ? age * 2 : -age * 2;
            Rectangle strip = new(area.X + drift, area.Y + row * area.Height / 16 - age, area.Width, area.Height / 16 + 1);
            g.DrawImage(sprite, strip, new Rectangle(0, row * sprite.Height / 16, sprite.Width, sprite.Height / 16 + 1), GraphicsUnit.Pixel);
        }
    }

    private void DrawBattleMotion(Graphics g, Point p, Point e, int enemyBottom, Point dodge)
    {
        DrawStatuses(g, Player.StatusEffects, p);
        DrawStatuses(g, _enemy.StatusEffects, e);
        int phase = _visualFrame;
        if (_beat == CombatBeat.Guard || _enemy.DefenseBonus > 0) Shield(g, e, Color.SteelBlue, phase);
        if (_beat == CombatBeat.PlayerStun || _beat == CombatBeat.EnemyStun)
            Stars(g, _beat == CombatBeat.PlayerStun ? p : e, phase);
        if (_beat is CombatBeat.Heal or CombatBeat.Bless) Sparks(g, p, Color.Gold, _beatTick, 52);
        if (_beat == CombatBeat.Judgment)
        {
            if (_beatTick <= 3) Sparks(g, e, Color.MediumPurple, _beatTick, 62);
            else Beam(g, e, p, _beatTick);
        }
        if (_beat == CombatBeat.Unveil)
        {
            Sparks(g, e, Color.MediumPurple, _beatTick * 3, 100 - _beatTick * 7);
            Sparks(g, e, Color.Gold, -_beatTick, 27 + _beatTick * 7);
        }
        if (_feedbackTicks == 0)
        {
            if (_enemy.IsAlive && _beat == CombatBeat.None && !ModalOpen)
            {
                if (_enemyIntent.Kind == EnemyActionKind.Flee) Sparks(g, new(e.X, enemyBottom - 12), Color.Tan, phase, 25);
                if (_enemyIntent.Kind == EnemyActionKind.Guard) Shield(g, e, Color.SteelBlue, phase);
                if (_enemy.Behavior is CasterBehavior || _enemy.Weapon?.AssetId == "fate-ray")
                    Sparks(g, new(e.X - 36, e.Y - 10), Color.MediumPurple, phase, 21);
            }
            return;
        }
        Point source = _feedbackFromPlayer ? p : e;
        Point target = _feedbackFromPlayer ? e : p;
        target.Offset(-dodge.X, -dodge.Y);
        int facing = _feedbackFromPlayer ? 1 : -1;
        source.Offset(19 * facing, 0);
        int age = FeedbackDuration - _feedbackTicks;
        if (Casting)
        {
            if (_feedbackTicks > ImpactTick) Sparks(g, source, Color.Orchid, age, 24 + age * 3);
            else Beam(g, source, target, age);
        }
        else if (!_feedbackFromPlayer)
            DrawWeapon(g, WeaponId, source, target, facing, age, null, default);
        if (_armorIntercept && !_feedbackFromPlayer && _feedbackTicks <= ImpactTick) Shield(g, target, Color.Gold, age);
        if (_feedbackTicks <= ImpactTick && _feedbackHit && _feedbackDamage > 0 && _feedbackFromPlayer)
        {
            int elapsed = ImpactTick - _feedbackTicks;
            int frame = _rapidHit ? elapsed % 4 : Math.Min(3, elapsed / 2);
            int size = _impactArt == "critical" ? 118 : 90;
            int offset = _rapidHit ? (elapsed < 4 ? -16 : 16) : 0;
            string path = $"assets/final-production-v1/effects/frames/{_impactArt}/{_impactArt}-{frame:00}-v1.png";
            UiTheme.DrawImageFit(g, Images.Get(path), new(target.X + offset - size / 2, target.Y - size / 2, size, size));
        }
        else if (_feedbackTicks <= ImpactTick && _feedbackHit && _feedbackDamage > 0 && !_feedbackCritical)
        {
            int spread = 12 + (age - 5) * 4;
            if (WeaponId is "mace" or "warhammer")
            {
                using var ring = new Pen(Color.SandyBrown, 4);
                g.DrawEllipse(ring, target.X - spread, target.Y - spread / 2, 2 * spread, spread);
            }
            else if (WeaponId is "greatsword" or "rapier")
            {
                using var slash = new Pen(Color.LightSteelBlue, WeaponId == "greatsword" ? 7 : 3);
                g.DrawLines(slash, new Point[] { new(target.X - spread, target.Y - spread), target, new(target.X + spread / 2, target.Y + spread) });
            }
            else Sparks(g, target, Color.Wheat, age - 5, spread);
        }
    }

    private void DrawWeapon(Graphics g, string id, Point hand, Point target, int facing, int age,
        Bitmap? body, GripPose pose)
    {
        WeaponGrip grip = WeaponGrip.For(id);
        if (grip.Size == 0) return;
        int tick = Math.Max(0, age);
        int tilt = id switch { "mace" or "warhammer" => HammerArc[tick], "greatsword" => HammerArc[tick] - 8,
            "rapier" => HammerArc[tick] / 3, "dagger" => 5 - ReachPercent[tick] / 9, _ => 0 };
        bool supported = body is not null && id is "light-crossbow" or "greatsword" or "warhammer";
        Point support = hand;
        if (supported)
        {
            // Arbalette ikinci el kundak altında, ağır silahta sapın arka ucunda.
            int along = id == "light-crossbow" ? 25 : -14;
            double angle = tilt * Math.PI / 180;
            support.Offset((int)(Math.Cos(angle) * along), (int)(Math.Sin(angle) * along) + (along > 0 ? 4 : 0));
            Point elbow = new(hand.X - 21, hand.Y + 14);
            Color skin = body!.GetPixel(pose.X, pose.Y);
            using var outline = new Pen(Color.FromArgb(skin.R / 2, skin.G / 2, skin.B / 2), 9);
            using var forearm = new Pen(Color.FromArgb(skin.R * 4 / 5, skin.G * 4 / 5, skin.B * 4 / 5), 6);
            g.DrawLine(outline, elbow, support);
            g.DrawLine(forearm, elbow, support);
        }
        var state = g.Save();
        g.TranslateTransform(hand.X, hand.Y);
        g.ScaleTransform(facing, 1);
        g.RotateTransform(tilt + grip.TurnToRight);
        string path = $"assets/final-production-v2/equipment/weapons-extended/runtime-256/{id}/east/{id}-east-v1.png";
        UiTheme.DrawPixelatedSprite(g, Images.Get(path), new(-grip.X * grip.Size / 256, -grip.Y * grip.Size / 256, grip.Size, grip.Size), 2);
        g.Restore(state);
        // Sap avucun arkasında, eklemler önünde kalır. Renk ve gölge mevcut el karesinden gelir.
        if (body is not null)
        {
            int x = pose.X / 4 * 4, y = pose.Y / 4 * 4;
            for (int dy = -4; dy <= 4; dy += 4)
                for (int dx = -4; dx <= 4; dx += 4)
                {
                    if ((dy == -4 && dx == -4) || (dy == 4 && dx == 4)) continue;
                    Color pixel = body.GetPixel(x + dx + 2, y + dy + 2);
                    if (pixel.A < 96) continue;
                    using var ink = new SolidBrush(Color.FromArgb(pixel.R / 16 * 16, pixel.G / 16 * 16, pixel.B / 16 * 16));
                    g.FillRectangle(ink, hand.X + x - pose.X + dx, hand.Y + y - pose.Y + dy, 4, 4);
                    if (supported && dy >= 0)
                        g.FillRectangle(ink, support.X + dx, support.Y + dy - 3, 4, 4);
                }
        }
        if (id == "light-crossbow" && age is >= 2 and <= 5)
        {
            int start = hand.X + (255 - grip.X) * grip.Size / 256 * facing;
            int end = _feedbackHit ? target.X : target.X + 38 * facing;
            int x = start + (end - start) * (age - 2) / 3;
            int muzzleY = hand.Y + (113 - grip.Y) * grip.Size / 256;
            int endY = target.Y - (_feedbackHit ? 0 : 30);
            int y = muzzleY + (endY - muzzleY) * (age - 2) / 3;
            g.FillRectangle(Brushes.SaddleBrown, x - 17, y, 32, 4);
            g.FillRectangle(Brushes.Silver, x + facing * 15 - 4, y - 2, 8, 7);
        }
    }

    private void DrawStatuses(Graphics g, StatusEffectCollection effects, Point center)
    {
        foreach (StatusEffect effect in effects.Active)
        {
            int t = _visualFrame;
            switch (effect)
            {
                case PoisonEffect:
                    for (int i = 0; i < 4; i++)
                    {
                        int rise = (t * 4 + i * 15) % 56;
                        var bubble = new Rectangle(center.X - 37 + i * 22, center.Y + 24 - rise, 9, 9);
                        g.FillRectangle(Brushes.OliveDrab, bubble);
                        g.FillRectangle(Brushes.GreenYellow, bubble.X + 2, bubble.Y + 1, 3, 3);
                    }
                    break;
                case BleedingEffect:
                    for (int i = 0; i < 3; i++)
                        g.FillRectangle(Brushes.Firebrick, center.X + 6 * i, center.Y + (t * 5 + i * 19) % 58, 4, 9);
                    break;
                case BurningEffect:
                    for (int i = 0; i < 5; i++)
                    {
                        int h = 12 + ((i * 13 + t * 7) % 24);
                        int x = center.X - 32 + i * 14, y = center.Y + 62;
                        g.FillPolygon(Brushes.OrangeRed, new Point[] { new(x - 8,y), new(x + 4,y - h), new(x + 9,y) });
                        g.FillRectangle(Brushes.Gold, x, y - h / 3, 4, h / 3);
                    }
                    break;
                case FrozenEffect:
                    Sparks(g, center, Color.LightCyan, t / 2, 42);
                    for (int i = 0; i < 3; i++)
                    {
                        int x = center.X - 26 + i * 24, y = center.Y + 60;
                        g.FillPolygon(Brushes.SkyBlue, new Point[] { new(x-6,y), new(x+3,y-24-i*5), new(x+7,y-9), new(x+4,y) });
                    }
                    break;
                case StunEffect: Stars(g, center, t); break;
                case ArmorBreakEffect:
                    for (int i = 0; i < 4; i++)
                        g.FillRectangle(Brushes.SlateGray, center.X - 22 + i * 14, center.Y + (t * 4 + i * 13) % 32, 7, 4);
                    break;
                case BlessingEffect: Sparks(g, center, Color.Gold, t, 46); break;
                case CurseEffect: Sparks(g, center, Color.DarkOrchid, -t, 52); break;
            }
        }
    }

    private static void Sparks(Graphics g, Point center, Color color, int phase, int radius)
    {
        using var brush = new SolidBrush(color);
        for (int i = 0; i < 6; i++)
        {
            double angle = (i + phase / 9d) * Math.Tau / 6;
            int x = center.X + (int)(Math.Cos(angle) * radius);
            int y = center.Y + (int)(Math.Sin(angle) * radius * .72);
            g.FillRectangle(brush, x - 4, y, 10, 3);
            g.FillRectangle(brush, x, y - 4, 3, 10);
        }
    }

    private static void Stars(Graphics g, Point center, int phase) =>
        Sparks(g, new(center.X, center.Y - 84), Color.Khaki, phase * 2, 24);

    private static void Shield(Graphics g, Point center, Color color, int phase)
    {
        int x = center.X, y = center.Y;
        using var pen = new Pen(color, 3 + phase % 3);
        g.DrawPolygon(pen, new Point[] { new(x-36,y-42), new(x,y-50), new(x+36,y-42), new(x+31,y+15), new(x,y+45), new(x-31,y+15) });
    }

    private static void Beam(Graphics g, Point from, Point to, int phase)
    {
        Point bend = new((from.X + to.X) / 2, (from.Y + to.Y) / 2 - 9 + phase % 3 * 8);
        Point[] ray = [from, bend, to];
        using var outer = new Pen(Color.Indigo, 10);
        using var core = new Pen(Color.Plum, 3);
        g.DrawLines(outer, ray);
        g.DrawLines(core, ray);
    }

    private bool DrawRollingDie(Graphics g, int? result)
    {
        bool reveal = RollingD20.Landed(_rollTicks);
        if (result is int face)
        {
            if (reveal && face is 1 or 20)
                Sparks(g, new(400, 340), face == 20 ? Color.Gold : Color.IndianRed, 24 - _rollTicks, 102);
            RollingD20.Draw(g, new(305, 215, 190, 190), face, _rollTicks);
        }
        return reveal;
    }

    private static void DrawRollRow(Graphics g, int row, string label, string value)
    {
        int y = 420 + row * 36;
        UiTheme.DrawTextCentered(g, label, UiTheme.Body, UiTheme.Text, new(295, y, 146, 33));
        UiTheme.DrawTextCentered(g, value, UiTheme.Body, UiTheme.Gold, new(451, y, 52, 33));
    }

    private void DrawLootGlow(Graphics g, Rectangle box, Color color) =>
        Sparks(g, new(box.X + box.Width / 2, box.Y + box.Height / 2), color, _visualFrame, 66);
}
