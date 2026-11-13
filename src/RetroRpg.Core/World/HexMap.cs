using RetroRpg.Core.Common;

namespace RetroRpg.Core.World;

public sealed class HexMap
{
    public static readonly HexCoordinate EyeTower = new(-3, -5);
    // Mor bölgenin tek güney kapısı ve kapıdan kuleye uzanan dar yol.
    public static readonly HexCoordinate PurpleGate = new(-5, -3);
    public static readonly IReadOnlyList<HexCoordinate> PurpleCorridor = [new(-5, -4), new(-4, -5)];

    private readonly Dictionary<HexCoordinate, HexTile> _tiles = [];
    public IReadOnlyDictionary<HexCoordinate, HexTile> Tiles => _tiles;

    public void Add(HexTile tile) => _tiles[tile.Coordinate] = tile;
    public HexTile? Get(HexCoordinate coordinate) => _tiles.GetValueOrDefault(coordinate);

    public bool CanMove(HexCoordinate from, HexCoordinate to) =>
        from.DistanceTo(to) == 1 && _tiles.TryGetValue(to, out HexTile? tile) && !tile.IsLocked;

    public void RevealAround(HexCoordinate center)
    {
        Get(center)?.Discover();
        foreach (HexCoordinate neighbor in center.Neighbors()) Get(neighbor)?.Reveal();
    }

    public void RevealAll()
    {
        foreach (HexTile tile in _tiles.Values) tile.Discover();
    }

    // Basit breadth-first search: kilitli hexlerden geçmeden en kısa yürüyüş rotasını döndürür.
    // Sonuç başlangıç ve hedefi içerir; rota yoksa boş liste döner.
    public IReadOnlyList<HexCoordinate> FindPath(HexCoordinate from, HexCoordinate to)
    {
        if (!_tiles.ContainsKey(from) || !_tiles.TryGetValue(to, out HexTile? destination) || destination.IsLocked)
            return [];
        if (from == to) return [from];

        var frontier = new Queue<HexCoordinate>();
        var previous = new Dictionary<HexCoordinate, HexCoordinate?> { [from] = null };
        frontier.Enqueue(from);

        while (frontier.Count > 0)
        {
            HexCoordinate current = frontier.Dequeue();
            foreach (HexCoordinate neighbor in current.Neighbors())
            {
                if (previous.ContainsKey(neighbor) || !_tiles.TryGetValue(neighbor, out HexTile? tile) || tile.IsLocked)
                    continue;
                previous[neighbor] = current;
                if (neighbor == to)
                {
                    var path = new List<HexCoordinate>();
                    for (HexCoordinate? step = to; step is not null; step = previous[step.Value]) path.Add(step.Value);
                    path.Reverse();
                    return path;
                }
                frontier.Enqueue(neighbor);
            }
        }

        return [];
    }

    // Merkezler tek parça dünya resmindeki bölgelerle hizalanır.
    private static readonly (HexCoordinate Center, Biome Biome)[] BiomeCenters =
    [
        (new(1, 0), Biome.Plains), (new(-1, 0), Biome.Forest), (new(-4, -1), Biome.Swamp),
        (new(0, 5), Biome.Tropical), (new(3, 3), Biome.Desert), (new(0, -5), Biome.Volcanic),
        (new(4, -3), Biome.Mountain), (new(7, -5), Biome.Snow), (new(-3, -5), Biome.Corrupted),
        (new(-7, 0), Biome.Underground)
    ];

    public static HexMap CreateDemoMap(int columns = 15, int rows = 12)
    {
        var map = new HexMap();
        var start = new HexCoordinate(0, 0);
        int halfColumns = columns / 2;
        int halfRows = rows / 2;
        for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                int r = row - halfRows;
                int offsetColumn = column - halfColumns;
                int q = offsetColumn - (int)Math.Floor(r / 2d);
                var coordinate = new HexCoordinate(q, r);
                Biome biome = BiomeCenters.MinBy(x => x.Center.DistanceTo(coordinate)).Biome;
                int distance = coordinate.DistanceTo(start);
                var tile = new HexTile(coordinate, biome, 1 + Math.Abs(q + r) % 3)
                {
                    IsDangerous = distance >= 2 && Math.Abs(q * 7 + r * 11) % 4 == 0,
                    HasQuest = q == 2 && r == -1,
                    SceneId = $"route-{biome.ToString().ToLowerInvariant()}"
                };
                map.Add(tile);
            }

