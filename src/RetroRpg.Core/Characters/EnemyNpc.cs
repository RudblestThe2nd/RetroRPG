using RetroRpg.Core.Combat;
using RetroRpg.Core.Items;

namespace RetroRpg.Core.Characters;

// Savaşılabilen NPC. Bir Npc türüdür; CharacterClass türü değildir, yalnız Class özelliğine sahiptir.
public class EnemyNpc : Npc, ICombatant
{
    public EnemyNpc(string name, string visualAssetId, int maxHealth, int armorClass, CharacterClass characterClass, int rewardGold)
        : base(name, visualAssetId)
    {
        MaxHealth = Math.Max(1, maxHealth);
        Health = MaxHealth;
        BaseArmorClass = armorClass;
        Class = characterClass;
        RewardGold = Math.Max(0, rewardGold);
    }

    public int MaxHealth { get; }
    public int Health { get; private set; }
    public int BaseArmorClass { get; }
    public int DefenseBonus { get; set; }
    public int ArmorClass => BaseArmorClass + DefenseBonus;
    public CharacterClass Class { get; }
    public int RewardGold { get; }
    public int RewardExperience => Math.Max(10, MaxHealth * 2 + RewardGold);
    public Weapon? Weapon { get; set; }
    public EnemyBehavior Behavior { get; set; } = new AggressiveBehavior();
    public StatusEffectCollection StatusEffects { get; } = new();
    public bool IsAlive => Health > 0;

    public virtual int AttackBonus => Class.AttackBonus() + (Weapon?.AttackBonus ?? 0);
    public int InitiativeBonus => 0;
    public int DamageBonus => Weapon?.DamageBonus ?? 0;
    public int DamageDie => Weapon?.DamageDie ?? 4;

    public void ReceiveDamage(int amount) => Health = Math.Max(0, Health - Math.Max(0, amount));

    public override string Interact() => $"{Name} saldırmaya hazırlanıyor.";
}
