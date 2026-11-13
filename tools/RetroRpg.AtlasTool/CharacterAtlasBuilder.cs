using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.Json;
using RetroRpg.Core.Assets;
using RetroRpg.Core.Common;

namespace RetroRpg.AtlasTool;

public sealed class CharacterAtlasBuilder
{
    private const int CellSize = 256;

    public CharacterAtlasManifest Build(string sourcePath, string outputRoot, string race, string gender)
    {
        using var source = new Bitmap(sourcePath);
        IReadOnlyList<Rectangle> regions = AlphaRegionDetector.Detect(source);
        if (regions.Count < 12) throw new InvalidDataException($"Yeterli karakter karesi bulunamadı: {sourcePath} ({regions.Count})");

        string characterRoot = Path.Combine(outputRoot, race, gender);
        string safeRoot = Path.GetFullPath(outputRoot) + Path.DirectorySeparatorChar;
        string safeCharacterRoot = Path.GetFullPath(characterRoot);
        if (!safeCharacterRoot.StartsWith(safeRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Runtime karakter yolu çıktı kökünün dışında olamaz.");
        if (Directory.Exists(safeCharacterRoot)) Directory.Delete(safeCharacterRoot, true);
        Directory.CreateDirectory(characterRoot);
        var frames = new List<AtlasFrame>();
        List<List<(Rectangle Rect, int SourceIndex)>> rows = GroupRows(regions);

        int sourceIndex = 0;
        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            List<(Rectangle Rect, int SourceIndex)> row = rows[rowIndex];
            for (int column = 0; column < row.Count; column++)
            {
                Rectangle region = InflateInside(row[column].Rect, source.Size, 3);
                CharacterAnimation animation = AnimationFor(rowIndex, column);
                int groupStart = GroupStartFor(rowIndex, column);
                Direction direction = (Direction)((column - groupStart) % 4);
                int animationFrame = (column - groupStart) / 4;
                string id = $"{animation.ToString().ToLowerInvariant()}-{direction.ToString().ToLowerInvariant()}-{animationFrame:00}";
                string animationFolder = Path.Combine(characterRoot, animation.ToString().ToLowerInvariant(), direction.ToString().ToLowerInvariant());
                Directory.CreateDirectory(animationFolder);
                string framePath = Path.Combine(animationFolder, $"{animationFrame:00}.png");
                Normalize(source, region, framePath);

                frames.Add(new AtlasFrame(
                    id, sourceIndex++, rowIndex, animation, direction, animationFrame,
                    animation == CharacterAnimation.Idle ? 400 : 120,
                    animation is CharacterAnimation.Idle or CharacterAnimation.Walk or CharacterAnimation.Talk,
                    new IntRect(region.X, region.Y, region.Width, region.Height),
                    new IntPoint(128, 230),
                    new Dictionary<string, IntPoint>
                    {
                        ["main-hand"] = new(166, 142),
                        ["off-hand"] = new(90, 142),
                        ["head"] = new(128, 62),
                        ["feet"] = new(128, 230)
                    },
                    Path.GetRelativePath(outputRoot, framePath).Replace('\\', '/')));
            }
        }

        var manifest = new CharacterAtlasManifest("runtime-v1", race, gender,
            Path.GetFullPath(sourcePath), CellSize, CellSize, frames);
        string manifestPath = Path.Combine(characterRoot, "manifest.json");
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions.Default));
        return manifest;
    }

    private static List<List<(Rectangle Rect, int SourceIndex)>> GroupRows(IReadOnlyList<Rectangle> regions)
    {
        var rows = new List<List<(Rectangle, int)>>();
        int index = 0;
        foreach (Rectangle region in regions)
        {
            List<(Rectangle, int)>? row = rows.FirstOrDefault(x =>
                Math.Abs(x.Average(v => v.Item1.Top + v.Item1.Height / 2d) - (region.Top + region.Height / 2d)) < 70);
            if (row is null) { row = []; rows.Add(row); }
            row.Add((region, index++));
        }
        foreach (List<(Rectangle, int)> row in rows) row.Sort((a, b) => a.Item1.Left.CompareTo(b.Item1.Left));
        return rows.OrderBy(x => x.Average(v => v.Item1.Top)).ToList();
    }

    private static CharacterAnimation AnimationFor(int row, int column) => row switch
    {
        0 when column < 4 => CharacterAnimation.Idle,
        0 => CharacterAnimation.Walk,
        1 => CharacterAnimation.Attack,
        2 when column < 4 => CharacterAnimation.Defend,
        2 when column < 8 => CharacterAnimation.Cast,
        2 => CharacterAnimation.Hit,
        3 => CharacterAnimation.Death,
        4 when column < 4 => CharacterAnimation.Talk,
        4 when column < 8 => CharacterAnimation.Sit,
        _ => CharacterAnimation.UseItem
    };

    private static int GroupStartFor(int row, int column) => row switch
    {
        0 when column >= 4 => 4,
        2 when column >= 8 => 8,
        2 when column >= 4 => 4,
        4 when column >= 8 => 8,
        4 when column >= 4 => 4,
        _ => 0
    };

    private static Rectangle InflateInside(Rectangle source, Size bounds, int padding)
    {
        int left = Math.Max(0, source.Left - padding);
        int top = Math.Max(0, source.Top - padding);
        int right = Math.Min(bounds.Width, source.Right + padding);
        int bottom = Math.Min(bounds.Height, source.Bottom + padding);
        return Rectangle.FromLTRB(left, top, right, bottom);
    }

    private static void Normalize(Bitmap source, Rectangle region, string outputPath)
    {
        using var target = new Bitmap(CellSize, CellSize, PixelFormat.Format32bppArgb);
        using Graphics graphics = Graphics.FromImage(target);
        graphics.Clear(Color.Transparent);
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = PixelOffsetMode.Half;
        float scale = Math.Min(220f / region.Width, 220f / region.Height);
        int width = Math.Max(1, (int)Math.Round(region.Width * scale));
        int height = Math.Max(1, (int)Math.Round(region.Height * scale));
        int x = (CellSize - width) / 2;
        int y = 230 - height;
        graphics.DrawImage(source, new Rectangle(x, y, width, height), region, GraphicsUnit.Pixel);
        target.Save(outputPath, ImageFormat.Png);
    }
}
