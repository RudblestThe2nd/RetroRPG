using RetroRpg.Core.Characters;
using RetroRpg.Core.Common;

namespace RetroRpg.Core.Items;

public static class ItemCatalog
{
    public static IReadOnlyList<Weapon> StartingWeapons { get; } =
    [
        new Weapon("dagger", "Hançer", 8, 1, 4),
        new Weapon("rapier", "İnce Kılıç", 20, 2, 8),
        new Weapon("mace", "Topuz", 12, 3, 6),
        new Weapon("warhammer", "Savaş Çekici", 18, 5, 8),
        new Weapon("greatsword", "Büyük Kılıç", 30, 7, 12),
        new Weapon("light-crossbow", "Hafif Arbalet", 25, 5, 8)
    ];

    public static Weapon StartingWeaponFor(string classId) => classId switch
    {
        "rogue" => StartingWeapons.First(x => x.AssetId == "dagger"),
        "ranger" => StartingWeapons.First(x => x.AssetId == "light-crossbow"),
        "fighter" or "paladin" or "barbarian" => StartingWeapons.First(x => x.AssetId == "greatsword"),
        _ => StartingWeapons.First(x => x.AssetId == "mace")
    };

    // Yeni karakterin ve eşya bilgisi olmayan eski kayıtların aldığı başlangıç eşyaları.
    // Sınıf ana paketi belirler; geçmiş ve seçilen hikâye küçük eklemeler yapar.
    public static void GiveStartingKit(PlayerCharacter player, string storySlotId = "story-slot-1")
    {
        StartingKitCatalog.For(player.Class.Id).GiveTo(player);
        StartingKitCatalog.AddBackgroundItem(player);
        StartingKitCatalog.AddStoryItem(player, storySlotId);
    }
}

// Ödevin kalıtım hedefini sade tutan başlangıç ekipmanı ailesi.
// Her alt paket yalnız değişen eşyaları seçer; verme davranışı taban sınıftadır.
public abstract class StartingKitPackage
{
    protected abstract Weapon Weapon { get; }
    protected virtual IEnumerable<Item> ExtraItems => [];

    public void GiveTo(PlayerCharacter player)
    {
        player.Inventory.Add(Weapon);
        player.Equipment.Equip(Weapon, Weapon.Slot);
        foreach (Item item in ExtraItems)
        {
            player.Inventory.Add(item);
            if (item is Armor armor && player.Equipment.Get(armor.Slot) is null)
                player.Equipment.Equip(armor, armor.Slot);
        }
    }
}

public sealed class WarriorStartingKit(string classId) : StartingKitPackage
{
    protected override Weapon Weapon => classId == "paladin"
        ? ItemCatalog.StartingWeapons.First(x => x.AssetId == "warhammer")
        : ItemCatalog.StartingWeapons.First(x => x.AssetId == "greatsword");

    protected override IEnumerable<Item> ExtraItems => classId switch
    {
        "paladin" => [new Armor("Zincir Gömlek", 30, 10, 2, EquipmentSlot.Torso), new Consumable("İyileştirme İksiri", 10, .5, 6)],
        "barbarian" => [new Consumable("Yol Azığı", 4, .4, 3)],
        _ => [new Armor("Dolgulu Yelek", 15, 4, 1, EquipmentSlot.Torso)]
    };
}

public sealed class ExpertStartingKit(string classId) : StartingKitPackage
{
    protected override Weapon Weapon => ItemCatalog.StartingWeaponFor(classId);
    protected override IEnumerable<Item> ExtraItems => classId switch
    {
        "rogue" => [new QuestItem("Maymuncuk Takımı", "tool:lockpicks")],
        "ranger" => [new Consumable("Yol Azığı", 4, .4, 3)],
        "bard" => [new Consumable("İyileştirme İksiri", 10, .5, 6)],
        _ => [new Consumable("Yol Azığı", 4, .4, 3)]
    };
}

public sealed class CasterStartingKit(string classId) : StartingKitPackage
{
    protected override Weapon Weapon => classId is "wizard" or "sorcerer" or "warlock"
        ? ItemCatalog.StartingWeapons.First(x => x.AssetId == "dagger")
        : ItemCatalog.StartingWeapons.First(x => x.AssetId == "mace");

    protected override IEnumerable<Item> ExtraItems => classId switch
    {
        "wizard" => [new QuestItem("Büyü Notları", "focus:spellbook")],
        "warlock" => [new QuestItem("Maceracı Meşalesi", "tool:torch")],
        "druid" => [new Consumable("Yol Azığı", 4, .4, 3)],
        _ => [new Consumable("İyileştirme İksiri", 10, .5, 6)]
    };
}

public static class StartingKitCatalog
{
    public static StartingKitPackage For(string classId) => classId switch
    {
        "fighter" or "paladin" or "barbarian" => new WarriorStartingKit(classId),
        "rogue" or "ranger" or "bard" or "monk" => new ExpertStartingKit(classId),
        _ => new CasterStartingKit(classId)
    };

    public static void AddBackgroundItem(PlayerCharacter player)
    {
        string[] ids = player.Background.Choices.Select(x => x.Id).ToArray();
        Item? item = ids.Contains("family-nobles") || player.Background.Id == "noble-family" ? new QuestItem("Aile Mührü", "credential:noble")
            : ids.Contains("profession-healer") ? new Consumable("Şifacı Merhemi", 6, .3, 4)
            : ids.Contains("profession-hunter") ? new Consumable("Yol Azığı", 4, .4, 3)
            : ids.Contains("profession-guard") || player.Background.Id == "soldier-family" ? new QuestItem("Eski Nöbetçi Rozeti", "credential:guard")
            : ids.Contains("profession-trader") ? new QuestItem("Tüccar Defteri", "tool:ledger")
            : null;
        if (item is not null) player.Inventory.Add(item);
    }

    public static void AddStoryItem(PlayerCharacter player, string storySlotId)
    {
        Item item = storySlotId switch
        {
            "story-slot-2" => new QuestItem("Mühürlü Mektup", "story:sealed-letter"),
            "story-slot-3" => new QuestItem("Kırık Kalıntı", "story:broken-relic"),
            _ => new QuestItem("Sınır Yolu Haritası", "story:frontier-map")
        };
        player.Inventory.Add(item);
        player.Inventory.Add(new Consumable("Yol Azığı", 4, .4, 3));
    }

    public static string SummaryFor(PlayerCharacter player, string storySlotId = "story-slot-1")
    {
        var preview = new PlayerCharacter(player.Name, player.Gender, player.Race, player.Class, player.Background,
            new CharacterStats
            {
                Strength = player.Stats.Strength,
                Dexterity = player.Stats.Dexterity,
                Constitution = player.Stats.Constitution,
                Intelligence = player.Stats.Intelligence,
                Wisdom = player.Stats.Wisdom,
                Charisma = player.Stats.Charisma
            }, applyRaceBonuses: false);
        GivePreview(preview, storySlotId);
        return string.Join(" · ", preview.Inventory.Items.Select(x => x.Name));
    }

    private static void GivePreview(PlayerCharacter player, string storySlotId) => ItemCatalog.GiveStartingKit(player, storySlotId);
}
