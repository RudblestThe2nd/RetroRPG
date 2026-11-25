using RetroRpg.Core.Game;
using RetroRpg.Core.Common;
using RetroRpg.Core.Items;
using RetroRpg.Core.World;

namespace RetroRpg.Game.Screens;

internal enum ShopMode { Buy, Sell }

internal sealed class ShopScreen : GameScreen
{
    private readonly GameSession _session;
    private readonly WorldLocation _location;
    private readonly LocationService _service;
    private readonly IReadOnlyList<ShopOffer> _offers;
    private readonly List<PixelButton> _rows = [];
    private readonly PixelButton _actionButton;
    private readonly PixelButton _buyTab;
    private readonly PixelButton _sellTab;
    private readonly ShopService _shop = new();
    private ShopMode _mode;
    private int _selected;
    private string _message = "Bir ürün seç.";

    public ShopScreen(MainGameForm host, GameSession session, WorldLocation location, LocationService service) : base(host)
    {
        _session = session;
        _location = location;
        _service = service;
        _offers = ShopCatalog.For(service);

        _buyTab = AddButton(new PixelButton(new Rectangle(62, 128, 145, 42), "SATIN AL", () => SetMode(ShopMode.Buy))
        { Skin = PixelButton.Medium, Font = UiTheme.Small });
        _sellTab = AddButton(new PixelButton(new Rectangle(218, 128, 145, 42), "SAT", () => SetMode(ShopMode.Sell))
        { Skin = PixelButton.Medium, Font = UiTheme.Small });
        for (int i = 0; i < 8; i++)
        {
            int index = i;
            _rows.Add(AddButton(new PixelButton(new Rectangle(62, 178 + i * 52, 314, 44), "", () => Select(index))
            { Skin = PixelButton.Medium, Font = UiTheme.Small }));
        }
        _actionButton = AddButton(new PixelButton(new Rectangle(455, 563, 260, 60), "SATIN AL", Act)
        { Skin = PixelButton.MedievalLarge });
        AddButton(new PixelButton(new Rectangle(270, 710, 260, 58), "YERLEŞİME DÖN", () => Host.ShowLocation(_location))
        { Skin = PixelButton.MedievalLarge });
        SetMode(ShopMode.Buy);
    }

    private IReadOnlyList<Item> SellableItems => _session.Player.Inventory.Items
        .Where(x => x is not QuestItem && (_service == LocationService.Market || x is Weapon or Armor))
        .ToArray();

    private void SetMode(ShopMode mode)
    {
        _mode = mode;
        _selected = 0;
        _buyTab.Selected = mode == ShopMode.Buy;
        _sellTab.Selected = mode == ShopMode.Sell;
        _actionButton.Text = mode == ShopMode.Buy ? "SATIN AL" : "SAT";
        _message = mode == ShopMode.Buy ? "Satın almak için bir ürün seç." : "Satılacak eşyayı seç.";
        RefreshRows();
    }

    internal void ShowSellForQa() => SetMode(ShopMode.Sell);

    private void RefreshRows()
    {
        IReadOnlyList<Item> sellable = SellableItems;
        for (int i = 0; i < _rows.Count; i++)
        {
            PixelButton row = _rows[i];
            if (_mode == ShopMode.Buy && i < _offers.Count)
            {
                ShopOffer offer = _offers[i];
                row.Text = $"{offer.Name} · {PurchasePrice(offer)} ALTIN";
                row.Visible = row.Enabled = true;
            }
            else if (_mode == ShopMode.Sell && i < sellable.Count)
            {
                Item item = sellable[i];
                row.Text = $"{LootGenerator.RarityLabel(item.Rarity)} · {item.Name} · {_shop.SellPrice(item)} ALTIN";
                row.Visible = row.Enabled = true;
            }
            else row.Visible = row.Enabled = false;
            row.Selected = i == _selected && row.Visible;
        }

        if (_mode == ShopMode.Buy)
            _actionButton.Enabled = _offers.Count > 0 && _session.Player.Gold >= PurchasePrice(_offers[Math.Min(_selected, _offers.Count - 1)]);
        else _actionButton.Enabled = sellable.Count > 0;
        Invalidate();
    }

    private void Select(int index)
    {
        int count = _mode == ShopMode.Buy ? _offers.Count : SellableItems.Count;
        if (count == 0) return;
        _selected = Math.Clamp(index, 0, count - 1);
        _message = _mode == ShopMode.Buy ? "SATIN AL ile işlemi onayla." : "SAT ile eşyayı yarı değerine sat.";
        RefreshRows();
    }

