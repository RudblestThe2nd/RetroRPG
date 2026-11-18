using RetroRpg.Core.Combat;
using RetroRpg.Core.Common;
using RetroRpg.Core.Entities;
using RetroRpg.Core.Items;

namespace RetroRpg.Core.Characters;

// Oyuncunun yönettiği karakter. Class, seçilen alt sınıfın nesnesidir (örneğin ChampionClass).
public sealed class PlayerCharacter : GameEntity, ICombatant
{
    public const int MaximumLevel = 10;

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
    public int Level { get; private set; } = 1;
    public int Experience { get; private set; }
    public int ExperienceToNextLevel => Level >= MaximumLevel ? 0 : Level * 100;
    public int MaxPowerPoints => 2 + (Level - 1) / 2;
    public int PowerPoints { get; private set; } = 2;
    public int MaxHealth { get; private set; }
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

    // XP mevcut seviyenin içindeki ilerlemeyi taşır. Seviye atlamak canı artırır ve gücü yeniler.
    public int AddExperience(int amount)
    {
        Experience += Math.Max(0, amount);
        int gained = 0;
        while (Level < MaximumLevel && Experience >= ExperienceToNextLevel)
        {
            Experience -= ExperienceToNextLevel;
            Level++;
            int healthGain = Math.Max(2, Class.BaseHitPoints / 4 + Math.Max(0, CharacterStats.Modifier(Stats.Constitution)));
            MaxHealth += healthGain;
            Health += healthGain;
            gained++;
        }
        if (Level >= MaximumLevel) Experience = 0;
        PowerPoints = MaxPowerPoints;
        return gained;
    }

    public bool TrySpendPowerPoint(int cost = 1)
    {
        cost = Math.Max(1, cost);
        if (PowerPoints < cost) return false;
        PowerPoints -= cost;
        return true;
    }

    public void RestorePowerPoints() => PowerPoints = MaxPowerPoints;

    public void RestoreProgression(int level, int experience, int powerPoints)
    {
        Level = Math.Clamp(level, 1, MaximumLevel);
        Experience = Level >= MaximumLevel ? 0 : Math.Clamp(experience, 0, Math.Max(0, ExperienceToNextLevel - 1));
        int healthGain = Math.Max(2, Class.BaseHitPoints / 4 + Math.Max(0, CharacterStats.Modifier(Stats.Constitution)));
        MaxHealth = Math.Max(1, Class.BaseHitPoints + CharacterStats.Modifier(Stats.Constitution) + (Level - 1) * healthGain);
        PowerPoints = powerPoints < 0 ? MaxPowerPoints : Math.Clamp(powerPoints, 0, MaxPowerPoints);
    }

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
