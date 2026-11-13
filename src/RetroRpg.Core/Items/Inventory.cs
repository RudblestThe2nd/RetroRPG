using RetroRpg.Core.Common;

namespace RetroRpg.Core.Items;

public sealed class Inventory
{
    private readonly List<Item> _items = [];
    public IReadOnlyList<Item> Items => _items;
    public double TotalWeight => _items.Sum(x => x.Weight);

    public void Add(Item item) => _items.Add(item ?? throw new ArgumentNullException(nameof(item)));
    public bool Remove(Item item) => _items.Remove(item);
    public T? Find<T>() where T : Item => _items.OfType<T>().FirstOrDefault();
}

public sealed class EquipmentSet
{
    private readonly Dictionary<EquipmentSlot, Item> _equipped = [];
    public IReadOnlyDictionary<EquipmentSlot, Item> Equipped => _equipped;

    public bool CanEquip(Item item, EquipmentSlot slot) => item switch
    {
        Weapon weapon => weapon.Slot == slot,
        Armor armor => armor.Slot == slot,
        _ => false
    };

    public void Equip(Item item, EquipmentSlot slot)
    {
        if (!CanEquip(item, slot)) throw new InvalidOperationException($"{item.Name}, {slot} yuvasına takılamaz.");
        _equipped[slot] = item;
    }

    public Item? Get(EquipmentSlot slot) => _equipped.GetValueOrDefault(slot);

    public bool IsEquipped(Item item) => _equipped.Values.Contains(item);

    public Item? Unequip(EquipmentSlot slot)
    {
        if (!_equipped.Remove(slot, out Item? item)) return null;
        return item;
    }

    public bool Unequip(Item item)
    {
        foreach (KeyValuePair<EquipmentSlot, Item> entry in _equipped)
            if (ReferenceEquals(entry.Value, item)) return _equipped.Remove(entry.Key);
        return false;
    }
}