    private void Act()
    {
        if (_mode == ShopMode.Buy)
        {
            ShopOffer offer = _offers[_selected];
            if (_shop.TryPurchase(_session.Player, offer, PurchasePrice(offer), out _message)) Host.Autosave();
        }
        else
        {
            IReadOnlyList<Item> items = SellableItems;
            if (items.Count == 0) return;
            if (_shop.TrySell(_session.Player, items[Math.Min(_selected, items.Count - 1)], out _message))
            {
                _selected = Math.Max(0, Math.Min(_selected, SellableItems.Count - 1));
                Host.Autosave();
            }
        }
        RefreshRows();
    }

    public override void HandleKey(Keys key) { if (key == Keys.Escape) Host.ShowLocation(_location); }

    protected override void Draw(Graphics g)
    {
        string scene = _service == LocationService.Blacksmith
            ? "assets/final-production-v5/scenes/settlements/06-blacksmith-armorer-interior-v1.png"
            : "assets/final-production-v5/scenes/settlements/05-market-merchant-v1.png";
        UiTheme.DrawImage(g, Images.Get(scene), CanvasRectangle);
        UiTheme.Shade(g, CanvasRectangle, 120);
        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, new Rectangle(24, 18, 752, 82));
        UiTheme.DrawText(g, _service == LocationService.Blacksmith ? "DEMİRCİ" : "PAZAR", UiTheme.Title, UiTheme.Gold, 50, 34);
        UiTheme.DrawTextCentered(g, $"ALTIN: {_session.Player.Gold}", UiTheme.Heading, UiTheme.Gold, new Rectangle(590, 34, 150, 42));

        UiTheme.DrawPanel(g, Images, PanelStyle.StoneWood, new Rectangle(36, 116, 360, 566));
        UiTheme.DrawPanel(g, Images, PanelStyle.StoneWood, new Rectangle(410, 116, 354, 566));

        if (_mode == ShopMode.Buy && _offers.Count > 0)
        {
            ShopOffer offer = _offers[_selected];
            int price = PurchasePrice(offer);
            string description = _session.Flags.ContainsKey("discount:market") && _service == LocationService.Market
                ? offer.Description + " Gezgin tüccar indirimi uygulandı."
                : offer.Description;
            DrawDetails(g, offer.Name, description, offer.IconPath, $"FİYAT: {price} ALTIN",
                _session.Player.Gold >= price ? UiTheme.Success : UiTheme.Danger);
        }
        else if (_mode == ShopMode.Sell && SellableItems.Count > 0)
        {
            Item item = SellableItems[_selected];
            string equipped = _session.Player.Equipment.IsEquipped(item) ? "Kuşanılmış; satılırsa otomatik çıkarılır." : "Envanter eşyası.";
            string detail = $"{LootGenerator.RarityLabel(item.Rarity)} · {equipped}";
            if (item is Weapon weapon) detail += $" {weapon.Technique.Name}: {weapon.Technique.Description}";
            DrawDetails(g, item.Name, detail, InventoryScreen.ItemIcon(item), $"SATIŞ: {_shop.SellPrice(item)} ALTIN",
                UiTheme.Success, item.Rarity);
        }
        else UiTheme.DrawTextCentered(g, "SATILABİLİR EŞYA YOK", UiTheme.Heading, UiTheme.Muted, new Rectangle(438, 250, 298, 80));

        UiTheme.DrawWrappedCentered(g, _message, UiTheme.Small, UiTheme.Text, new Rectangle(445, 628, 286, 42));
    }

    private void DrawDetails(Graphics g, string name, string description, string icon, string price, Color priceColor,
        ItemRarity rarity = ItemRarity.Common)
    {
        UiTheme.DrawTextCentered(g, name.ToUpperInvariant(), UiTheme.Heading,
            rarity == ItemRarity.Common ? UiTheme.Gold : InventoryScreen.RarityColor(rarity), new Rectangle(438, 145, 298, 42));
        UiTheme.DrawImageFit(g, Images.Get(icon), new Rectangle(494, 204, 186, 186));
        UiTheme.DrawWrappedCentered(g, description, UiTheme.Body, UiTheme.Text, new Rectangle(450, 405, 274, 78));
        UiTheme.DrawTextCentered(g, price, UiTheme.Heading, priceColor, new Rectangle(450, 485, 274, 40));
    }

    private int PurchasePrice(ShopOffer offer) =>
        _service == LocationService.Market && _session.Flags.ContainsKey("discount:market")
            ? Math.Max(1, offer.Price * 80 / 100)
            : offer.Price;
}
