using RetroRpg.Core.Characters;
using RetroRpg.Core.Items;
using RetroRpg.Core.Persistence;
using RetroRpg.Core.Story;
using RetroRpg.Core.World;

namespace RetroRpg.Core.Game;

public sealed class GameSession(PlayerCharacter player)
{
    public PlayerCharacter Player { get; } = player;
    public HexMap Map { get; } = HexMap.CreateDemoMap();
    public HexCoordinate CurrentHex { get; private set; } = new(0, 0);
    public Dictionary<string, string> Flags { get; } = [];
    public string StorySlotId { get; set; } = "story-slot-1";
    public StoryProgress Story { get; private set; } = new();

    // Hikâye geçişleri oyuncuyu yürütmeden taşır: prolog sonrası Yeşilgeçit, düşüş sonrası kulenin dibi.
    public void PlaceAt(HexCoordinate coordinate)
    {
        if (Map.Get(coordinate) is null) return;
        CurrentHex = coordinate;
        Map.RevealAround(CurrentHex);
    }

    public bool TryMove(HexCoordinate target)
    {
        if (!Map.CanMove(CurrentHex, target)) return false;
        CurrentHex = target;
        Map.RevealAround(CurrentHex);
        return true;
    }

    // Karakter oluşturmada seçilen yüz varyantı. Kayıt biçimi değişmesin diye bayraklarda tutulur.
    public int PortraitIndex
    {
        get => Flags.TryGetValue("portrait", out string? value) && int.TryParse(value, out int index) ? index : 1;
        set => Flags["portrait"] = value.ToString();
    }

    // Kazanılan savaştan sonra hex güvenli olur; kayıttan yüklenince de güvenli kalır.
    public void ClearDanger(HexCoordinate coordinate)
    {
        if (Map.Get(coordinate) is not HexTile tile) return;
        tile.IsDangerous = false;
        Flags[$"cleared:{coordinate.Q},{coordinate.R}"] = "1";
    }

    public void ApplyUnlock(string flag)
    {
        Flags[flag] = "1";
        if (flag == "unlock:purple-gate")
        {
            Map.OpenPurpleGate();
            return;
        }
        HexCoordinate? coordinate = flag switch
        {
            "unlock:mountain-pass" => new HexCoordinate(3, -2),
            "unlock:cave-route" => new HexCoordinate(-6, 0),
            _ => null
        };
        if (coordinate is HexCoordinate hex && Map.Get(hex) is HexTile tile)
        {
            tile.IsLocked = false;
            tile.LockReason = null;
        }
    }

    public const int SaveVersion = 3;

    public GameSaveData ToSaveData() => new(
        SaveVersion,
        new CharacterSaveData(Player.Name, Player.Gender, Player.Race.Id, Player.Class.Id, Player.Class.SubclassId,
            Player.Background.Id, Player.Stats.Strength, Player.Stats.Dexterity, Player.Stats.Constitution,
            Player.Stats.Intelligence, Player.Stats.Wisdom, Player.Stats.Charisma, Player.Health, Player.Gold,
            ItemSaveMapper.ToSaveData(Player.Inventory, Player.Equipment)),
        CurrentHex.Q,
        CurrentHex.R,
        Map.Tiles.Values.Where(x => x.Discovery == Common.DiscoveryState.Discovered)
            .Select(x => $"{x.Coordinate.Q},{x.Coordinate.R}").ToArray(),
        Flags,
        StorySlotId,
        DateTimeOffset.UtcNow,
        Story.ToSaveData());

    public static GameSession FromSaveData(GameSaveData save)
    {
        CharacterSaveData data = save.Character;
        var stats = new CharacterStats
        {
            Strength = data.Strength,
            Dexterity = data.Dexterity,
            Constitution = data.Constitution,
            Intelligence = data.Intelligence,
            Wisdom = data.Wisdom,
            Charisma = data.Charisma
        };
        var player = new PlayerCharacter(data.Name, data.Gender, RaceCatalog.Find(data.RaceId),
            ClassCatalog.FindSubclass(data.SubclassId),
            BackgroundCatalog.Find(data.BackgroundId), stats, applyRaceBonuses: false);
        player.RestoreState(data.Health, data.Gold);
        if (data.Items is null) ItemCatalog.GiveStartingKit(player, save.StorySlotId);
        else ItemSaveMapper.Restore(data.Items, player.Inventory, player.Equipment);
        var session = new GameSession(player)
        {
            CurrentHex = new HexCoordinate(save.CurrentQ, save.CurrentR),
            StorySlotId = save.StorySlotId,
            // Sürüm 1/2 kayıtlarında Story yoktur; güvenli başlangıç değerleri kurulur.
            Story = StoryProgress.FromSaveData(save.Story)
        };
        foreach (KeyValuePair<string, string> flag in save.Flags)
        {
            session.Flags[flag.Key] = flag.Value;
            if (flag.Key.StartsWith("cleared:") && TryParseHex(flag.Key["cleared:".Length..], out HexCoordinate cleared))
                session.ClearDanger(cleared);
            else if (flag.Key.StartsWith("unlock:", StringComparison.OrdinalIgnoreCase))
                session.ApplyUnlock(flag.Key);
        }
        foreach (string coordinate in save.DiscoveredHexes)
        {
            if (!TryParseHex(coordinate, out HexCoordinate hex)) continue;
            session.Map.Get(hex)?.Discover();
            foreach (HexCoordinate neighbor in hex.Neighbors()) session.Map.Get(neighbor)?.Reveal();
        }
        // Eski bir kayıt artık mühürlü bir mor hexte kalmışsa oyuncu güvenle Yeşilgeçit'e alınır.
        if (session.Map.Get(session.CurrentHex) is not { IsLocked: false }) session.CurrentHex = new HexCoordinate(0, 0);
        session.Map.RevealAround(session.CurrentHex);
        return session;
    }

    private static bool TryParseHex(string text, out HexCoordinate coordinate)
    {
        coordinate = default;
        string[] parts = text.Split(',');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int q) || !int.TryParse(parts[1], out int r)) return false;
        coordinate = new HexCoordinate(q, r);
        return true;
    }
}
