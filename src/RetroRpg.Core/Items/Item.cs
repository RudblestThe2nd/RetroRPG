using RetroRpg.Core.Characters;
using RetroRpg.Core.Common;
using RetroRpg.Core.Entities;

namespace RetroRpg.Core.Items;

public abstract class Item : GameEntity
{
    protected Item(string name, int value, double weight, ItemRarity rarity = ItemRarity.Common) : base(name)
    {
        Value = Math.Max(0, value);
        Weight = Math.Max(0, weight);
        Rarity = rarity;
    }

    public int Value { get; }
    public double Weight { get; }
    public ItemRarity Rarity { get; }
    public virtual bool CanUse(PlayerCharacter character) => character.IsAlive;

    // Görev, kimlik ve hikâye eşyaları mağazaya satılamaz.
    public virtual bool CanBeSold => true;
}

public sealed class Weapon : Item
{
    public Weapon(string id, string name, int value, double weight, int damageDie, EquipmentSlot slot = EquipmentSlot.MainHand,
        ItemRarity rarity = ItemRarity.Common, int enhancementBonus = 0)
        : base(name, value, weight, rarity)
    {
        AssetId = id;
        DamageDie = damageDie;
        Slot = slot;
        EnhancementBonus = Math.Max(0, enhancementBonus);
    }

    public string AssetId { get; }
    public int DamageDie { get; }
    public EquipmentSlot Slot { get; }
    public int EnhancementBonus { get; }
    public WeaponTechnique Technique => WeaponTechniqueCatalog.For(AssetId);
    public int AttackBonus => Technique.AttackBonus + EnhancementBonus;
    public int DamageBonus => Technique.DamageBonus + EnhancementBonus;
    public int CriticalThreshold => Technique.CriticalThreshold;
}

// Kaderbozan Zırhı bu sınıftan türediği için Armor mühürlü değildir.
public class Armor : Item
{
    public Armor(string name, int value, double weight, int armorBonus, EquipmentSlot slot,
        ItemRarity rarity = ItemRarity.Common)
        : base(name, value, weight, rarity) { ArmorBonus = armorBonus; Slot = slot; }

    public int ArmorBonus { get; }
    public EquipmentSlot Slot { get; }
}

public sealed class Consumable : Item
{
    public Consumable(string name, int value, double weight, int healing, ItemRarity rarity = ItemRarity.Common,
        string? curesStatusId = null)
        : base(name, value, weight, rarity) { Healing = healing; CuresStatusId = curesStatusId; }
    public int Healing { get; }
    public string? CuresStatusId { get; }

    public void Use(PlayerCharacter character)
    {
        if (!CanUse(character)) throw new InvalidOperationException("Bu eşya şu anda kullanılamaz.");
        character.Heal(Healing);
        if (CuresStatusId is not null) character.StatusEffects.Remove(CuresStatusId);
    }
}

public sealed class QuestItem(string name, string questKey) : Item(name, 0, 0)
{
    public string QuestKey { get; } = questKey;
    public override bool CanBeSold => false;
}
