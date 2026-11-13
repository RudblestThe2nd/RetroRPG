namespace RetroRpg.Game;

internal enum PixelButtonState { Normal, Hover, Pressed, Disabled }

// Ekrana çizilen düğme. Görünümü ui/assets içindeki _normal/_hover/_pressed/_disabled dosyalarından gelir.
// Skin ve Icon boşsa düğme, ekran görselinin üstündeki hazır düğme çiziminin tıklama alanıdır.
internal sealed class PixelButton(Rectangle bounds, string text, Action onClick)
{
    public const string Large = "ui/assets/controls/buttons/large/button-large";
    public const string Medium = "ui/assets/controls/buttons/medium/button-medium";
    public const string MedievalLarge = "assets/final-production-v4/ui/controls/buttons/large/button-large";

    public Rectangle Bounds { get; set; } = bounds;
    public string Text { get; set; } = text;
    public Action OnClick { get; } = onClick;
    public string? Skin { get; init; }
    public string? Icon { get; set; }
    public Image? Picture { get; set; }
    public bool Enabled { get; set; } = true;
    public bool Visible { get; set; } = true;
    public bool Selected { get; set; }
    public Font? Font { get; init; }

    internal bool IsHovered { get; set; }
    internal bool IsPressed { get; set; }

    public PixelButtonState State =>
        !Enabled ? PixelButtonState.Disabled :
        IsPressed && IsHovered ? PixelButtonState.Pressed :
        IsHovered || Selected ? PixelButtonState.Hover :
        PixelButtonState.Normal;

    public static string StateSuffix(PixelButtonState state) => "_" + state.ToString().ToLowerInvariant() + ".png";
}
