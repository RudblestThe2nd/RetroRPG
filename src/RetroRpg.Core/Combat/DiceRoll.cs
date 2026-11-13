namespace RetroRpg.Core.Combat;

public abstract class DiceRoll
{
    protected DiceRoll(int sides, int rawValue, int bonus)
    {
        Sides = sides;
        RawValue = rawValue;
        Bonus = bonus;
    }

    public int Sides { get; }
    public int RawValue { get; }
    public int Bonus { get; }
    public int Total => RawValue + Bonus;
}

public class D20Roll(int rawValue, int bonus) : DiceRoll(20, rawValue, bonus)
{
    public bool IsNaturalTwenty => RawValue == 20;
    public bool IsNaturalOne => RawValue == 1;
}

public sealed class AttackRoll(int rawValue, int bonus, int targetArmorClass) : D20Roll(rawValue, bonus)
{
    public int TargetArmorClass { get; } = targetArmorClass;
    public bool IsCritical => IsNaturalTwenty;
    public bool IsHit => !IsNaturalOne && (IsNaturalTwenty || Total >= TargetArmorClass);
}

public sealed class InitiativeRoll(int rawValue, int bonus) : D20Roll(rawValue, bonus);

public sealed record DamageResult(int DieValue, int Bonus, bool IsCritical)
{
    public int Total => Math.Max(1, (IsCritical ? DieValue * 2 : DieValue) + Bonus);
}
