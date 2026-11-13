using RetroRpg.Core.Combat;
using RetroRpg.Core.Common;
using RetroRpg.Core.Entities;
using RetroRpg.Core.Items;

namespace RetroRpg.Core.Characters;

// Oyuncunun yönettiği karakter. Class, seçilen alt sınıfın nesnesidir (örneğin ChampionClass).
public sealed class PlayerCharacter : GameEntity, ICombatant
{
    public PlayerCharacter(
        string name,
        Gender gender,
        CharacterRace race,
        CharacterClass characterClass,
        CharacterBackground background,
        CharacterStats stats,
        bool applyRaceBonuses = true) : base(name)
    {
        Gender = gender;
        Race = race;
        Class = characterClass;
        Background = background;
        Stats = stats;
        if (applyRaceBonuses)
        {
            Race.ApplyBonuses(Stats);
            Background.ApplyBonuses(Stats);
        }
        MaxHealth = Math.Max(1, Class.BaseHitPoints + CharacterStats.Modifier(Stats.Constitution));
        Health = MaxHealth;
        Gold = background.StartingGold;
    }

    public Gender Gender { get; }
    public CharacterRace Race { get; }
    public CharacterClass Class { get; }
    public CharacterBackground Background { get; }
    public CharacterStats Stats { get; }
    public int MaxHealth { get; }
    public int Health { get; private set; }
    public int ArmorClass => 10 + CharacterStats.Modifier(Stats.Dexterity) + DefenseBonus +
        Equipment.Equipped.Values.OfType<Armor>().Sum(x => x.ArmorBonus);

    // Savaşta "Savun" seçilince bir düşman saldırısı boyunca eklenen AC.
    public int DefenseBonus { get; set; }
    public int Gold { get; set; }
    public Inventory Inventory { get; } = new();
    public EquipmentSet Equipment { get; } = new();
    public bool IsAlive => Health > 0;

    public int AttackBonus => CharacterStats.Modifier(Class.UsesDexterity ? Stats.Dexterity : Stats.Strength) + Class.AttackBonus();
    public int InitiativeBonus => Stats.InitiativeBonus;
    public int DamageBonus => Math.Max(0, CharacterStats.Modifier(Stats.Strength));
    public int DamageDie => Equipment.Get(EquipmentSlot.MainHand) is Weapon weapon ? weapon.DamageDie : 4;

    public void ReceiveDamage(int amount)
    {
        int remaining = Math.Max(0, Health - Math.Max(0, amount));
        // Kaderbozan Zırhı kuşanılmışsa ölümcül darbeyi bir kez 1 CAN'da durdurur.
        if (remaining == 0 && Health > 0 &&
            Equipment.Get(EquipmentSlot.StoryLayer) is FatebreakerArmor armor && armor.TryUseLastStand())
            remaining = 1;
        Health = remaining;
    }

    // Kaderin Hükmü: AC'yi, zırhı ve son direnişi yok sayar. Yalnız hikâye olayı çağırır.
    public void FallToFate() => Health = 0;

    public void Heal(int amount) => Health = Math.Min(MaxHealth, Health + Math.Max(0, amount));
    public void RestoreState(int health, int gold)
    {
        Health = Math.Clamp(health, 0, MaxHealth);
        Gold = Math.Max(0, gold);
    }
}
