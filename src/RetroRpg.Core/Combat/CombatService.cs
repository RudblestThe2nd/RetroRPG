namespace RetroRpg.Core.Combat;

public sealed record CombatLogEntry(string Text, int Roll, int Bonus, int Target, bool Success);

public sealed class CombatService(IDiceRoller dice)
{
    private readonly IDiceRoller _dice = dice;
    private readonly List<CombatLogEntry> _log = [];
    public IReadOnlyList<CombatLogEntry> Log => _log;

    public InitiativeRoll RollInitiative(ICombatant combatant)
    {
        var result = _dice.RollD20(combatant.InitiativeBonus);
        return new InitiativeRoll(result.RawValue, result.Bonus);
    }

    public AttackRoll Attack(ICombatant attacker, ICombatant defender)
        => Attack(attacker, defender, 0, attacker.DamageDie, 0);

    public AttackRoll Attack(ICombatant attacker, ICombatant defender, int attackAdjustment,
        int damageDie, int damageBonusAdjustment = 0)
    {
        var raw = _dice.RollD20(attacker.AttackBonus + attackAdjustment);
        var attack = new AttackRoll(raw.RawValue, raw.Bonus, defender.ArmorClass);
        _log.Add(new CombatLogEntry(
            $"{attacker.Name}: d20 {attack.RawValue} + {attack.Bonus} = {attack.Total} / AC {attack.TargetArmorClass}",
            attack.RawValue, attack.Bonus, attack.TargetArmorClass, attack.IsHit));

        if (attack.IsHit)
        {
            DamageResult damage = new(_dice.Roll(Math.Max(2, damageDie)),
                Math.Max(0, attacker.DamageBonus + damageBonusAdjustment), attack.IsCritical);
            defender.ReceiveDamage(damage.Total);
            _log.Add(new CombatLogEntry($"Hasar: {damage.Total}", damage.DieValue, damage.Bonus, 0, true));
        }

        return attack;
    }
}