        ConfigureTerrainBarriers(map);
        SealPurpleRegion(map);
        // Göz Kulesi bölgesi rastgele encounter atmaz; buraya girildiğinde savaş kesindir.
        if (map.Get(EyeTower) is HexTile eyeTower)
        {
            eyeTower.IsLocked = false;
            eyeTower.IsDangerous = true;
            eyeTower.IsForcedCombat = true;
            eyeTower.HasQuest = true;
            eyeTower.SceneId = "eye-tower";
        }
        map.RevealAll();
        return map;
    }

    private static void ConfigureTerrainBarriers(HexMap map)
    {
        // Harita geçiş editöründe kullanıcının kaydettiği ilk paket yalnız koyu mor biyomu kapsar.
        // Diğer biyomların kalıcı engelleri kullanıcı onları seçtikçe bu listeye eklenecektir.
        HexCoordinate[] permanentObstacles =
        [
            new(2, -6), new(1, -5),
            new(-4, -4), new(-3, -4), new(-2, -4), new(0, -4),
            new(-3, -3), new(-2, -3)
        ];

        foreach (HexCoordinate coordinate in permanentObstacles)
            if (map.Get(coordinate) is HexTile tile) MarkPermanentObstacle(tile);

        SetUnlockableGate(map, new HexCoordinate(3, -2),
            "Çığla kapanmış kısa dağ geçidi; Dağ Keşişi veya halat köprü yolu açabilir.");
        SetUnlockableGate(map, new HexCoordinate(-6, 0),
            "Çökmüş yeraltı geçidi; maden asansörü onarılmalı.");
    }

    // Editör engelleri dışında kalan bütün mor hexler hikâye tarafından mühürlenir.
    // Kule hexinin kendisi açık kalır; fakat çevresi mühürlü olduğu için kapı açılmadan ulaşılamaz.
    private static void SealPurpleRegion(HexMap map)
    {
        foreach (HexTile tile in map.Tiles.Values.Where(x =>
                     x.Biome == Biome.Corrupted && !x.IsPermanentObstacle && x.Coordinate != EyeTower))
        {
            tile.IsLocked = true;
            tile.IsSealed = true;
            tile.IsDangerous = false;
            tile.IsForcedCombat = false;
            tile.LockReason = "Mor sis ve kader yarığı bu sınırı mühürlüyor; kuleye tek yol güney kapısıdır.";
        }

        if (map.Get(PurpleGate) is HexTile gate)
        {
            gate.IsSealed = false;
            gate.LockReason = "Üç Yol Taşı etkinleşmeden kuleye giden kader yolu kapalı.";
        }
    }

    // Üç Yol Taşı tamamlanınca yalnız güney kapısı ve kuleye giden dar yol açılır.
    // Diğer mor sınırlar mühürlü kalır. Sabit hikâye olayları rastgele savaşın yerini alır.
    public void OpenPurpleGate()
    {
        foreach (HexCoordinate coordinate in PurpleCorridor.Prepend(PurpleGate))
        {
            if (Get(coordinate) is not HexTile tile) continue;
            tile.IsLocked = false;
            tile.IsSealed = false;
            tile.IsDangerous = false;
            tile.LockReason = null;
        }
        if (Get(EyeTower) is HexTile tower) tower.IsDangerous = false;
    }

    private static void MarkPermanentObstacle(HexTile tile)
    {
        tile.IsLocked = true;
        tile.IsPermanentObstacle = true;
        tile.IsDangerous = false;
        tile.IsForcedCombat = false;
        tile.HasQuest = false;
        tile.LockReason = tile.Biome switch
        {
            Biome.Corrupted => "Karanlık yarık ve sivri kayalar geçişe izin vermiyor; mor bölgenin kapısını bulmalısın.",
            Biome.Mountain => "Sarp kayalık duvar tırmanılamıyor; doğal dağ geçidine yönelmelisin.",
            Biome.Volcanic => "Taze lav gölü geçilemez; soğumuş taşların çevresinden dolaşmalısın.",
            Biome.Swamp => "Dipsiz bataklık yürüyüşe kapalı; sağlam kök ve taşları izlemelisin.",
            Biome.Desert => "Derin kanyon geçişi kesiyor; mesa kümelerinin arasındaki yolu bulmalısın.",
            Biome.Snow => "Kırılmış buz uçurumu geçilemez; güvenli buzul yoluna dönmelisin.",
            Biome.Underground => "Çökmüş mağara ağzı kapalı; başka bir giriş veya onarım gerekiyor.",
            _ => "Bu arazi yürüyerek geçilemez; çevresinden dolaşmalısın."
        };
    }

    private static void SetUnlockableGate(HexMap map, HexCoordinate coordinate, string reason)
    {
        if (map.Get(coordinate) is not HexTile gate) return;
        gate.IsLocked = true;
        gate.IsPermanentObstacle = false;
        gate.IsDangerous = false;
        gate.IsForcedCombat = false;
        gate.LockReason = reason;
    }
}
