using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RetroRpg.AtlasTool;

internal static class AlphaRegionDetector
{
    public static IReadOnlyList<Rectangle> Detect(Bitmap bitmap, byte threshold = 96, int minimumArea = 350)
    {
        Rectangle bounds = new(0, 0, bitmap.Width, bitmap.Height);
        BitmapData data = bitmap.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            int byteCount = Math.Abs(data.Stride) * data.Height;
            byte[] pixels = new byte[byteCount];
            Marshal.Copy(data.Scan0, pixels, 0, byteCount);
            bool[] visited = new bool[bitmap.Width * bitmap.Height];
            var regions = new List<Rectangle>();
            var queue = new Queue<int>();

            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                {
                    int index = y * bitmap.Width + x;
                    if (visited[index] || Alpha(pixels, data.Stride, x, y) < threshold) continue;

                    visited[index] = true;
                    queue.Enqueue(index);
                    int minX = x, maxX = x, minY = y, maxY = y, area = 0;
                    while (queue.Count > 0)
                    {
                        int current = queue.Dequeue();
                        int cx = current % bitmap.Width;
                        int cy = current / bitmap.Width;
                        area++;
                        minX = Math.Min(minX, cx); maxX = Math.Max(maxX, cx);
                        minY = Math.Min(minY, cy); maxY = Math.Max(maxY, cy);

                        Visit(cx - 1, cy); Visit(cx + 1, cy); Visit(cx, cy - 1); Visit(cx, cy + 1);
                    }

                    if (area >= minimumArea) regions.Add(Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1));

                    void Visit(int nx, int ny)
                    {
                        if (nx < 0 || ny < 0 || nx >= bitmap.Width || ny >= bitmap.Height) return;
                        int next = ny * bitmap.Width + nx;
                        if (visited[next]) return;
                        visited[next] = true;
                        if (Alpha(pixels, data.Stride, nx, ny) >= threshold) queue.Enqueue(next);
                    }
                }

            return OrderIntoRows(regions);
        }
        finally { bitmap.UnlockBits(data); }
    }

    private static byte Alpha(byte[] pixels, int stride, int x, int y) => pixels[y * stride + x * 4 + 3];

    private static IReadOnlyList<Rectangle> OrderIntoRows(IEnumerable<Rectangle> regions)
    {
        var rows = new List<List<Rectangle>>();
        foreach (Rectangle region in regions.OrderBy(x => x.Top).ThenBy(x => x.Left))
        {
            List<Rectangle>? row = rows.FirstOrDefault(x => Math.Abs(x.Average(r => r.Top + r.Height / 2d) - (region.Top + region.Height / 2d)) < 70);
            if (row is null) { row = []; rows.Add(row); }
            row.Add(region);
        }

        return rows.OrderBy(x => x.Average(r => r.Top)).SelectMany(x => x.OrderBy(r => r.Left)).ToArray();
    }
}
