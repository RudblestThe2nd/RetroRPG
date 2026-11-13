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
        ArmorClass = armorClass;
        Class = characterClass;
        RewardGold = Math.Max(0, rewardGold);
    }

    public int MaxHealth { get; }
    public int Health { get; private set; }
    public int ArmorClass { get; }
    public CharacterClass Class { get; }
    public int RewardGold { get; }
    public Weapon? Weapon { get; set; }
    public bool IsAlive => Health > 0;

    public virtual int AttackBonus => Class.AttackBonus();
    public int InitiativeBonus => 0;
    public int DamageBonus => 0;
    public int DamageDie => Weapon?.DamageDie ?? 4;

    public void ReceiveDamage(int amount) => Health = Math.Max(0, Health - Math.Max(0, amount));

    public override string Interact() => $"{Name} saldırmaya hazırlanıyor.";
}
