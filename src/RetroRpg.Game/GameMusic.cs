using System.Text.Json;
using System.IO;
using System.Windows.Media;
using RetroRpg.Core.Common;

namespace RetroRpg.Game;

internal enum MusicTrack
{
    Menu, Forest, Swamp, Desert, Snow, Mountain, Tropical, Volcanic, Corrupted, Underground, Plains,
    Combat, Settlement, Inn, Camp, Story, Death
}

internal sealed record AudioSettingsData(int MusicVolume = 50, int EffectsVolume = 75);

// Kısa chiptune döngülerini ilk kullanımda üretir ve MediaPlayer ile efektlerden bağımsız çalar.
internal sealed class GameMusic : IDisposable
{
    private const int SampleRate = 22_050;
    private const int LoopSeconds = 8;
    private readonly string _cacheFolder;
    private readonly MediaPlayer _player = new();
    private MusicTrack? _current;

    public GameMusic(string cacheFolder, bool suppressPlayback)
    {
        _cacheFolder = cacheFolder;
        SuppressPlayback = suppressPlayback;
        _player.MediaEnded += (_, _) =>
        {
            _player.Position = TimeSpan.Zero;
            if (!SuppressPlayback && Volume > 0) _player.Play();
        };
    }

    public int Volume { get; private set; } = 50;
    public bool SuppressPlayback { get; }

    public void SetVolume(int volume)
    {
        Volume = Math.Clamp(volume, 0, 100);
        _player.Volume = Volume / 100d;
        if (SuppressPlayback) return;
        if (Volume == 0) _player.Pause();
        else if (_current is not null) _player.Play();
    }

    public void Play(MusicTrack track)
    {
        if (_current == track)
        {
            if (!SuppressPlayback && Volume > 0) _player.Play();
            return;
        }
        _current = track;
        if (SuppressPlayback) return;
        Directory.CreateDirectory(_cacheFolder);
        string path = Path.Combine(_cacheFolder, $"retro-rpg-{track.ToString().ToLowerInvariant()}-v1.wav");
        if (!File.Exists(path)) File.WriteAllBytes(path, CreateTrack(track));
        _player.Stop();
        _player.Open(new Uri(path));
        _player.Volume = Volume / 100d;
        if (Volume > 0) _player.Play();
    }

    public static MusicTrack ForBiome(Biome biome) => biome switch
    {
        Biome.Forest => MusicTrack.Forest,
        Biome.Swamp => MusicTrack.Swamp,
        Biome.Desert => MusicTrack.Desert,
        Biome.Snow => MusicTrack.Snow,
        Biome.Mountain => MusicTrack.Mountain,
        Biome.Tropical => MusicTrack.Tropical,
        Biome.Volcanic => MusicTrack.Volcanic,
        Biome.Corrupted => MusicTrack.Corrupted,
        Biome.Underground => MusicTrack.Underground,
        _ => MusicTrack.Plains
    };

    public static AudioSettingsData LoadSettings(string path)
    {
        try
        {
            return File.Exists(path)
                ? JsonSerializer.Deserialize<AudioSettingsData>(File.ReadAllText(path)) ?? new AudioSettingsData()
                : new AudioSettingsData();
        }
        catch { return new AudioSettingsData(); }
    }

