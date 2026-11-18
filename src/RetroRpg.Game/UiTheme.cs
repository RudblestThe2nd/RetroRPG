using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace RetroRpg.Game;

internal enum PanelStyle { PurpleHeader, Stone, StoneWood, GoldFrame }

// Ortak renkler, yazı tipleri ve çizim yardımcıları. Çerçeveler ui/assets/frames içindeki gerçek görsellerdir.
internal static class UiTheme
{
    public static readonly Color Background = Color.FromArgb(16, 13, 22);
    public static readonly Color Gold = Color.FromArgb(240, 200, 120);
    public static readonly Color Text = Color.FromArgb(242, 230, 205);
    public static readonly Color Muted = Color.FromArgb(160, 150, 170);
    public static readonly Color Danger = Color.FromArgb(214, 54, 54);
    public static readonly Color Success = Color.FromArgb(92, 200, 110);
    public static readonly Color Ink = Color.FromArgb(18, 16, 20);

    private static readonly PrivateFontCollection PixelFonts = new();
    private static bool _fontInitialized;

    public static Font Title { get; private set; } = new("Consolas", 20, FontStyle.Bold);
    public static Font Heading { get; private set; } = new("Consolas", 13, FontStyle.Bold);
    public static Font Body { get; private set; } = new("Consolas", 11, FontStyle.Bold);
    public static Font Small { get; private set; } = new("Consolas", 9, FontStyle.Bold);

    public static void Initialize(string gameDesignRoot)
    {
        if (_fontInitialized) return;
        _fontInitialized = true;
        string fontPath = Path.Combine(gameDesignRoot, "ui", "assets", "fonts", "PixelifySans-Variable.ttf");
        if (!File.Exists(fontPath)) return;

        try
        {
            PixelFonts.AddFontFile(fontPath);
            FontFamily family = PixelFonts.Families[0];
            FontStyle bold = family.IsStyleAvailable(FontStyle.Bold) ? FontStyle.Bold : FontStyle.Regular;
            Title = new Font(family, 28, bold, GraphicsUnit.Pixel);
            Heading = new Font(family, 20, bold, GraphicsUnit.Pixel);
            Body = new Font(family, 16, bold, GraphicsUnit.Pixel);
            Small = new Font(family, 13, FontStyle.Regular, GraphicsUnit.Pixel);
        }
        catch (ArgumentException)
        {
            // Font okunamazsa oyun güvenli varsayılanlarla açılmaya devam eder.
        }
    }

    public static void Prepare(Graphics g)
    {
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.None;
        g.SmoothingMode = SmoothingMode.None;
        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
    }

    public static void DrawText(Graphics g, string text, Font font, Color color, float x, float y)
    {
        using var shadow = new SolidBrush(Color.FromArgb(220, 0, 0, 0));
        using var brush = new SolidBrush(color);
        int px = (int)MathF.Round(x), py = (int)MathF.Round(y);
        g.DrawString(text, font, shadow, px + 1, py + 1);
        g.DrawString(text, font, brush, px, py);
    }

