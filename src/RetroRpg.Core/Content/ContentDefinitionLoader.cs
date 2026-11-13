using System.Text.Json;
using RetroRpg.Core.Assets;

namespace RetroRpg.Core.Content;

public sealed record IdNameDefinition(string Id, string Name);
public sealed record SubclassDefinition(string Id, string Name, string ParentClassId);
public sealed record ItemDefinition(string Id, string Name, string Type, int Value, int DamageDie);
public sealed record ContentDefinitionDocument(
    string Version,
    IReadOnlyList<IdNameDefinition> Races,
    IReadOnlyList<IdNameDefinition> Classes,
    IReadOnlyList<SubclassDefinition> Subclasses,
    IReadOnlyList<IdNameDefinition> Backgrounds,
    IReadOnlyList<IdNameDefinition> Biomes,
    IReadOnlyList<ItemDefinition> Items);

public sealed class ContentDefinitionLoader
{
    public ContentDefinitionDocument Load(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("İçerik tanımı bulunamadı.", path);
        return JsonSerializer.Deserialize<ContentDefinitionDocument>(File.ReadAllText(path), JsonOptions.Default)
            ?? throw new InvalidDataException("İçerik tanımı okunamadı.");
    }
}
