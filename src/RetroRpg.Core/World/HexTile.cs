using RetroRpg.Core.Common;

namespace RetroRpg.Core.World;

public sealed class HexTile(HexCoordinate coordinate, Biome biome, int movementCost = 1)
{
    public HexCoordinate Coordinate { get; } = coordinate;
    public Biome Biome { get; } = biome;
    public int MovementCost { get; } = Math.Max(1, movementCost);
    public DiscoveryState Discovery { get; private set; } = DiscoveryState.Undiscovered;
    public bool IsLocked { get; set; }
    public bool IsPermanentObstacle { get; set; }
    // Mor bölgenin hikâye sınırı. Editör engeli değildir; haritada geçit işareti de almaz.
    public bool IsSealed { get; set; }
    public string? LockReason { get; set; }
    public bool IsDangerous { get; set; }
    public bool IsForcedCombat { get; set; }
    public bool HasQuest { get; set; }
    public string? SceneId { get; set; }

    // Daha önce keşfedilmiş hex yeniden sise düşmez.
    public void Reveal()
    {
        if (Discovery == DiscoveryState.Undiscovered) Discovery = DiscoveryState.Fogged;
    }
    public void Discover() => Discovery = DiscoveryState.Discovered;
}
