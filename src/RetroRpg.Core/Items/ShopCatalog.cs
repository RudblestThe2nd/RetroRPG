using RetroRpg.Core.Common;
using RetroRpg.Core.Characters;
using RetroRpg.Core.World;

namespace RetroRpg.Core.Items;

public sealed record ShopOffer(
    string Id,
    string Name,
    int Price,
    string Description,
    string IconPath,
    Func<Item> CreateItem);

public static class ShopCatalog
{
    public static IReadOnlyList<ShopOffer> For(LocationService service) => service switch
    {
        LocationService.Market => Market,
        LocationService.Blacksmith => Blacksmith,
        _ => []
    };

    public static IReadOnlyList<ShopOffer> Market { get; } =
    [
        new("healing-potion", "İyileştirme İksiri", 10, "+6 CAN yeniler.",
            "items/item-pack-01/icons/consumables-loot/01-health-potion.png",
            () => new Consumable("İyileştirme İksiri", 10, .5, 6)),
        new("greater-healing", "Güçlü İyileştirme", 22, "+12 CAN yeniler.",
            "items/item-pack-01/icons/consumables-loot/04-stamina-tonic.png",
            () => new Consumable("Güçlü İyileştirme İksiri", 22, .6, 12)),
        new("antidote", "Panzehir", 8, "Basit zehirlenmeler için; +2 CAN.",
            "items/item-pack-01/icons/consumables-loot/03-antidote.png",
            () => new Consumable("Panzehir", 8, .2, 2)),
        new("ration", "Yol Azığı", 4, "Kampta veya yolda +3 CAN.",
            "items/item-pack-01/icons/consumables-loot/12-ration-bag.png",
            () => new Consumable("Yol Azığı", 4, .4, 3)),
        new("lockpicks", "Maymuncuk Takımı", 12, "Kilitli sandık ve kapılar için keşif eşyası.",
            "items/item-pack-01/icons/consumables-loot/16-lockpick-tools.png",
            () => new QuestItem("Maymuncuk Takımı", "tool:lockpicks")),
        new("torch", "Maceracı Meşalesi", 5, "Karanlık mağaralarda kullanılacak keşif eşyası.",
            "items/item-pack-01/icons/weapons/23-adventurer-torch.png",
            () => new QuestItem("Maceracı Meşalesi", "tool:torch"))
    ];

    public static IReadOnlyList<ShopOffer> Blacksmith { get; } =
    [
        WeaponOffer("dagger", "Hançer", 8, 4, "items/item-pack-01/icons/weapons/06-rogue-dagger.png"),
        WeaponOffer("mace", "Topuz", 12, 6, "items/item-pack-01/icons/weapons/10-iron-mace.png"),
        WeaponOffer("rapier", "İnce Kılıç", 20, 8, "items/item-pack-01/icons/weapons/05-rapier.png"),
        WeaponOffer("warhammer", "Savaş Çekici", 18, 8, "items/item-pack-01/icons/weapons/11-warhammer.png"),
        WeaponOffer("greatsword", "Büyük Kılıç", 30, 12, "items/item-pack-01/icons/weapons/03-greatsword.png"),
        new("padded-vest", "Dolgulu Yelek", 15, "Gövde zırhı, AC +1.",
            "items/item-pack-01/icons/armor/07-padded-vest.png",
            () => new Armor("Dolgulu Yelek", 15, 4, 1, EquipmentSlot.Torso)),
        new("chain-shirt", "Zincir Gömlek", 30, "Gövde zırhı, AC +2.",
            "items/item-pack-01/icons/armor/08-chainmail-shirt.png",
            () => new Armor("Zincir Gömlek", 30, 10, 2, EquipmentSlot.Torso)),
        new("iron-helmet", "Demir Miğfer", 18, "Baş zırhı, AC +1.",
            "items/item-pack-01/icons/armor/02-iron-helmet.png",
            () => new Armor("Demir Miğfer", 18, 3, 1, EquipmentSlot.Head))
    ];

    private static ShopOffer WeaponOffer(string id, string name, int price, int damageDie, string icon) =>
        new(id, name, price, $"Ana el silahı, hasar d{damageDie}.", icon,
            () => new Weapon(id, name, price, Math.Max(1, damageDie / 2d), damageDie));
}

public sealed class ShopService
{
    public bool TryPurchase(PlayerCharacter player, ShopOffer offer, out string message)
        => TryPurchase(player, offer, offer.Price, out message);

    public bool TryPurchase(PlayerCharacter player, ShopOffer offer, int price, out string message)
    {
        price = Math.Max(1, price);
        if (player.Gold < price)
        {
            message = $"Yetersiz altın. {price} altın gerekiyor.";
            return false;
        }

        player.Gold -= price;
        player.Inventory.Add(offer.CreateItem());
        message = $"{offer.Name} envantere eklendi.";
        return true;
    }

    public int SellPrice(Item item) => Math.Max(1, item.Value / 2);

    public bool TrySell(PlayerCharacter player, Item item, out string message)
    {
        if (!player.Inventory.Items.Contains(item))
        {
            message = "Bu eşya envanterde değil.";
            return false;
        }
        if (!item.CanBeSold)
        {
            message = "Görev ve kimlik eşyaları satılamaz.";
            return false;
        }

        player.Equipment.Unequip(item);
        player.Inventory.Remove(item);
        int price = SellPrice(item);
        player.Gold += price;
        message = $"{item.Name} {price} altına satıldı.";
        return true;
    }
}
