using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RetroRpg.Game;

// Oyundaki her görseli game_design kökündeki gerçek dosyadan yükler ve önbellekte tutar.
// Eksik dosya sessizce geçilmez: tam yol log dosyasına yazılır ve ekranın altında kırmızı uyarı olarak görünür.
internal sealed class UiImages : IDisposable
{
    private readonly string _root;
    private readonly string _logPath;
    private readonly Dictionary<string, Image?> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _missing = [];

    public UiImages(string root, string logPath)
    {
        _root = root;
        _logPath = logPath;
    }

    public IReadOnlyList<string> Missing => _missing;

    // trim: bazı UI görsellerinin şeffaf kenarlarında komşu parçalardan kalan kırıntılar var.
    // Kırpma, en büyük opak parçanın sınırlarını alır.
    public Image? Get(string relativePath, bool trim = false, bool mirror = false)
    {
        string key = (mirror ? "mirror|" : "") + (trim ? "trim|" : "") + relativePath;
        if (_cache.TryGetValue(key, out Image? cached)) return cached;

        string fullPath = Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Image? image = null;
        if (File.Exists(fullPath))
        {
            using Image loaded = Image.FromFile(fullPath);
            var bitmap = new Bitmap(loaded);
            image = trim ? TrimToLargestPart(bitmap) : bitmap;
            if (mirror) image.RotateFlip(RotateFlipType.RotateNoneFlipX);
        }
        else ReportMissing(fullPath);

        _cache[key] = image;
        return image;
    }

    public Cursor CreateCursor(string relativePath)
    {
        if (Get(relativePath) is not Bitmap bitmap) return Cursors.Default;
        Rectangle bounds = OpaqueBounds(bitmap);
        return CursorFromBitmap(bitmap, bounds.Left, bounds.Top);
    }

    private void ReportMissing(string fullPath)
    {
        if (_missing.Contains(fullPath)) return;
        _missing.Add(fullPath);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
            File.AppendAllText(_logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Eksik asset: {fullPath}{Environment.NewLine}");
        }
        catch (IOException) { }
    }

    private static Bitmap TrimToLargestPart(Bitmap source)
    {
        int width = source.Width, height = source.Height;
        byte[] alpha = ReadAlpha(source);
        var seen = new bool[width * height];
        var queue = new Queue<int>();
        Rectangle best = new(0, 0, width, height);
        int bestCount = 0;

        for (int start = 0; start < alpha.Length; start++)
        {
            if (seen[start] || alpha[start] <= 16) continue;
            int count = 0, left = width, top = height, right = 0, bottom = 0;
            seen[start] = true;
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                int index = queue.Dequeue();
                int x = index % width, y = index / width;
                count++;
                left = Math.Min(left, x); right = Math.Max(right, x);
                top = Math.Min(top, y); bottom = Math.Max(bottom, y);
                Visit(x + 1, y); Visit(x - 1, y); Visit(x, y + 1); Visit(x, y - 1);
            }
            if (count > bestCount)
            {
                bestCount = count;
                best = Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
            }
        }

        Bitmap trimmed = source.Clone(best, PixelFormat.Format32bppArgb);
        source.Dispose();
        return trimmed;

        void Visit(int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height) return;
            int index = y * width + x;
            if (seen[index] || alpha[index] <= 16) return;
            seen[index] = true;
            queue.Enqueue(index);
        }
    }

    private static Rectangle OpaqueBounds(Bitmap bitmap)
    {
        byte[] alpha = ReadAlpha(bitmap);
        int left = bitmap.Width, top = bitmap.Height;
        for (int i = 0; i < alpha.Length; i++)
        {
            if (alpha[i] <= 16) continue;
            left = Math.Min(left, i % bitmap.Width);
            top = Math.Min(top, i / bitmap.Width);
        }
        return new Rectangle(left, top, 1, 1);
    }

    private static byte[] ReadAlpha(Bitmap bitmap)
    {
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            var pixels = new byte[data.Stride * bitmap.Height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
            var alpha = new byte[bitmap.Width * bitmap.Height];
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                    alpha[y * bitmap.Width + x] = pixels[y * data.Stride + x * 4 + 3];
            return alpha;
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    // Windows imleci için sıcak noktayı (tıklama ucunu) ayarlayabilmek gerekiyor; Cursor sınıfı bunu doğrudan vermiyor.
    private static Cursor CursorFromBitmap(Bitmap bitmap, int hotspotX, int hotspotY)
    {
        IntPtr icon = bitmap.GetHicon();
        GetIconInfo(icon, out IconInfo info);
        info.IsIcon = false;
        info.HotspotX = hotspotX;
        info.HotspotY = hotspotY;
        IntPtr cursor = CreateIconIndirect(ref info);
        DestroyIcon(icon);
        return new Cursor(cursor);
    }

    public void Dispose()
    {
        foreach (Image? image in _cache.Values) image?.Dispose();
        _cache.Clear();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IconInfo
    {
        [MarshalAs(UnmanagedType.Bool)] public bool IsIcon;
        public int HotspotX;
        public int HotspotY;
        public IntPtr MaskBitmap;
        public IntPtr ColorBitmap;
    }

    [DllImport("user32.dll")] private static extern bool GetIconInfo(IntPtr icon, out IconInfo info);
    [DllImport("user32.dll")] private static extern IntPtr CreateIconIndirect(ref IconInfo info);
    [DllImport("user32.dll")] private static extern bool DestroyIcon(IntPtr icon);
}
