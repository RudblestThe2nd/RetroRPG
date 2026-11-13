using System.Text.Json;
using RetroRpg.Core.Assets;
using RetroRpg.Core.Common;

namespace RetroRpg.Core.Rendering;

public sealed record RuntimePoint(int X, int Y);
public sealed record RuntimeRect(int X, int Y, int Width, int Height);
public sealed record RuntimeFrame(
    string Id, int SourceIndex, int SourceRow, CharacterAnimation Animation, Direction Direction,
    int AnimationFrame, int DurationMs, bool Loop, RuntimeRect SourceRectangle, RuntimePoint Pivot,
    IReadOnlyDictionary<string, RuntimePoint> Anchors, string RuntimePath);
public sealed record RuntimeAtlasManifest(
    string Version, string Race, string Gender, string SourcePath, int CellWidth, int CellHeight,
    IReadOnlyList<RuntimeFrame> Frames);

public sealed class CharacterAtlasRuntime
{
    private readonly RuntimeAtlasManifest _manifest;
    private readonly string _runtimeRoot;

    private CharacterAtlasRuntime(RuntimeAtlasManifest manifest, string runtimeRoot)
    {
        _manifest = manifest;
        _runtimeRoot = Path.GetFullPath(runtimeRoot);
    }

    public string Race => _manifest.Race;
    public string Gender => _manifest.Gender;
    public IReadOnlyList<RuntimeFrame> Frames => _manifest.Frames;

    public static CharacterAtlasRuntime Load(string manifestPath, string runtimeRoot)
    {
        RuntimeAtlasManifest manifest = JsonSerializer.Deserialize<RuntimeAtlasManifest>(File.ReadAllText(manifestPath), JsonOptions.Default)
            ?? throw new InvalidDataException("Karakter atlas manifesti okunamadı.");
        return new CharacterAtlasRuntime(manifest, runtimeRoot);
    }

    public RuntimeFrame Select(CharacterAnimation animation, Direction direction, int frameIndex = 0)
    {
        RuntimeFrame[] matches = _manifest.Frames
            .Where(x => x.Animation == animation && x.Direction == direction)
            .OrderBy(x => x.AnimationFrame).ToArray();
        if (matches.Length == 0) throw new KeyNotFoundException($"Animasyon karesi yok: {animation}/{direction}");
        return matches[Math.Abs(frameIndex) % matches.Length];
    }

    public string ResolvePath(RuntimeFrame frame)
    {
        string path = Path.GetFullPath(Path.Combine(_runtimeRoot, frame.RuntimePath));
        if (!path.StartsWith(_runtimeRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(path))
            throw new FileNotFoundException("Runtime karakter karesi bulunamadı.", path);
        return path;
    }
}
