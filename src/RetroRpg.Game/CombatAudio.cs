using System.Media;

namespace RetroRpg.Game;

// Savaş sesleri küçük retro dalga biçimleri olarak bellekte üretilir. Böylece ses dosyası
// kaybolduğunda sessiz kalan butonlar olmaz ve bütün vuruşlar aynı ses dilini kullanır.
internal enum CombatSound { Dice, Swing, Hit, Critical, Miss, Block, Heal, Victory }

internal static class CombatAudio
{
    private const int SampleRate = 11_025;
    private static readonly List<MemoryStream> Streams = [];
    private static readonly Dictionary<CombatSound, SoundPlayer> Players = CreatePlayers();

    public static bool Enabled { get; set; } = true;
    internal static bool SuppressPlayback { get; set; }

    public static void Play(CombatSound sound)
    {
        if (!Enabled || SuppressPlayback) return;
        try { Players[sound].Play(); }
        catch { /* Ses aygıtı yoksa oyun devam eder. */ }
    }

    private static Dictionary<CombatSound, SoundPlayer> CreatePlayers()
    {
        var players = new Dictionary<CombatSound, SoundPlayer>();
        foreach (CombatSound sound in Enum.GetValues<CombatSound>())
        {
            var stream = new MemoryStream(CreateWave(sound));
            var player = new SoundPlayer(stream);
            player.Load();
            Streams.Add(stream);
            players[sound] = player;
        }
        return players;
    }

    private static byte[] CreateWave(CombatSound sound)
    {
        double seconds = sound switch
        {
            CombatSound.Dice => 0.22,
            CombatSound.Swing => 0.14,
            CombatSound.Hit => 0.16,
            CombatSound.Critical => 0.28,
            CombatSound.Miss => 0.18,
            CombatSound.Block => 0.20,
            CombatSound.Heal => 0.34,
            _ => 0.42
        };
        int count = (int)(SampleRate * seconds);
        short[] samples = new short[count];

        for (int i = 0; i < count; i++)
        {
            double t = i / (double)SampleRate;
            double progress = i / (double)Math.Max(1, count - 1);
            double envelope = Math.Pow(1 - progress, sound is CombatSound.Heal or CombatSound.Victory ? 0.45 : 1.8);
            double value = Sample(sound, t, progress) * envelope;
            samples[i] = (short)(Math.Clamp(value, -1, 1) * short.MaxValue * 0.34);
        }

        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output);
        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + samples.Length * sizeof(short));
        writer.Write("WAVEfmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(SampleRate);
        writer.Write(SampleRate * sizeof(short));
        writer.Write((short)sizeof(short));
        writer.Write((short)16);
        writer.Write("data"u8.ToArray());
        writer.Write(samples.Length * sizeof(short));
        foreach (short sample in samples) writer.Write(sample);
        return output.ToArray();
    }

    private static double Sample(CombatSound sound, double t, double progress)
    {
        double Noise(double speed)
        {
            double raw = Math.Sin((t * speed + 1.7) * 12.9898) * 43_758.5453;
            return (raw - Math.Floor(raw)) * 2 - 1;
        }

        double Tone(double frequency) => Math.Sin(Math.Tau * frequency * t);
        return sound switch
        {
            CombatSound.Dice => Noise(1_500) * (0.55 + Math.Abs(Tone(38)) * 0.35) + Tone(120) * 0.18,
            CombatSound.Swing => Tone(980 - 700 * progress) + Noise(180) * 0.18,
            CombatSound.Hit => Tone(105) * 0.65 + Noise(850) * 0.85,
            CombatSound.Critical => Tone(90) * 0.75 + Noise(1_100) * 0.9 + Tone(900 + 500 * progress) * 0.35,
            CombatSound.Miss => Tone(520 - 260 * progress) * 0.55 + Noise(240) * 0.12,
            CombatSound.Block => Math.Sign(Tone(progress < 0.45 ? 1_150 : 760)) * 0.65 + Tone(2_200) * 0.25,
            CombatSound.Heal => Tone(progress < 0.33 ? 440 : progress < 0.66 ? 660 : 880) * 0.72,
            CombatSound.Victory => Tone(progress < 0.25 ? 392 : progress < 0.50 ? 523 : progress < 0.75 ? 659 : 784) * 0.72,
            _ => 0
        };
    }
}
