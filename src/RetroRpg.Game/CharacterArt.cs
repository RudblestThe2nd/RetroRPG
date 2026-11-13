using RetroRpg.Core.Common;
using RetroRpg.Core.Rendering;

namespace RetroRpg.Game;

// Oyuncu karakterinin portre, harita tokenı ve runtime animasyon karelerinin yolları.
internal static class CharacterArt
{
    public const int PortraitCount = 12;
    private static readonly Dictionary<string, CharacterAtlasRuntime?> Runtimes = [];

    public static string PortraitPath(string raceId, Gender gender, int index)
    {
        string g = gender.ToString().ToLowerInvariant();
        return $"characters/face-variants-v1/{raceId}/portraits/{g}/{raceId}-{g}-face-{index:00}-v1.png";
    }

    public static string TokenPath(string raceId, Gender gender) =>
        $"characters/map-tokens-v1/tokens/players/player-{raceId}-{gender.ToString().ToLowerInvariant()}-token-v1.png";

    // characters/runtime-v1 manifestinden istenen animasyon karesinin tam yolunu verir.
    public static string FramePath(MainGameForm host, string raceId, Gender gender, CharacterAnimation animation, Direction direction, int frame = 0)
    {
        string key = $"{raceId}.{gender.ToString().ToLowerInvariant()}";
        if (!Runtimes.TryGetValue(key, out CharacterAtlasRuntime? runtime))
        {
            runtime = host.Assets.TryResolve($"character.{key}", out string manifest)
                ? CharacterAtlasRuntime.Load(manifest, Path.Combine(host.GameDesignRoot, "characters", "runtime-v1"))
                : null;
            Runtimes[key] = runtime;
        }
        if (runtime is null) return $"characters/runtime-v1/{raceId}/{gender.ToString().ToLowerInvariant()}/manifest.json";
        RuntimeFrame selected = runtime.Select(animation, direction, frame);
        return Path.Combine(host.GameDesignRoot, "characters", "runtime-v1", selected.RuntimePath);
    }

    public static int FrameCount(MainGameForm host, string raceId, Gender gender, CharacterAnimation animation, Direction direction)
    {
        FramePath(host, raceId, gender, animation, direction);
        string key = $"{raceId}.{gender.ToString().ToLowerInvariant()}";
        return Runtimes[key]?.Frames.Count(x => x.Animation == animation && x.Direction == direction) ?? 1;
    }
}
