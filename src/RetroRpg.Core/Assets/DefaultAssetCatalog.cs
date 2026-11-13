namespace RetroRpg.Core.Assets;

public static class DefaultAssetCatalog
{
    public static AssetCatalog Create(string gameDesignRoot)
    {
        var catalog = new AssetCatalog(gameDesignRoot);
        catalog.Register("map.world.2d", @"maps\hex\world-map-hex-2d-v1.png");
        catalog.Register("map.world.isometric", @"assets\final-production-v1\maps\world-map-hex-isometric-v1.png");
        catalog.Register("ui.combat.simple", @"assets\final-production-v2\ui\screens\runtime-800\combat-screen-simple-d20-v1.png");
        catalog.Register("dice.d20", @"assets\final-production-v2\combat\dice\runtime-256\d20-icon-v1.png");
        catalog.Register("manifest.production.v1", @"assets\final-production-v1\manifests\final-production-v1.json");
        catalog.Register("manifest.production.v2", @"assets\final-production-v2\manifests\final-production-v2.json");
        string projectManifest = Path.Combine(gameDesignRoot, "Content", "manifests", "asset-catalog-v1.json");
        if (File.Exists(projectManifest)) catalog.LoadJson(projectManifest);
        return catalog;
    }
}
