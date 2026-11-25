using RetroRpg.Core.Combat;

namespace RetroRpg.Core.Items;

// Silah türleri aynı Weapon sınıfında kalır; değişen savaş davranışı bu küçük kalıtım ailesindedir.
public abstract class WeaponTechnique(string name, string description)
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public virtual int AttackBonus => 0;
    public virtual int DamageBonus => 0;
    public virtual int CriticalThreshold => 20;
    public virtual StatusEffect? EffectOnHit(AttackRoll roll) => null;
}

public sealed class DaggerTechnique() : WeaponTechnique("Keskin Uç", "19-20 zarında kritik vurur.")
{
    public override int CriticalThreshold => 19;
}

public sealed class SwordTechnique() : WeaponTechnique("Kanatan Kesik", "Yüksek saldırı zarında kanama uygular.")
{
    public override StatusEffect? EffectOnHit(AttackRoll roll) => roll.RawValue >= 17 ? new BleedingEffect() : null;
}

public sealed class CrushingTechnique() : WeaponTechnique("Zırh Ezme", "Yüksek saldırı zarında hedefin AC değerini düşürür.")
{
    public override StatusEffect? EffectOnHit(AttackRoll roll) => roll.RawValue >= 15 ? new ArmorBreakEffect() : null;
}

public sealed class GreatWeaponTechnique() : WeaponTechnique("Ağır Darbe", "+2 hasar, -1 isabet.")
{
    public override int AttackBonus => -1;
    public override int DamageBonus => 2;
}

public sealed class RangedTechnique() : WeaponTechnique("Nişanlı Atış", "+2 isabet, -1 hasar.")
{
    public override int AttackBonus => 2;
    public override int DamageBonus => -1;
}

public sealed class NaturalTechnique() : WeaponTechnique("Doğal Saldırı", "Ek silah özelliği yoktur.");

public static class WeaponTechniqueCatalog
{
    public static WeaponTechnique For(string assetId) => assetId switch
    {
        "dagger" => new DaggerTechnique(),
        "rapier" => new SwordTechnique(),
        "mace" or "warhammer" => new CrushingTechnique(),
        "greatsword" => new GreatWeaponTechnique(),
        "light-crossbow" => new RangedTechnique(),
        _ => new NaturalTechnique()
    };
}
