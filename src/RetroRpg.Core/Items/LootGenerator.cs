using RetroRpg.Core.Characters;
using RetroRpg.Core.Common;

namespace RetroRpg.Core.Items;

public static class LootGenerator
{
    private static readonly (string Id, string Name, int Value, double Weight, int Die)[] Weapons =
    [
        ("dagger", "Hançer", 8, 1, 4),
        ("mace", "Topuz", 12, 3, 6),
        ("rapier", "İnce Kılıç", 20, 2, 8),
        ("warhammer", "Savaş Çekici", 18, 5, 8),
        ("greatsword", "Büyük Kılıç", 30, 7, 12),
        ("light-crossbow", "Hafif Arbalet", 25, 5, 8)
    ];

    public static Item? Roll(EnemyNpc enemy, int playerLevel, int seed)
    {
        var random = new Random(seed);
        int dropChance = enemy is MainBossNpc ? 100 : enemy is MiniBossNpc ? 90 : enemy.MaxHealth >= 20 ? 75 : 55;
        if (random.Next(100) >= dropChance) return null;
        return Create(RollRarity(enemy, playerLevel, random), random.Next());
    }

    public static Item Create(ItemRarity rarity, int seed)
    {
        var random = new Random(seed);
        int bonus = (int)rarity;
        int kind = random.Next(100);
        if (kind < 62)
        {
            var data = Weapons[random.Next(Weapons.Length)];
            return new Weapon(data.Id, RarityName(data.Name, rarity), ScaleValue(data.Value, rarity), data.Weight,
                data.Die, rarity: rarity, enhancementBonus: bonus);
        }
        if (kind < 86)
        {
            bool helmet = random.Next(2) == 0;
            string name = helmet ? "Demir Miğfer" : "Zincir Gömlek";
            return new Armor(RarityName(name, rarity), ScaleValue(helmet ? 18 : 30, rarity), helmet ? 3 : 10,
                (helmet ? 1 : 2) + bonus, helmet ? EquipmentSlot.Head : EquipmentSlot.Torso, rarity);
        }
        return new Consumable(RarityName("İyileştirme İksiri", rarity), ScaleValue(10, rarity), .5,
            6 + bonus * 3, rarity);
    }

    public static string RarityName(string name, ItemRarity rarity) => rarity switch
    {
        ItemRarity.Uncommon => $"İşlenmiş {name}",
        ItemRarity.Rare => $"Usta İşi {name}",
        ItemRarity.Epic => $"Efsunlu {name}",
        ItemRarity.Legendary => $"Kadim {name}",
        _ => name
    };

    public static string RarityLabel(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Uncommon => "İYİ",
        ItemRarity.Rare => "NADİR",
        ItemRarity.Epic => "DESTANSI",
        ItemRarity.Legendary => "EFSANEVİ",
        _ => "SIRADAN"
    };

    private static ItemRarity RollRarity(EnemyNpc enemy, int level, Random random)
    {
        if (enemy is MainBossNpc) return ItemRarity.Legendary;
        int roll = random.Next(100) + Math.Min(15, level * 2) + (enemy is MiniBossNpc ? 18 : enemy.MaxHealth >= 20 ? 10 : 0);
        return roll switch
        {
            >= 112 => ItemRarity.Epic,
            >= 92 => ItemRarity.Rare,
            >= 62 => ItemRarity.Uncommon,
            _ => ItemRarity.Common
        };
    }

    private static int ScaleValue(int value, ItemRarity rarity) => value * (2 + (int)rarity) / 2;
}
