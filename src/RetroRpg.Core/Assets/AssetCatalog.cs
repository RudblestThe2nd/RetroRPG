using System.Text.Json;

namespace RetroRpg.Core.Assets;

public sealed class AssetCatalog
{
    private readonly Dictionary<string, string> _paths = new(StringComparer.OrdinalIgnoreCase);

    public AssetCatalog(string assetRoot) => AssetRoot = Path.GetFullPath(assetRoot);
    public string AssetRoot { get; }
    public IReadOnlyDictionary<string, string> Entries => _paths;

    public void Register(string id, string relativePath)
    {
        string fullPath = Path.GetFullPath(Path.Combine(AssetRoot, relativePath));
        if (!fullPath.StartsWith(AssetRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Asset yolu proje kökünün dışına çıkamaz.");
        _paths[id] = fullPath;
    }

    public string Resolve(string id)
    {
        if (!_paths.TryGetValue(id, out string? path)) throw new KeyNotFoundException($"Asset bulunamadı: {id}");
        if (!File.Exists(path)) throw new FileNotFoundException($"Asset dosyası kayıp: {path}", path);
        return path;
    }

    public bool TryResolve(string id, out string path)
    {
        path = string.Empty;
        if (!_paths.TryGetValue(id, out string? candidate) || !File.Exists(candidate)) return false;
        path = candidate;
        return true;
    }

    public void LoadJson(string manifestPath)
    {
        string json = File.ReadAllText(manifestPath);
        AssetManifest? manifest = JsonSerializer.Deserialize<AssetManifest>(json, JsonOptions.Default);
        if (manifest is null) throw new InvalidDataException($"Manifest okunamadı: {manifestPath}");
        foreach (AssetEntry entry in manifest.Assets) Register(entry.Id, entry.Path);
    }
}

public sealed record AssetEntry(string Id, string Path, string Type);
public sealed record AssetManifest(string Version, IReadOnlyList<AssetEntry> Assets);

public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
}
