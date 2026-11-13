using System.Text.Json;
using RetroRpg.Core.Common;

namespace RetroRpg.Core.Rendering;

public sealed record EquipmentAnchorProfile(
    IReadOnlyDictionary<Direction, RuntimePoint> Hands,
    RuntimePoint Head,
    RuntimePoint Torso,
    RuntimePoint Feet,
    RuntimePoint Back);

public sealed class EquipmentAnchorCatalog
{
    private readonly Dictionary<string, EquipmentAnchorProfile> _profiles = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, EquipmentAnchorProfile> Profiles => _profiles;

    public static EquipmentAnchorCatalog Load(string manifestPath)
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        JsonElement profiles = document.RootElement.GetProperty("anchor_profiles");
        var catalog = new EquipmentAnchorCatalog();
        foreach (JsonProperty profile in profiles.EnumerateObject())
        {
            JsonElement value = profile.Value;
            var hands = new Dictionary<Direction, RuntimePoint>();
            foreach (JsonProperty hand in value.GetProperty("hand").EnumerateObject())
                hands[Enum.Parse<Direction>(hand.Name, true)] = Point(hand.Value);
            catalog._profiles[profile.Name] = new EquipmentAnchorProfile(
                hands, Point(value.GetProperty("head")), Point(value.GetProperty("torso")),
                Point(value.GetProperty("feet")), Point(value.GetProperty("back")));
        }
        return catalog;
    }

    public EquipmentAnchorProfile For(string raceId, Gender gender)
    {
        string key = $"{raceId}-{gender.ToString().ToLowerInvariant()}";
        return _profiles.TryGetValue(key, out EquipmentAnchorProfile? profile)
            ? profile : throw new KeyNotFoundException($"Ekipman ankraj profili bulunamadı: {key}");
    }

    private static RuntimePoint Point(JsonElement array) => new(array[0].GetInt32(), array[1].GetInt32());
}