    public static void SaveSettings(string path, AudioSettingsData settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static byte[] CreateTrack(MusicTrack track)
    {
        (double root, int[] notes, double pulse, double noise) = Profile(track);
        int count = SampleRate * LoopSeconds;
        short[] samples = new short[count];
        for (int i = 0; i < count; i++)
        {
            double t = i / (double)SampleRate;
            int step = (int)(t / LoopSeconds * 16) % 16;
            double local = t % (LoopSeconds / 16d) / (LoopSeconds / 16d);
            double leadFrequency = root * Math.Pow(2, notes[step] / 12d);
            double bassFrequency = root / 2 * Math.Pow(2, notes[step / 4 * 4] / 12d);
            double envelope = Math.Min(1, local * 8) * Math.Pow(1 - local, 0.55);
            double lead = Math.Sign(Math.Sin(Math.Tau * leadFrequency * t)) * envelope * 0.18;
            double bass = Math.Sin(Math.Tau * bassFrequency * t) * 0.17;
            double beat = local < 0.08 ? (1 - local / 0.08) * Math.Sin(Math.Tau * (70 - 35 * local) * t) * pulse : 0;
            double grain = PseudoNoise(t) * noise * (0.4 + envelope * 0.6);
            samples[i] = (short)(Math.Clamp(lead + bass + beat + grain, -1, 1) * short.MaxValue * 0.42);
        }
        return Wave(samples);
    }

    private static (double Root, int[] Notes, double Pulse, double Noise) Profile(MusicTrack track) => track switch
    {
        MusicTrack.Menu => (110, [0, 3, 7, 10, 7, 3, 0, -2, 0, 3, 7, 12, 10, 7, 3, 0], .10, .01),
        MusicTrack.Forest => (147, [0, 4, 7, 4, 2, 5, 9, 5, 0, 4, 7, 12, 9, 7, 4, 2], .07, .02),
        MusicTrack.Swamp => (82, [0, 3, 5, 3, -2, 0, 3, 0, 0, 3, 6, 3, -2, 0, -5, -2], .05, .05),
        MusicTrack.Desert => (123, [0, 1, 5, 7, 5, 1, 0, -4, 0, 1, 8, 7, 5, 1, 0, -4], .09, .025),
        MusicTrack.Snow => (196, [0, 7, 12, 7, 5, 12, 17, 12, 0, 7, 14, 12, 7, 5, 2, 0], .04, .015),
        MusicTrack.Mountain => (98, [0, 5, 7, 12, 7, 5, 3, 0, 0, 5, 10, 12, 10, 7, 5, 3], .14, .01),
        MusicTrack.Tropical => (165, [0, 4, 7, 9, 7, 4, 2, 4, 0, 4, 7, 11, 9, 7, 4, 2], .13, .025),
        MusicTrack.Volcanic => (73, [0, 1, 6, 5, 0, 1, 8, 6, 0, 1, 6, 10, 8, 6, 1, 0], .20, .05),
        MusicTrack.Corrupted => (92, [0, 1, 6, 2, -1, 3, 7, 1, 0, 6, 1, 8, 2, -1, -5, 1], .08, .06),
        MusicTrack.Underground => (65, [0, 3, 0, -2, 0, 5, 3, 0, -5, 0, 3, 0, -2, -5, -7, -5], .06, .04),
        MusicTrack.Plains => (131, [0, 4, 7, 12, 9, 7, 4, 2, 0, 4, 9, 12, 9, 7, 4, 0], .11, .01),
        MusicTrack.Combat => (98, [0, 0, 3, 0, 5, 3, 7, 5, 0, 0, 8, 7, 5, 3, 1, 0], .28, .045),
        MusicTrack.Settlement => (147, [0, 4, 7, 12, 7, 4, 2, 5, 0, 4, 9, 12, 9, 7, 4, 2], .08, .008),
        MusicTrack.Inn => (165, [0, 4, 7, 4, 5, 9, 12, 9, 7, 11, 14, 11, 5, 9, 7, 4], .10, .005),
        MusicTrack.Camp => (110, [0, 3, 7, 3, 0, 5, 8, 5, 0, 3, 10, 7, 5, 3, 0, -2], .04, .025),
        MusicTrack.Death => (55, [0, -1, -5, -7, -5, -8, -12, -8, 0, -5, -7, -12, -8, -12, -17, -12], .03, .04),
        _ => (82, [0, 3, 7, 3, 0, 1, 6, 1, 0, 3, 8, 7, 3, 1, -2, 0], .06, .02)
    };

    private static double PseudoNoise(double t)
    {
        double raw = Math.Sin((t * 931 + 1.7) * 12.9898) * 43_758.5453;
        return (raw - Math.Floor(raw)) * 2 - 1;
    }

    private static byte[] Wave(short[] samples)
    {
        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output);
        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + samples.Length * sizeof(short));
        writer.Write("WAVEfmt "u8.ToArray());
        writer.Write(16); writer.Write((short)1); writer.Write((short)1);
        writer.Write(SampleRate); writer.Write(SampleRate * sizeof(short));
        writer.Write((short)sizeof(short)); writer.Write((short)16);
        writer.Write("data"u8.ToArray()); writer.Write(samples.Length * sizeof(short));
        foreach (short sample in samples) writer.Write(sample);
        return output.ToArray();
    }

    public void Dispose()
    {
        _player.Stop();
        _player.Close();
    }
}
