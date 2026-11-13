using RetroRpg.Core.Characters;
using RetroRpg.Core.Common;
using RetroRpg.Core.Entities;

namespace RetroRpg.Core.Items;

public abstract class Item : GameEntity
{
    protected Item(string name, int value, double weight) : base(name)
    {
        Value = Math.Max(0, value);
        Weight = Math.Max(0, weight);
    }

    public int Value { get; }
    public double Weight { get; }
    public virtual bool CanUse(PlayerCharacter character) => character.IsAlive;

    // Görev, kimlik ve hikâye eşyaları mağazaya satılamaz.
    public virtual bool CanBeSold => true;
}

public sealed class Weapon : Item
{
    public Weapon(string id, string name, int value, double weight, int damageDie, EquipmentSlot slot = EquipmentSlot.MainHand)
        : base(name, value, weight)
    {
        AssetId = id;
        DamageDie = damageDie;
        Slot = slot;
    }

    public string AssetId { get; }
    public int DamageDie { get; }
    public EquipmentSlot Slot { get; }
}

// Kaderbozan Zırhı bu sınıftan türediği için Armor mühürlü değildir.
public class Armor : Item
{
    public Armor(string name, int value, double weight, int armorBonus, EquipmentSlot slot)
        : base(name, value, weight) { ArmorBonus = armorBonus; Slot = slot; }

    public int ArmorBonus { get; }
    public EquipmentSlot Slot { get; }
}

public sealed class Consumable : Item
{
    public Consumable(string name, int value, double weight, int healing) : base(name, value, weight) => Healing = healing;
    public int Healing { get; }

    public void Use(PlayerCharacter character)
    {
        if (!CanUse(character)) throw new InvalidOperationException("Bu eşya şu anda kullanılamaz.");
        character.Heal(Healing);
    }
}

public sealed class QuestItem(string name, string questKey) : Item(name, 0, 0)
{
    public string QuestKey { get; } = questKey;
    public override bool CanBeSold => false;
}
