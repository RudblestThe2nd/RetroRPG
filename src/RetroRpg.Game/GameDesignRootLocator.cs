namespace RetroRpg.Game;

internal static class GameDesignRootLocator
{
    public static string Locate()
    {
        string? overridePath = Environment.GetEnvironmentVariable("RETRO_RPG_ASSET_ROOT");
        if (!string.IsNullOrWhiteSpace(overridePath) && Directory.Exists(overridePath)) return Path.GetFullPath(overridePath);

        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "assets")) &&
                Directory.Exists(Path.Combine(current.FullName, "characters"))) return current.FullName;
            current = current.Parent;
        }

        throw new DirectoryNotFoundException("game_design asset kökü bulunamadı. RETRO_RPG_ASSET_ROOT değişkenini ayarlayın.");
    }
}
