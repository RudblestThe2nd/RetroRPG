namespace RetroRpg.AtlasTool;

internal static class Program
{
    private static int Main(string[] args)
    {
        string gameRoot = args.Length > 0 ? Path.GetFullPath(args[0]) : FindGameRoot();
        string sourceRoot = Path.Combine(gameRoot, "characters", "production", "atlases");
        string outputRoot = Path.Combine(gameRoot, "characters", "runtime-v1");
        var builder = new CharacterAtlasBuilder();
        int count = 0;

        foreach (string sourcePath in Directory.EnumerateFiles(sourceRoot, "*-animation-atlas-v1.png", SearchOption.AllDirectories))
        {
            DirectoryInfo genderDirectory = new(Path.GetDirectoryName(sourcePath)!);
            string gender = genderDirectory.Name;
            string race = genderDirectory.Parent!.Name;
            CharacterAtlasManifest manifest = builder.Build(sourcePath, outputRoot, race, gender);
            Console.WriteLine($"{race}/{gender}: {manifest.Frames.Count} kare");
            count++;
        }

        Console.WriteLine($"Tamamlandı: {count} atlas → {outputRoot}");
        return count == 22 ? 0 : 2;
    }

    private static string FindGameRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "characters", "production"))) return current.FullName;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException("game_design klasörü bulunamadı. İlk argüman olarak yolu verin.");
    }
}
