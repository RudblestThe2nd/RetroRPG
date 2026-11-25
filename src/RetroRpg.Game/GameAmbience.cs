using System.Windows.Media;
using RetroRpg.Core.Common;

namespace RetroRpg.Game;

internal enum AmbienceTrack
{
    None, Forest, Swamp, Desert, Snow, Mountain, Tropical, Volcanic, Corrupted, Underground, Plains,
    Settlement, Inn, Camp, Death
}

// Müzikten bağımsız ikinci, düşük sesli bir döngü çalar. Dosyalar ilk kullanımda yerel önbelleğe üretilir.
internal sealed class GameAmbience : IDisposable
{
    private const int SampleRate = 22_050;
    private const int LoopSeconds = 8;
    private readonly string _cacheFolder;
    private readonly MediaPlayer _player = new();
    private AmbienceTrack _current;

    public GameAmbience(string cacheFolder, bool suppressPlayback)
    {
        _cacheFolder = cacheFolder;
        SuppressPlayback = suppressPlayback;
        _player.MediaEnded += (_, _) =>
        {
            _player.Position = TimeSpan.Zero;
            if (!SuppressPlayback && Volume > 0 && _current != AmbienceTrack.None) _player.Play();
        };
    }

    public int Volume { get; private set; } = 50;
    public bool SuppressPlayback { get; }

    public void SetVolume(int volume)
    {
        Volume = Math.Clamp(volume, 0, 100);
        _player.Volume = Volume / 100d * 0.38;
        if (SuppressPlayback) return;
        if (Volume == 0 || _current == AmbienceTrack.None) _player.Pause();
        else _player.Play();
    }

    public void Play(AmbienceTrack track)
    {
        if (_current == track) return;
        _current = track;
        _player.Stop();
        if (track == AmbienceTrack.None || SuppressPlayback) return;

        Directory.CreateDirectory(_cacheFolder);
        string path = Path.Combine(_cacheFolder, $"retro-rpg-ambience-{track.ToString().ToLowerInvariant()}-v1.wav");
        if (!File.Exists(path)) File.WriteAllBytes(path, CreateTrack(track));
        _player.Open(new Uri(path));
        _player.Volume = Volume / 100d * 0.38;
        if (Volume > 0) _player.Play();
    }

    public static AmbienceTrack ForBiome(Biome biome) => biome switch
    {
        Biome.Forest => AmbienceTrack.Forest,
        Biome.Swamp => AmbienceTrack.Swamp,
        Biome.Desert => AmbienceTrack.Desert,
        Biome.Snow => AmbienceTrack.Snow,
        Biome.Mountain => AmbienceTrack.Mountain,
        Biome.Tropical => AmbienceTrack.Tropical,
        Biome.Volcanic => AmbienceTrack.Volcanic,
        Biome.Corrupted => AmbienceTrack.Corrupted,
        Biome.Underground => AmbienceTrack.Underground,
        _ => AmbienceTrack.Plains
    };

    private static byte[] CreateTrack(AmbienceTrack track)
    {
        (double wind, double water, double fire, double tone, double root) = Profile(track);
        short[] samples = new short[SampleRate * LoopSeconds];
        double smoothNoise = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            double t = i / (double)SampleRate;
            double noise = PseudoNoise(t * 3.1);
            smoothNoise = smoothNoise * 0.994 + noise * 0.006;
            double gust = smoothNoise * (0.65 + 0.35 * Math.Sin(Math.Tau * t / 4));
            double stream = (Math.Sin(Math.Tau * 173 * t) + Math.Sin(Math.Tau * 257 * t)) *
                            (0.35 + 0.15 * Math.Sin(Math.Tau * t / 2.7));
            double sparkNoise = PseudoNoise(t * 47.3);
            double crackle = Math.Abs(sparkNoise) > 0.88 ? sparkNoise : 0;
            double drone = Math.Sin(Math.Tau * root * t) * (0.7 + 0.3 * Math.Sin(Math.Tau * t / 6));
            double sample = gust * wind + stream * water + crackle * fire + drone * tone;
            samples[i] = (short)(Math.Clamp(sample, -1, 1) * short.MaxValue * 0.46);
        }
        return Wave(samples);
    }

    private static (double Wind, double Water, double Fire, double Tone, double Root) Profile(AmbienceTrack track) => track switch
    {
        AmbienceTrack.Forest => (.20, .025, .01, .015, 82),
        AmbienceTrack.Swamp => (.09, .08, .015, .04, 55),
        AmbienceTrack.Desert => (.32, 0, 0, .02, 73),
        AmbienceTrack.Snow => (.27, 0, 0, .025, 98),
        AmbienceTrack.Mountain => (.36, 0, 0, .02, 65),
        AmbienceTrack.Tropical => (.13, .14, 0, .015, 110),
        AmbienceTrack.Volcanic => (.12, 0, .32, .09, 46),
        AmbienceTrack.Corrupted => (.18, 0, .05, .12, 41),
        AmbienceTrack.Underground => (.035, .045, .08, .08, 49),
        AmbienceTrack.Settlement => (.08, .025, .06, .025, 98),
        AmbienceTrack.Inn => (.02, 0, .28, .035, 110),
        AmbienceTrack.Camp => (.12, 0, .24, .025, 82),
        AmbienceTrack.Death => (.24, 0, 0, .08, 37),
        _ => (.18, .015, 0, .015, 98)
    };

    private static double PseudoNoise(double t)
    {
        double raw = Math.Sin((t * 971 + 2.1) * 12.9898) * 43_758.5453;
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
