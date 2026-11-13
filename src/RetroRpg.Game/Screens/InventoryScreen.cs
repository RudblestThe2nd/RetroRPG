using RetroRpg.Core.Common;
using RetroRpg.Core.Game;
using RetroRpg.Core.Items;

namespace RetroRpg.Game.Screens;

internal sealed class InventoryScreen : GameScreen
{
    private readonly GameSession _session;
    private readonly List<PixelButton> _slots = [];
    private readonly PixelButton _useButton;
    private readonly PixelButton _equipButton;
    private int _selected = -1;
    private string _message = "Bir eşya seç.";

    public InventoryScreen(MainGameForm host, GameSession session) : base(host)
    {
        _session = session;
        for (int i = 0; i < 25; i++)
        {
            int index = i;
            var button = new PixelButton(new Rectangle(389 + i % 5 * 63, 157 + i / 5 * 62, 54, 54), "", () => Select(index));
            _slots.Add(AddButton(button));
        }
        AddButton(new PixelButton(new Rectangle(69, 667, 140, 54), "HARİTAYA DÖN", () => Host.ShowMap()) { Skin = PixelButton.Medium, Font = UiTheme.Small });
        _useButton = AddButton(new PixelButton(new Rectangle(243, 667, 140, 54), "KULLAN", UseSelected) { Skin = PixelButton.Medium });
        _equipButton = AddButton(new PixelButton(new Rectangle(417, 667, 146, 54), "KUŞAN", EquipSelected) { Skin = PixelButton.Medium });
        RefreshButtons();
    }

    private IReadOnlyList<Item> Items => _session.Player.Inventory.Items;

    private void Select(int index)
    {
        _selected = index < Items.Count ? index : -1;
        RefreshButtons();
        Invalidate();
    }

    internal void SelectFirstForQa() => Select(0);
    internal void SelectItemForQa(Item item) => Select(Items.ToList().IndexOf(item));

    private void RefreshButtons()
    {
        for (int i = 0; i < _slots.Count; i++) _slots[i].Selected = i == _selected;
        Item? item = SelectedItem();
        _useButton.Enabled = item is Consumable;
        _equipButton.Enabled = item is Weapon or Armor;
        _equipButton.Text = item is not null && _session.Player.Equipment.IsEquipped(item) ? "ÇIKAR" : "KUŞAN";
    }

    private Item? SelectedItem() => _selected >= 0 && _selected < Items.Count ? Items[_selected] : null;

    private void UseSelected()
    {
        if (SelectedItem() is not Consumable consumable) return;
        int before = _session.Player.Health;
        consumable.Use(_session.Player);
        _session.Player.Inventory.Remove(consumable);
        _message = $"{consumable.Name}: +{_session.Player.Health - before} CAN";
        if (_selected >= Items.Count) _selected = Items.Count - 1;
        Host.Autosave();
        RefreshButtons();
        Invalidate();
    }

    private void EquipSelected()
    {
        if (SelectedItem() is not Item item) return;
        if (_session.Player.Equipment.IsEquipped(item))
        {
            _session.Player.Equipment.Unequip(item);
            _message = $"{item.Name} çıkarıldı.";
            Host.Autosave();
            RefreshButtons();
            Invalidate();
            return;
        }
        EquipmentSlot slot = item switch
        {
            Weapon weapon => weapon.Slot,
            Armor armor => armor.Slot,
            _ => EquipmentSlot.MainHand
        };
        if (!_session.Player.Equipment.CanEquip(item, slot)) return;
        _session.Player.Equipment.Equip(item, slot);
        _message = $"{item.Name} kuşanıldı.";
        Host.Autosave();
        RefreshButtons();
        Invalidate();
    }

