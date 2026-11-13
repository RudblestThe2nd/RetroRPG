using RetroRpg.Core.Game;
using RetroRpg.Core.Items;
using RetroRpg.Core.World;

namespace RetroRpg.Game.Screens;

internal sealed class LocationScreen : GameScreen
{
    private readonly GameSession _session;
    private readonly WorldLocation _location;
    private readonly List<PixelButton> _serviceButtons = [];
    private string _scene;
    private string _message;

    public LocationScreen(MainGameForm host, GameSession session, WorldLocation location,
        LocationService? openedService = null, string? initialMessage = null) : base(host)
    {
        _session = session;
        _location = location;
        _scene = location.SceneAsset;
        _message = location.Description;
        for (int i = 0; i < location.Services.Count; i++)
        {
            LocationService service = location.Services[i];
            int x = 45 + i % 4 * 180, y = 618 + i / 4 * 58;
            _serviceButtons.Add(AddButton(new PixelButton(new Rectangle(x, y, 170, 48), Label(service), () => Open(service))
            { Skin = PixelButton.Medium }));
        }
        AddButton(new PixelButton(new Rectangle(590, 738, 165, 48), "HARİTAYA DÖN", () => Host.ShowMap()) { Skin = PixelButton.Medium });
        AddButton(new PixelButton(new Rectangle(45, 738, 165, 48), "DIŞARI ÇIK", ResetExterior) { Skin = PixelButton.Medium });
        if (openedService is LocationService initialService)
        {
            _scene = ServiceScene(initialService);
            _message = initialMessage ?? "İçeri girdin.";
            _session.Flags[$"location:{_location.LocationId}:{initialService}"] = "visited";
        }
    }

    private void Open(LocationService service)
    {
        _scene = ServiceScene(service);
        var player = _session.Player;
        switch (service)
        {
            case LocationService.Inn:
                Host.ShowInn(_location);
                return;
            case LocationService.Market:
            case LocationService.Blacksmith:
                Host.ShowShop(_location, service);
                return;
            case LocationService.Healer:
            case LocationService.Temple:
                if (Spend(4)) { player.Heal(8); _message = "Tedavi oldun. +8 CAN"; }
                break;
            case LocationService.Barracks:
            case LocationService.WarRoom:
                if (!_session.Flags.ContainsKey($"access:{_location.LocationId}:{service}"))
                {
                    Host.ShowGuardDialogue(_location, service);
                    return;
                }
                _session.Flags[$"location:{_location.LocationId}:{service}"] = "visited";
                _message = service == LocationService.Barracks
                    ? "İzinle kışlaya girdin; askerlerden bölge bilgisi aldın."
                    : "İzinle savaş odasına girdin; harekât haritasını inceledin.";
                break;
            default:
                _session.Flags[$"location:{_location.LocationId}:{service}"] = "visited";
                _message = service switch
                {
                    _ => "Zindanın kilitli geçidini keşfettin."
                };
                break;
        }
        Host.Autosave();
        Invalidate();
    }

    private bool Spend(int amount)
    {
        if (_session.Player.Gold < amount) { _message = $"Bunun için {amount} altın gerekiyor."; return false; }
        _session.Player.Gold -= amount;
        return true;
    }

    private void ResetExterior() { _scene = _location.SceneAsset; _message = _location.Description; Invalidate(); }

    private static string Label(LocationService service) => service switch
    {
        LocationService.Inn => "HAN",
        LocationService.Market => "PAZAR",
        LocationService.Blacksmith => "DEMİRCİ",
        LocationService.Healer => "ŞİFACI · 4",
        LocationService.Temple => "TAPINAK · 4",
        LocationService.Barracks => "KIŞLA",
        LocationService.WarRoom => "SAVAŞ ODASI",
        _ => "ZİNDAN"
    };

    private static string ServiceScene(LocationService service) => $"assets/final-production-v5/scenes/settlements/{service switch
    {
        LocationService.Inn => "04-inn-tavern-interior-v1.png",
        LocationService.Market => "05-market-merchant-v1.png",
        LocationService.Blacksmith => "06-blacksmith-armorer-interior-v1.png",
        LocationService.Healer => "07-alchemist-healer-interior-v1.png",
        LocationService.Temple => "08-temple-interior-v1.png",
        LocationService.Barracks => "09-barracks-armory-interior-v1.png",
        LocationService.WarRoom => "10-great-hall-war-room-v1.png",
        _ => "11-dungeon-secret-passage-v1.png"
    }}";

    protected override void Draw(Graphics g)
    {
        UiTheme.DrawImage(g, Images.Get(_scene), CanvasRectangle);
        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, new Rectangle(28, 18, 744, 88));
        UiTheme.DrawText(g, $"{_location.KindName} · {_location.Name}", UiTheme.Title, UiTheme.Gold, 52, 30);
        UiTheme.DrawText(g, $"ALTIN {_session.Player.Gold}", UiTheme.Body, UiTheme.Text, 610, 52);
        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, new Rectangle(28, 542, 744, 68));
        UiTheme.DrawTextCentered(g, _message, UiTheme.Small, UiTheme.Text, new Rectangle(50, 556, 700, 40));
        UiTheme.DrawPanel(g, Images, PanelStyle.GoldFrame, new Rectangle(28, 608, 744, 184));
    }
}