    public static void DrawTextCentered(Graphics g, string text, Font font, Color color, Rectangle area)
    {
        using var format = new StringFormat(StringFormat.GenericTypographic)
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap
        };
        using var shadow = new SolidBrush(Color.FromArgb(220, 0, 0, 0));
        using var brush = new SolidBrush(color);
        var shadowArea = new RectangleF(area.X + 1, area.Y + 1, area.Width, area.Height);
        g.DrawString(text, font, shadow, shadowArea, format);
        g.DrawString(text, font, brush, area, format);
    }

    public static void DrawWrapped(Graphics g, string text, Font font, Color color, Rectangle area)
    {
        using var brush = new SolidBrush(color);
        g.DrawString(text, font, brush, area);
    }

    public static void DrawWrappedCentered(Graphics g, string text, Font font, Color color, Rectangle area)
    {
        using var format = new StringFormat(StringFormat.GenericTypographic)
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisWord
        };
        using var shadow = new SolidBrush(Color.FromArgb(220, 0, 0, 0));
        using var brush = new SolidBrush(color);
        g.DrawString(text, font, shadow, new RectangleF(area.X + 1, area.Y + 1, area.Width, area.Height), format);
        g.DrawString(text, font, brush, area, format);
    }

    // Ekran görüntüsü QA'sı için: iki yazı boyutuna da sığmayan metinler burada toplanır.
    public static List<string> OverflowReports { get; } = [];

    // Metni satır kırarak alana ortalar. Normal boyut sığmazsa bir küçük boyut denenir;
    // o da sığmazsa metin kısaltılır ve QA raporuna yazılır.
    // ink: parşömen gibi açık zeminde gölgesiz mürekkep yazısı.
    public static void DrawWrappedFit(Graphics g, string text, Font font, Font fallback, Color color, Rectangle area, bool ink = false)
    {
        using var format = new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Center };
        Font chosen = Fits(g, text, font, area, format) ? font : fallback;
        if (!Fits(g, text, chosen, area, format) && !OverflowReports.Contains(text)) OverflowReports.Add(text);
        if (ink) DrawInkCentered(g, text, chosen, color, area);
        else DrawWrappedCentered(g, text, chosen, color, area);
    }

    public static void DrawInkCentered(Graphics g, string text, Font font, Color color, Rectangle area)
    {
        using var format = new StringFormat(StringFormat.GenericTypographic)
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Near,
            Trimming = StringTrimming.EllipsisWord
        };
        using var brush = new SolidBrush(color);
        int measuredHeight = Math.Min(area.Height,
            (int)Math.Ceiling(g.MeasureString(text, font, area.Width, format).Height));
        int y = area.Y + (area.Height - measuredHeight) / 2;
        // Dikey başlangıcı tam piksele sabitlemek parşömendeki siyah harflerin kaymasını engeller.
        g.DrawString(text, font, brush, new Rectangle(area.X, y, area.Width, measuredHeight), format);
    }

    private static bool Fits(Graphics g, string text, Font font, Rectangle area, StringFormat format) =>
        g.MeasureString(text, font, area.Width, format).Height <= area.Height;

    // Çerçeveyi dokuz parçaya bölerek çizer; köşeler bozulmadan panel istenen boyuta uzar.
    public static void DrawPanel(Graphics g, UiImages images, PanelStyle style, Rectangle dest)
    {
        (string path, int left, int top, int right, int bottom) = style switch
        {
            PanelStyle.PurpleHeader => ("ui/assets/frames/panels/panel-purple-header.png", 30, 62, 30, 30),
            PanelStyle.StoneWood => ("ui/assets/frames/panels/panel-wide-stone-wood.png", 34, 30, 34, 36),
            PanelStyle.GoldFrame => ("ui/assets/frames/tooltips/tooltip-black-gold-small.png", 14, 14, 14, 14),
            _ => ("ui/assets/frames/panels/panel-square-stone.png", 30, 30, 30, 30)
        };
        if (images.Get(path, trim: true) is not Image image)
        {
            DrawMissingBox(g, dest);
            return;
        }
        DrawNineSlice(g, image, dest, left, top, right, bottom);
    }

    public static void DrawNineSlice(Graphics g, Image image, Rectangle dest, int left, int top, int right, int bottom)
    {
        int sw = image.Width, sh = image.Height;
        int[] sx = [0, left, sw - right, sw];
        int[] sy = [0, top, sh - bottom, sh];
        int[] dx = [dest.Left, dest.Left + left, dest.Right - right, dest.Right];
        int[] dy = [dest.Top, dest.Top + top, dest.Bottom - bottom, dest.Bottom];
        for (int row = 0; row < 3; row++)
            for (int col = 0; col < 3; col++)
            {
                var source = Rectangle.FromLTRB(sx[col], sy[row], sx[col + 1], sy[row + 1]);
                var target = Rectangle.FromLTRB(dx[col], dy[row], dx[col + 1], dy[row + 1]);
                if (source.Width > 0 && source.Height > 0 && target.Width > 0 && target.Height > 0)
                    g.DrawImage(image, target, source, GraphicsUnit.Pixel);
            }
    }

    public static void DrawImage(Graphics g, Image? image, Rectangle dest)
    {
        if (image is null) DrawMissingBox(g, dest);
        else g.DrawImage(image, dest);
    }

    // Görseli oranını koruyarak alana sığdırır ve ortalar.
    public static void DrawImageFit(Graphics g, Image? image, Rectangle area)
    {
        if (image is null)
        {
            DrawMissingBox(g, area);
            return;
        }
        float scale = Math.Min((float)area.Width / image.Width, (float)area.Height / image.Height);
        int width = (int)(image.Width * scale), height = (int)(image.Height * scale);
        // Bütün oyun assetleri piksel-arttır; küçültmede de yumuşatma yapılmaz.
        InterpolationMode previous = g.InterpolationMode;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.DrawImage(image, area.X + (area.Width - width) / 2, area.Y + (area.Height - height) / 2, width, height);
        g.InterpolationMode = previous;
    }

    // Eski, daha yumuşak karakter karelerini sahnede iki kat piksel ızgarasına oturtur.
    public static void DrawPixelatedSprite(Graphics g, Image? image, Rectangle area, int pixelScale = 2)
    {
        if (image is null)
        {
            DrawMissingBox(g, area);
            return;
        }

        int lowWidth = Math.Max(1, area.Width / pixelScale);
        int lowHeight = Math.Max(1, area.Height / pixelScale);
        using var low = new Bitmap(lowWidth, lowHeight, PixelFormat.Format32bppArgb);
        using (Graphics buffer = Graphics.FromImage(low))
        {
            buffer.Clear(Color.Transparent);
            buffer.CompositingMode = CompositingMode.SourceCopy;
            buffer.InterpolationMode = InterpolationMode.NearestNeighbor;
            buffer.PixelOffsetMode = PixelOffsetMode.Half;
            buffer.DrawImage(image, new Rectangle(0, 0, lowWidth, lowHeight));
        }

        for (int y = 0; y < low.Height; y++)
            for (int x = 0; x < low.Width; x++)
            {
                Color color = low.GetPixel(x, y);
                if (color.A < 96)
                {
                    low.SetPixel(x, y, Color.Transparent);
                    continue;
                }
                int r = color.R / 16 * 16;
                int green = color.G / 16 * 16;
                int b = color.B / 16 * 16;
                low.SetPixel(x, y, Color.FromArgb(255, r, green, b));
            }

        InterpolationMode previous = g.InterpolationMode;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.DrawImage(low, area, new Rectangle(0, 0, low.Width, low.Height), GraphicsUnit.Pixel);
        g.InterpolationMode = previous;
    }

    public static void DrawBar(Graphics g, Rectangle area, int value, int max, Color fill)
    {
        using var back = new SolidBrush(Color.FromArgb(40, 34, 38));
        using var front = new SolidBrush(fill);
        using var border = new Pen(Color.FromArgb(120, 100, 70), 2);
        g.FillRectangle(back, area);
        int width = max <= 0 ? 0 : area.Width * Math.Clamp(value, 0, max) / max;
        g.FillRectangle(front, area.X, area.Y, width, area.Height);
        g.DrawRectangle(border, area);
    }

    public static void DrawDiceQuestion(Graphics g, Rectangle area)
    {
        Rectangle center = Rectangle.Inflate(area, -22, -7);
        using var disk = new SolidBrush(Color.FromArgb(235, 28, 20, 38));
        g.FillEllipse(disk, center);
        DrawTextCentered(g, "?", Title, Color.White, center);
    }

    public static void DrawDiceValue(Graphics g, Rectangle area, int value)
    {
        bool compact = area.Width < 80;
        Rectangle center = Rectangle.Inflate(area, compact ? -8 : -18, compact ? -8 : -5);
        using var disk = new SolidBrush(Color.FromArgb(255, 28, 20, 38));
        using var rim = new Pen(Color.FromArgb(220, 236, 189, 91), 2);
        g.FillEllipse(disk, center);
        g.DrawEllipse(rim, center);
        DrawTextCentered(g, value.ToString(), compact ? Small : Title, Color.White, center);
    }

    public static void Shade(Graphics g, Rectangle area, int alpha)
    {
        using var brush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0));
        g.FillRectangle(brush, area);
    }

    public static void DrawButton(Graphics g, UiImages images, PixelButton button)
    {
        PixelButtonState state = button.State;
        if (button.Skin is not null)
        {
            DrawImage(g, images.Get(button.Skin + PixelButton.StateSuffix(state), trim: true), button.Bounds);
        }
        else if (button.Icon is not null)
        {
            DrawImageFit(g, images.Get(button.Icon + PixelButton.StateSuffix(state)), button.Bounds);
        }
        else
        {
            // Ekran görselinde zaten çizili olan düğme: yalnız durum vurgusu eklenir.
            if (state == PixelButtonState.Hover)
            {
                using var glow = new SolidBrush(Color.FromArgb(40, 255, 220, 140));
                using var pen = new Pen(Gold, 3);
                g.FillRectangle(glow, button.Bounds);
                g.DrawRectangle(pen, button.Bounds);
            }
            else if (state == PixelButtonState.Pressed) Shade(g, button.Bounds, 90);
            else if (state == PixelButtonState.Disabled) Shade(g, button.Bounds, 150);
        }

        if (button.Picture is not null)
        {
            Rectangle inner = Rectangle.Inflate(button.Bounds, -6, -6);
            DrawImageFit(g, button.Picture, inner);
            if (state == PixelButtonState.Disabled) Shade(g, inner, 140);
        }

        if (button.Text.Length > 0 && (button.Skin is not null || button.Icon is null))
        {
            Color color = state == PixelButtonState.Disabled ? Muted : state == PixelButtonState.Hover ? Color.White : Text;
            var textArea = button.Bounds;
            if (state == PixelButtonState.Pressed) textArea.Offset(0, 2);
            DrawTextCentered(g, button.Text, button.Font ?? Body, color, textArea);
        }

        if (button.Selected && (button.Icon is not null || button.Picture is not null))
        {
            using var pen = new Pen(Gold, 3);
            g.DrawRectangle(pen, Rectangle.Inflate(button.Bounds, 2, 2));
        }
    }

    public static void DrawMissingBox(Graphics g, Rectangle area)
    {
        using var brush = new HatchBrush(HatchStyle.WideDownwardDiagonal, Color.FromArgb(200, 40, 40), Color.FromArgb(60, 10, 10));
        g.FillRectangle(brush, area);
    }

    public static void DrawMissingAssets(Graphics g, IReadOnlyList<string> missing, Size client)
    {
        if (missing.Count == 0) return;
        var area = new Rectangle(0, client.Height - 22, client.Width, 22);
        using var brush = new SolidBrush(Color.FromArgb(230, 140, 20, 20));
        g.FillRectangle(brush, area);
        DrawText(g, $"EKSİK ASSET ({missing.Count}): {missing[^1]}", Small, Color.White, 6, area.Y + 4);
    }
}
