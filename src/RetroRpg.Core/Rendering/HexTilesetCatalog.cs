using System.Text.Json;
using RetroRpg.Core.Assets;
using RetroRpg.Core.Common;
using RetroRpg.Core.World;

namespace RetroRpg.Core.Rendering;

public sealed record HexTileDefinition(string Id, string Category, int MovementCost, bool Passable, int Danger, string Image);
public sealed record HexTilesetManifest(string Pack, string Orientation, IReadOnlyList<HexTileDefinition> Tiles);

// tilesets/{biome}-v1 manifestlerini okur; her hex için biyomunun temel karolarından birini seçer.
public sealed class HexTilesetCatalog
{
    public const string OverlayFolder = "tilesets/hex-overlays-v1/static";
    private readonly Dictionary<Biome, string[]> _baseTiles = [];

    public static HexTilesetCatalog Load(string assetRoot)
    {
        var catalog = new HexTilesetCatalog();
        foreach (Biome biome in Enum.GetValues<Biome>())
        {
            string name = biome.ToString().ToLowerInvariant();
            string folder = $"tilesets/{name}-v1";
            string manifestPath = Path.Combine(assetRoot, folder, "manifests", $"{name}-hex-tileset-v1.json");
            HexTilesetManifest manifest = JsonSerializer.Deserialize<HexTilesetManifest>(File.ReadAllText(manifestPath), JsonOptions.Default)
                ?? throw new InvalidDataException($"Tileset manifesti okunamadı: {manifestPath}");
            catalog._baseTiles[biome] = manifest.Tiles
                .Where(x => x.Category == "base" && x.Passable)
                .Select(x => $"{folder}/{x.Image}")
                .ToArray();
        }
        return catalog;
    }

    public string BaseTilePath(Biome biome, HexCoordinate coordinate)
    {
        string[] tiles = _baseTiles[biome];
        int seed = Math.Abs(coordinate.Q * 73856093 ^ coordinate.R * 19349663);
        return tiles[seed % tiles.Length];
    }

    public static string OverlayPath(string overlayFile) => $"{OverlayFolder}/{overlayFile}.png";
}