    public override void HandleKey(Keys key) { if (key == Keys.Escape) Host.ShowMap(); }

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get("assets/final-production-v5/ui/screens/runtime-800/inventory-wood-red-v1.png"), CanvasRectangle);
        UiTheme.DrawTextCentered(g, "ENVANTER VE EKİPMAN", UiTheme.Title, UiTheme.Gold, new Rectangle(160, 30, 480, 48));
        var player = _session.Player;
        UiTheme.DrawImageFit(g, Images.Get(CharacterArt.PortraitPath(player.Race.Id, player.Gender, _session.PortraitIndex)),
            new Rectangle(132, 202, 86, 86));
        UiTheme.DrawTextCentered(g, player.Name, UiTheme.Small, UiTheme.Gold, new Rectangle(118, 292, 116, 28));

        EquipmentSlot[] equipmentSlots = [EquipmentSlot.Head, EquipmentSlot.Torso, EquipmentSlot.Feet, EquipmentSlot.Cloak,
            EquipmentSlot.Accessory, EquipmentSlot.MainHand, EquipmentSlot.OffHand, EquipmentSlot.Back];
        Rectangle[] equipmentAreas =
        [
            new(70, 120, 64, 64), new(145, 120, 64, 64), new(220, 120, 64, 64), new(70, 240, 64, 64),
            new(220, 240, 64, 64), new(70, 375, 64, 64), new(220, 375, 64, 64), new(145, 460, 64, 64)
        ];
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            Item? equipped = player.Equipment.Get(equipmentSlots[i]);
            if (equipped is not null) UiTheme.DrawImageFit(g, Images.Get(ItemIcon(equipped)), Rectangle.Inflate(equipmentAreas[i], -7, -7));
        }

        for (int i = 0; i < Items.Count && i < 25; i++)
        {
            Rectangle area = Rectangle.Inflate(_slots[i].Bounds, -6, -6);
            string icon = ItemIcon(Items[i]);
            UiTheme.DrawImageFit(g, Images.Get(icon), area);
        }

        Item? selected = SelectedItem();
        string description = selected switch
        {
            null => _message,
            FatebreakerArmor armor => $"{armor.Name} · AC +{armor.ArmorBonus}\nHikâye katmanı; göğüs zırhını çıkarmaz. Kaderin Hükmü'nü kırar. Son direniş: {armor.LastStandCharges}",
            _ => $"{selected.Name}\nDeğer: {selected.Value} altın · Ağırlık: {selected.Weight:0.0}"
        };
        UiTheme.DrawWrapped(g, description, UiTheme.Body, UiTheme.Text, new Rectangle(392, 503, 325, 115));
        UiTheme.DrawTextCentered(g, $"AĞIRLIK {player.Inventory.TotalWeight:0.0}", UiTheme.Small, UiTheme.Text, new Rectangle(392, 625, 185, 30));
        UiTheme.DrawTextCentered(g, $"ALTIN {player.Gold}", UiTheme.Body, UiTheme.Gold, new Rectangle(592, 662, 130, 55));
    }

    internal static string ItemIcon(Item item) => item switch
    {
        Weapon weapon => weapon.AssetId switch
        {
            "dagger" => "items/item-pack-01/icons/weapons/06-rogue-dagger.png",
            "rapier" => "items/item-pack-01/icons/weapons/05-rapier.png",
            "mace" => "items/item-pack-01/icons/weapons/10-iron-mace.png",
            "warhammer" => "items/item-pack-01/icons/weapons/11-warhammer.png",
            "light-crossbow" => "items/item-pack-01/icons/weapons/15-crossbow.png",
            _ => "items/item-pack-01/icons/weapons/03-greatsword.png"
        },
        FatebreakerArmor => "items/item-pack-01/icons/armor/25-fatebreaker-armor-v1.png",
        Armor armor when armor.Slot == EquipmentSlot.Head => "items/item-pack-01/icons/armor/02-iron-helmet.png",
        Armor => "items/item-pack-01/icons/armor/08-chainmail-shirt.png",
        Consumable consumable when consumable.Name.Contains("Azık", StringComparison.OrdinalIgnoreCase) => "items/item-pack-01/icons/consumables-loot/12-ration-bag.png",
        Consumable consumable when consumable.Name.Contains("Panzehir", StringComparison.OrdinalIgnoreCase) => "items/item-pack-01/icons/consumables-loot/03-antidote.png",
        Consumable => "items/item-pack-01/icons/consumables-loot/01-health-potion.png",
        QuestItem quest when quest.QuestKey.Contains("torch", StringComparison.OrdinalIgnoreCase) => "items/item-pack-01/icons/weapons/23-adventurer-torch.png",
        QuestItem quest when quest.QuestKey.Contains("lockpick", StringComparison.OrdinalIgnoreCase) => "items/item-pack-01/icons/consumables-loot/16-lockpick-tools.png",
        QuestItem { QuestKey: "story:black-map" or "story:frontier-map" } => "items/item-pack-02/icons/quest-relics/24-world-map-fragment.png",
        QuestItem { QuestKey: "story:waystone-root" } => "items/item-pack-02/icons/quest-relics/18-forest-seed.png",
        QuestItem { QuestKey: "story:waystone-sky" } => "items/item-pack-02/icons/quest-relics/13-storm-pearl.png",
        QuestItem { QuestKey: "story:waystone-sun" } => "items/item-pack-02/icons/quest-relics/09-sun-medallion.png",
        _ => "items/item-pack-02/icons/quest-relics/04-ancient-tablet.png"
    };
}
