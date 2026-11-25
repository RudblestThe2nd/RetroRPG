using RetroRpg.Core.Characters;
using RetroRpg.Core.Common;

namespace RetroRpg.Core.Combat;

public enum EnemyActionKind { Attack, Guard, Flee }

public sealed record EnemyAction(
    string Name,
    EnemyActionKind Kind = EnemyActionKind.Attack,
    int AttackAdjustment = 0,
    int DamageDieAdjustment = 0,
    int DamageBonusAdjustment = 0,
    Func<StatusEffect>? EffectOnHit = null);

// Düşmanın sıradaki hamlesini seçer. CombatScreen yalnız dönen EnemyAction'ı uygular.
public abstract class EnemyBehavior(string name)
{
    public string Name { get; } = name;
    public abstract EnemyAction Choose(EnemyNpc enemy, ICombatant target, int turn);
}

public sealed class AggressiveBehavior : EnemyBehavior
{
    public AggressiveBehavior() : base("Saldırgan") { }
    public override EnemyAction Choose(EnemyNpc enemy, ICombatant target, int turn) =>
        target.Health * 2 <= target.MaxHealth || turn % 3 == 0
            ? new("AĞIR SALDIRI", AttackAdjustment: -1, DamageDieAdjustment: 2, DamageBonusAdjustment: 2)
            : new("SALDIRI");
}

public sealed class CowardlyBehavior : EnemyBehavior
{
    public CowardlyBehavior() : base("Korkak") { }
    public override EnemyAction Choose(EnemyNpc enemy, ICombatant target, int turn) =>
        turn > 1 && enemy.Health * 4 <= enemy.MaxHealth
            ? new("KAÇMAYA HAZIRLANIYOR", EnemyActionKind.Flee)
            : new("TEMKİNLİ VURUŞ", AttackAdjustment: 1, DamageDieAdjustment: -2);
}

public sealed class GuardianBehavior : EnemyBehavior
{
    public GuardianBehavior() : base("Muhafız") { }
    public override EnemyAction Choose(EnemyNpc enemy, ICombatant target, int turn) =>
        turn % 3 == 1
            ? new("SİPER ALIYOR", EnemyActionKind.Guard)
            : new("KARŞI DARBE", DamageBonusAdjustment: 1);
}

public sealed class PredatorBehavior : EnemyBehavior
{
    public PredatorBehavior() : base("Avcı") { }
    public override EnemyAction Choose(EnemyNpc enemy, ICombatant target, int turn)
    {
        bool wounded = target.Health * 2 <= target.MaxHealth || target.StatusEffects.Has<BleedingEffect>();
        return wounded
            ? new("YARALI AVA SALDIRI", AttackAdjustment: 2, DamageBonusAdjustment: 2, EffectOnHit: () => new BleedingEffect())
            : new("PENÇE", EffectOnHit: () => new BleedingEffect(2));
    }
}

public sealed class CasterBehavior(Biome biome) : EnemyBehavior("Büyücü")
{
    public override EnemyAction Choose(EnemyNpc enemy, ICombatant target, int turn)
    {
        if (turn % 2 == 1) return new("BÜYÜ ODAKLIYOR", AttackAdjustment: 1);
        Func<StatusEffect> effect = biome switch
        {
            Biome.Volcanic => () => new BurningEffect(),
            Biome.Snow => () => new FrozenEffect(),
            Biome.Swamp or Biome.Tropical => () => new PoisonEffect(),
            Biome.Corrupted => () => new CurseEffect(),
            _ => () => new StunEffect()
        };
        return new("DURUM BÜYÜSÜ", DamageDieAdjustment: -2, EffectOnHit: effect);
    }
}

public static class EnemyBehaviorCatalog
{
    public static EnemyBehavior For(EnemyDefinition definition)
    {
        string id = definition.Id;
        if (definition.ClassId is "wizard" or "sorcerer" or "warlock" or "druid" ||
            ContainsAny(id, "witch", "shaman", "cultist", "oracle", "elemental"))
            return new CasterBehavior(definition.Biome);
        if (ContainsAny(id, "goblin", "bandit", "raider")) return new CowardlyBehavior();
        if (ContainsAny(id, "guardian", "golem", "knight", "armored", "brute")) return new GuardianBehavior();
        if (ContainsAny(id, "wolf", "hound", "panther", "serpent", "spider", "stalker")) return new PredatorBehavior();
        return new AggressiveBehavior();
    }

    private static bool ContainsAny(string value, params string[] parts) =>
        parts.Any(x => value.Contains(x, StringComparison.OrdinalIgnoreCase));
}
