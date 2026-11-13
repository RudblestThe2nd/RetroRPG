using RetroRpg.Core.Game;
using RetroRpg.Core.Items;

namespace RetroRpg.Game.Screens;

internal sealed class CampScreen : GameScreen
{
    private readonly GameSession _session;
    private string _message = "Kamp hazır.";

    public CampScreen(MainGameForm host, GameSession session) : base(host)
    {
        _session = session;
        AddButton(new PixelButton(new Rectangle(75, 205, 170, 54), "UZUN DİNLEN", Rest) { Skin = PixelButton.Medium });
        AddButton(new PixelButton(new Rectangle(75, 280, 170, 54), "YEMEK HAZIRLA", Cook) { Skin = PixelButton.Medium });
        AddButton(new PixelButton(new Rectangle(75, 355, 170, 54), "ENVANTER", () => Host.ShowInventory()) { Skin = PixelButton.Medium });
        AddButton(new PixelButton(new Rectangle(75, 430, 170, 54), "KAMBI TOPLA", () => Host.ShowMap()) { Skin = PixelButton.Medium });
        AddButton(new PixelButton(new Rectangle(235, 708, 330, 56), "HARİTAYA DÖN", () => Host.ShowMap()) { Skin = PixelButton.MedievalLarge });
    }

    private void Rest()
    {
        int healed = _session.Player.MaxHealth - _session.Player.Health;
        _session.Player.Heal(_session.Player.MaxHealth);
        _message = healed > 0 ? $"Dinlendin. +{healed} CAN." : "Zaten tamamen dinlenmişsin.";
        Host.Autosave();
        Invalidate();
    }

    private void Cook()
    {
        Consumable? ingredient = _session.Player.Inventory.Items.OfType<Consumable>()
            .FirstOrDefault(x => x.Name.Contains("Azık", StringComparison.OrdinalIgnoreCase) ||
                                 x.Name.Contains("Bal", StringComparison.OrdinalIgnoreCase) ||
                                 x.Name.Contains("Ot", StringComparison.OrdinalIgnoreCase));
        if (ingredient is null) _message = "Yemek için Yol Azığı, yabani bal veya şifalı ot gerekiyor.";
        else
        {
            _session.Player.Inventory.Remove(ingredient);
            _session.Player.Inventory.Add(new Consumable("Kamp Yemeği", 6, 0.4, 5));
            _message = $"{ingredient.Name} kullanılarak bir kamp yemeği hazırlandı.";
            Host.Autosave();
        }
        Invalidate();
    }

    public override void HandleKey(Keys key) { if (key == Keys.Escape) Host.ShowMap(); }

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get("assets/final-production-v5/ui/screens/runtime-800/camp-wood-red-v1.png"), CanvasRectangle);
        UiTheme.DrawTextCentered(g, "KAMP", UiTheme.Title, UiTheme.Gold, new Rectangle(260, 30, 280, 48));
        var p = _session.Player;
        UiTheme.DrawImageFit(g, Images.Get(CharacterArt.PortraitPath(p.Race.Id, p.Gender, _session.PortraitIndex)), new Rectangle(610, 185, 84, 84));
        UiTheme.DrawText(g, $"CAN {p.Health}/{p.MaxHealth}", UiTheme.Body, UiTheme.Text, 600, 292);
        UiTheme.DrawText(g, $"ALTIN {p.Gold}", UiTheme.Body, UiTheme.Gold, 600, 334);
        UiTheme.DrawText(g, $"EŞYA {p.Inventory.Items.Count}", UiTheme.Body, UiTheme.Text, 600, 376);
        int ingredients = p.Inventory.Items.OfType<Consumable>().Count(x => x.Name.Contains("Azık") || x.Name.Contains("Bal") || x.Name.Contains("Ot"));
        UiTheme.DrawText(g, $"MALZEME {ingredients}", UiTheme.Small, UiTheme.Success, 600, 410);
        UiTheme.DrawWrapped(g, _message, UiTheme.Body, UiTheme.Text, new Rectangle(250, 560, 470, 95));
    }
}
