using System.Text.Json;
using System.Text.Json.Serialization;
using RetroRpg.Core.Assets;
using RetroRpg.Core.Common;
using RetroRpg.Core.Items;

namespace RetroRpg.Core.Characters;

public sealed record EnemyDefinition(string Id, Biome Biome, string Name, string Tier, string? ClassId = null, int SpriteVersion = 1);
public sealed record EnemyDefinitionDocument(string Version, string SpriteRoot, string TokenRoot, IReadOnlyList<EnemyDefinition> Enemies);

// Content/definitions/enemies-v1.json içindeki 120 biyom düşmanını okur ve savaşa hazır EnemyNpc üretir.
public sealed class EnemyCatalog
{
    private readonly EnemyDefinitionDocument _document;

    private EnemyCatalog(EnemyDefinitionDocument document) => _document = document;

    public IReadOnlyList<EnemyDefinition> Enemies => _document.Enemies;

    public static EnemyCatalog Load(string path)
    {
        var options = new JsonSerializerOptions(JsonOptions.Default) { Converters = { new JsonStringEnumConverter() } };
        EnemyDefinitionDocument document = JsonSerializer.Deserialize<EnemyDefinitionDocument>(File.ReadAllText(path), options)
            ?? throw new InvalidDataException($"Düşman tanımları okunamadı: {path}");
        return new EnemyCatalog(document);
    }

    // Merkezden uzaklaştıkça daha güçlü düşman çıkar. Aynı hex her zaman aynı düşmanı verir.
    public EnemyDefinition PickEncounter(Biome biome, int distanceFromStart, int seed)
    {
        string[] tiers = distanceFromStart switch
        {
            <= 2 => ["common"],
            3 => ["common", "uncommon"],
            _ => ["uncommon", "elite"]
        };
        EnemyDefinition[] pool = _document.Enemies.Where(x => x.Biome == biome && tiers.Contains(x.Tier)).ToArray();
        if (pool.Length == 0) throw new KeyNotFoundException($"{biome} biyomu için düşman tanımı yok.");
        return pool[Math.Abs(seed) % pool.Length];
    }

    public string SpritePath(EnemyDefinition enemy, string pose)
    {
        string biome = enemy.Biome.ToString().ToLowerInvariant();
        return $"{_document.SpriteRoot}/{biome}/sprites/{enemy.Id}/{biome}-{enemy.Id}-{pose}-v{enemy.SpriteVersion}.png";
    }

    public string TokenPath(EnemyDefinition enemy)
    {
        string biome = enemy.Biome.ToString().ToLowerInvariant();
        return $"{_document.TokenRoot}/{biome}/enemy-{biome}-{enemy.Id}-token-v1.png";
    }

    public static BiomeEnemyNpc Create(EnemyDefinition enemy)
    {
        (int health, int armorClass, int gold, int damageDie) = enemy.Tier switch
        {
            "uncommon" => (14, 11, 12, 6),
            "elite" => (20, 12, 20, 8),
            "rare" => (26, 13, 30, 10),
            _ => (10, 10, 8, 6)
        };
        CharacterClass characterClass = ClassCatalog.Find(enemy.ClassId ?? "fighter");
        string visualAssetId = $"enemy.{enemy.Biome.ToString().ToLowerInvariant()}.{enemy.Id}";
        return new BiomeEnemyNpc(enemy.Name, visualAssetId, health, armorClass, characterClass, gold, enemy.Biome)
        {
            Weapon = new Weapon("natural-attack", "Doğal Saldırı", 0, 0, damageDie)
        };
    }
}
