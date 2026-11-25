namespace RetroRpg.Core.Combat;

// Savaşa girebilen her şey. Oyuncu ve düşman NPC iki ayrı gerçek uygulamadır.
public interface ICombatant
{
    string Name { get; }
    int Health { get; }
    int MaxHealth { get; }
    int ArmorClass { get; }
    int AttackBonus { get; }
    int InitiativeBonus { get; }
    int DamageBonus { get; }
    int DamageDie { get; }
    bool IsAlive { get; }
    StatusEffectCollection StatusEffects { get; }
    void ReceiveDamage(int amount);
}
