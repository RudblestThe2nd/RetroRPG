using RetroRpg.Core.Common;

namespace RetroRpg.AtlasTool;

public sealed record IntRect(int X, int Y, int Width, int Height);
public sealed record IntPoint(int X, int Y);

public sealed record AtlasFrame(
    string Id,
    int SourceIndex,
    int SourceRow,
    CharacterAnimation Animation,
    Direction Direction,
    int AnimationFrame,
    int DurationMs,
    bool Loop,
    IntRect SourceRectangle,
    IntPoint Pivot,
    IReadOnlyDictionary<string, IntPoint> Anchors,
    string RuntimePath);

public sealed record CharacterAtlasManifest(
    string Version,
    string Race,
    string Gender,
    string SourcePath,
    int CellWidth,
    int CellHeight,
    IReadOnlyList<AtlasFrame> Frames);
