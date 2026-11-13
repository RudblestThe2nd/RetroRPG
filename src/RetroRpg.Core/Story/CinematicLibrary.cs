using System.Text.Json;

namespace RetroRpg.Core.Story;

// Frames, game_design köküne göre '/' ayraçlı göreli PNG yollarıdır. Oyun GIF kullanmaz.
public sealed record CinematicSequence(string Id, IReadOnlyList<string> Frames, int Fps, IReadOnlyList<string> FrameMeanings);

// İki hikâye manifestini okur. Ana hikâye manifesti "directory", final boss manifesti "folder" alanını kullanır.
public sealed class CinematicLibrary
{
    public const string MainStoryManifest = "assets/final-production-v6/story/cinematics/main-story/manifest.json";
    public const string FinalBossManifest = "assets/final-production-v6/story/cinematics/final-boss/manifest.json";
    private const int DefaultFps = 1;

    private readonly Dictionary<string, CinematicSequence> _sequences;

    private CinematicLibrary(Dictionary<string, CinematicSequence> sequences) => _sequences = sequences;

    public IReadOnlyCollection<CinematicSequence> Sequences => _sequences.Values;

    public static CinematicLibrary Load(string gameDesignRoot)
    {
        var sequences = new Dictionary<string, CinematicSequence>(StringComparer.OrdinalIgnoreCase);
        foreach (string manifest in new[] { MainStoryManifest, FinalBossManifest })
        {
            string manifestFolder = manifest[..manifest.LastIndexOf('/')];
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(Path.Combine(gameDesignRoot, manifest)));
            foreach (JsonElement sequence in document.RootElement.GetProperty("sequences").EnumerateArray())
            {
                string id = sequence.GetProperty("id").GetString()!;
                string folder = (sequence.TryGetProperty("directory", out JsonElement directory) ? directory : sequence.GetProperty("folder")).GetString()!;
                string[] frames = sequence.GetProperty("frames").EnumerateArray()
                    .Select(x => $"{manifestFolder}/{folder}/{x.GetString()}").ToArray();
                int fps = sequence.TryGetProperty("fps", out JsonElement fpsValue) ? fpsValue.GetInt32() : DefaultFps;
                string[] meanings = sequence.TryGetProperty("frameMeanings", out JsonElement meaningValues)
                    ? meaningValues.EnumerateArray().Select(x => x.GetString()!).ToArray()
                    : [];
                sequences[id] = new CinematicSequence(id, frames, Math.Max(1, fps), meanings);
            }
        }
        return new CinematicLibrary(sequences);
    }

    public CinematicSequence Get(string id) =>
        _sequences.TryGetValue(id, out CinematicSequence? sequence) ? sequence : throw new KeyNotFoundException($"Sinematik dizi yok: {id}");

    // frameMeanings alanından kare bulur; örneğin corrupted-region-v1 içindeki "eye-bishop".
    public int FrameFor(string sequenceId, string meaning)
    {
        int index = Get(sequenceId).FrameMeanings.ToList().IndexOf(meaning);
        return index >= 0 ? index : throw new KeyNotFoundException($"{sequenceId} içinde '{meaning}' karesi yok.");
    }

    // Olayın sahnesindeki kareler sırayla döner; sabit sahne tek kare döner.
    public IReadOnlyList<string> FramesFor(StoryScene scene)
    {
        if (scene.SequenceId is null) return [scene.ImagePath!];
        IReadOnlyList<string> frames = Get(scene.SequenceId).Frames;
        int first = scene.FrameMeaning is string meaning ? FrameFor(scene.SequenceId, meaning) : scene.FirstFrame;
        if (first < 0 || first + scene.FrameCount > frames.Count)
            throw new ArgumentOutOfRangeException(nameof(scene), $"{scene.SequenceId} kare aralığı geçersiz.");
        return frames.Skip(first).Take(scene.FrameCount).ToArray();
    }
}
