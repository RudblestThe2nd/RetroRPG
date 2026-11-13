using RetroRpg.Core.Common;
using RetroRpg.Core.Items;

namespace RetroRpg.Core.Persistence;

// Envanterdeki eşyaları kayda yazar ve geri kurar. Takılı eşyalar Equipped ile işaretlenir.
public static class ItemSaveMapper
{
    public static ItemSaveData[] ToSaveData(Inventory inventory, EquipmentSet equipment)
    {
        HashSet<Item> equipped = equipment.Equipped.Values.ToHashSet();
        IEnumerable<Item> items = inventory.Items.Concat(equipped.Where(x => !inventory.Items.Contains(x)));
        return items.Select(x => ToSaveData(x, equipped.Contains(x))).ToArray();
    }

    public static void Restore(IEnumerable<ItemSaveData> items, Inventory inventory, EquipmentSet equipment)
    {
        foreach (ItemSaveData data in items)
        {
            Item item = FromSaveData(data);
            inventory.Add(item);
            if (data.Equipped && data.Slot is EquipmentSlot slot) equipment.Equip(item, slot);
        }
    }

    private static ItemSaveData ToSaveData(Item item, bool equipped) => item switch
    {
        // Kaderbozan genel Armor kaydından önce gelmeli; Amount kalan son direniş hakkıdır.
        FatebreakerArmor f => new("fatebreaker", f.Name, f.Value, f.Weight, Amount: f.LastStandCharges, Slot: f.Slot, Equipped: equipped),
        Weapon w => new("weapon", w.Name, w.Value, w.Weight, w.AssetId, w.DamageDie, w.Slot, Equipped: equipped),
        Armor a => new("armor", a.Name, a.Value, a.Weight, Amount: a.ArmorBonus, Slot: a.Slot, Equipped: equipped),
        Consumable c => new("consumable", c.Name, c.Value, c.Weight, Amount: c.Healing),
        QuestItem q => new("quest", q.Name, q.Value, q.Weight, QuestKey: q.QuestKey),
        _ => throw new NotSupportedException($"{item.GetType().Name} kayda yazılamıyor.")
    };

    private static Item FromSaveData(ItemSaveData data) => data.Kind switch
    {
        "weapon" => new Weapon(data.AssetId ?? "", data.Name, data.Value, data.Weight, data.Amount, data.Slot ?? EquipmentSlot.MainHand),
        "armor" => new Armor(data.Name, data.Value, data.Weight, data.Amount, data.Slot ?? EquipmentSlot.Torso),
        "fatebreaker" => new FatebreakerArmor(data.Amount),
        "consumable" => new Consumable(data.Name, data.Value, data.Weight, data.Amount),
        "quest" => new QuestItem(data.Name, data.QuestKey ?? ""),
        _ => throw new InvalidDataException($"Kayıtta bilinmeyen eşya türü: {data.Kind}")
    };
}
