namespace RetroRpg.Core.Combat;

public sealed record StatusTurnResult(string Text, int Damage, bool SkipsTurn);

// Bütün geçici savaş durumlarının ortak tabanı. Etki süresi sahibinin turu başlarken azalır.
public abstract class StatusEffect(string id, string name, int turns)
{
    public string Id { get; } = id;
    public string Name { get; } = name;
    public int RemainingTurns { get; private set; } = Math.Max(1, turns);
    public virtual int AttackModifier => 0;
    public virtual int ArmorModifier => 0;
    public virtual int DamageModifier => 0;
    public virtual int TurnDamage => 0;
    public virtual bool PreventsAction => false;

    internal StatusTurnResult Advance(ICombatant owner)
    {
        int before = owner.Health;
        if (TurnDamage > 0) owner.ReceiveDamage(TurnDamage);
        int damage = before - owner.Health;
        bool skip = PreventsAction;
        RemainingTurns--;
        string text = damage > 0 ? $"{Name.ToUpperInvariant()}: -{damage} CAN" : Name.ToUpperInvariant();
        return new StatusTurnResult(text, damage, skip);
    }

    internal void Refresh(int turns) => RemainingTurns = Math.Max(RemainingTurns, Math.Max(1, turns));
}

public abstract class DamageStatusEffect(string id, string name, int turns, int damage)
    : StatusEffect(id, name, turns)
{
    public override int TurnDamage { get; } = Math.Max(1, damage);
}

public sealed class PoisonEffect(int turns = 3) : DamageStatusEffect("poison", "Zehir", turns, 2);
public sealed class BleedingEffect(int turns = 3) : DamageStatusEffect("bleeding", "Kanama", turns, 1);
public sealed class BurningEffect(int turns = 2) : DamageStatusEffect("burning", "Yanma", turns, 3);

public abstract class ControlStatusEffect(string id, string name, int turns) : StatusEffect(id, name, turns);

public sealed class StunEffect(int turns = 1) : ControlStatusEffect("stun", "Sersemleme", turns)
{
    public override bool PreventsAction => true;
}

public sealed class FrozenEffect(int turns = 2) : ControlStatusEffect("frozen", "Donma", turns)
{
    public override int AttackModifier => -2;
    public override int ArmorModifier => -1;
}

public abstract class StatStatusEffect(string id, string name, int turns) : StatusEffect(id, name, turns);

public sealed class ArmorBreakEffect(int turns = 2) : StatStatusEffect("armor-break", "Zırh Kırılması", turns)
{
    public override int ArmorModifier => -2;
}

public sealed class BlessingEffect(int turns = 3) : StatStatusEffect("blessing", "Kutsama", turns)
{
    public override int AttackModifier => 2;
    public override int DamageModifier => 1;
}

public sealed class CurseEffect(int turns = 3) : StatStatusEffect("curse", "Lanet", turns)
{
    public override int AttackModifier => -2;
    public override int DamageModifier => -1;
}

public sealed class StatusEffectCollection
{
    private readonly List<StatusEffect> _effects = [];
    public IReadOnlyList<StatusEffect> Active => _effects;
    public int AttackModifier => _effects.Sum(x => x.AttackModifier);
    public int ArmorModifier => _effects.Sum(x => x.ArmorModifier);
    public int DamageModifier => _effects.Sum(x => x.DamageModifier);

    public void Add(StatusEffect effect)
    {
        StatusEffect? current = _effects.FirstOrDefault(x => x.Id == effect.Id);
        if (current is null) _effects.Add(effect);
        else current.Refresh(effect.RemainingTurns);
    }

    public bool Has<T>() where T : StatusEffect => _effects.OfType<T>().Any();
    public bool Remove(string id)
    {
        StatusEffect? effect = _effects.FirstOrDefault(x => x.Id == id);
        return effect is not null && _effects.Remove(effect);
    }

    public IReadOnlyList<StatusTurnResult> AdvanceTurn(ICombatant owner)
    {
        var results = new List<StatusTurnResult>();
        foreach (StatusEffect effect in _effects.ToArray())
        {
            results.Add(effect.Advance(owner));
            if (effect.RemainingTurns <= 0) _effects.Remove(effect);
        }
        return results;
    }

    public void Clear() => _effects.Clear();
}
