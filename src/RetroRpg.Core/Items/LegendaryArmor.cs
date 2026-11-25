using RetroRpg.Core.Common;

namespace RetroRpg.Core.Items;

// Kaderbozan Zırhı: Kalpaklı Süvari'nin verdiği efsanevi hikâye zırhı.
// Göğüs yuvasını değil StoryLayer yuvasını kullanır; oyuncunun önceki göğüs zırhı yerinde kalır.
// Ham AC'si yalnız +1'dir. Asıl gücü Vaelor'un Kaderin Hükmü saldırısını kırmasıdır.
// Bütün ırk ve beden tipleri aynı katmanı kullanır; ırka göre kısıtlama yoktur.
public sealed class FatebreakerArmor : Armor
{
    public const string ItemName = "Kaderbozan Zırhı";

    public FatebreakerArmor(int lastStandCharges = 1)
        : base(ItemName, 0, 6, 1, EquipmentSlot.StoryLayer, ItemRarity.Legendary)
    {
        LastStandCharges = Math.Clamp(lastStandCharges, 0, 1);
    }

    // Bir kez ölümcül darbeyi 1 CAN'da durdurur. Hak kayda yazılır.
    public int LastStandCharges { get; private set; }

    public override bool CanBeSold => false;

    public bool TryUseLastStand()
    {
        if (LastStandCharges <= 0) return false;
        LastStandCharges--;
        return true;
    }
}
