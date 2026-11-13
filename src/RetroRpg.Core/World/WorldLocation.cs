using RetroRpg.Core.Common;
using RetroRpg.Core.Entities;

namespace RetroRpg.Core.World;

public enum LocationService { Inn, Market, Blacksmith, Healer, Temple, Barracks, WarRoom, Dungeon }

public abstract class WorldLocation : GameEntity
{
    protected WorldLocation(string id, string name, string description, string sceneAsset,
        params LocationService[] services) : base(name)
    {
        LocationId = id;
        Description = description;
        SceneAsset = sceneAsset;
        Services = services;
    }

    public string LocationId { get; }
    public string Description { get; }
    public string SceneAsset { get; }
    public IReadOnlyList<LocationService> Services { get; }
    public abstract string KindName { get; }
}

public sealed class VillageLocation(string id, string name)
    : WorldLocation(id, name, "Küçük ama güvenli bir yerleşim.",
        "assets/final-production-v5/scenes/settlements/01-village-exterior-v1.png",
        LocationService.Inn, LocationService.Market, LocationService.Healer)
{
    public override string KindName => "KÖY";
}

public sealed class TownLocation(string id, string name)
    : WorldLocation(id, name, "Ticaret yollarının birleştiği surlu kasaba.",
        "assets/final-production-v5/scenes/settlements/02-town-gate-square-v1.png",
        LocationService.Inn, LocationService.Market, LocationService.Blacksmith, LocationService.Temple)
{
    public override string KindName => "KASABA";
}

public sealed class CastleLocation(string id, string name)
    : WorldLocation(id, name, "Garnizon, yönetim salonu ve zindanları bulunan kale.",
        "assets/final-production-v5/scenes/settlements/03-castle-gate-outer-courtyard-v1.png",
        LocationService.Inn, LocationService.Market, LocationService.Blacksmith,
        LocationService.Barracks, LocationService.WarRoom, LocationService.Dungeon)
{
    public override string KindName => "KALE";
}

public sealed class EyeTowerLocation()
    : WorldLocation("eye-tower", "Göz Kulesi", "Zorunlu savaştan sonra girilebilen yozlaşmış kule.",
        "scenes/biomes/08-eye-tower-abyss.png", LocationService.WarRoom, LocationService.Dungeon)
{
    public override string KindName => "GÖZ KULESİ";
}

public static class WorldLocationCatalog
{
    public static WorldLocation? FindAt(HexTile tile)
    {
        if (tile.IsForcedCombat) return new EyeTowerLocation();
        if (tile.Coordinate == new HexCoordinate(0, 0)) return new VillageLocation("greenford", "Yeşilgeçit");
        if (tile.HasQuest) return new CastleLocation("quest-castle", "Kızılburç Kalesi");
        int hash = Math.Abs(tile.Coordinate.Q * 31 + tile.Coordinate.R * 17);
        return (hash % 11) switch
        {
            0 => new CastleLocation($"castle-{tile.Coordinate.Q}-{tile.Coordinate.R}", "Sınır Kalesi"),
            3 or 7 => new TownLocation($"town-{tile.Coordinate.Q}-{tile.Coordinate.R}", "Yol Kasabası"),
            5 => new VillageLocation($"village-{tile.Coordinate.Q}-{tile.Coordinate.R}", "Yolcu Köyü"),
            _ => null
        };
    }
}
