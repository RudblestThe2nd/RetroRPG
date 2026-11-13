using System.Text.Json;
using RetroRpg.Core.Assets;

namespace RetroRpg.Core.Persistence;

public sealed class SaveGameService(string saveDirectory)
{
    public string SaveDirectory { get; } = Path.GetFullPath(saveDirectory);

    public string Save(GameSaveData data, string slot = "slot-1")
    {
        Directory.CreateDirectory(SaveDirectory);
        string path = Path.Combine(SaveDirectory, SafeSlot(slot) + ".json");
        PreserveOlderVersion(path, SafeSlot(slot), data.Version);
        File.WriteAllText(path, JsonSerializer.Serialize(data, JsonOptions.Default));
        return path;
    }

    // Eski sürüm kayıt dosyası üzerine yazılmadan önce bir kez yedeklenir; eski dosya hiç değiştirilmez.
    private void PreserveOlderVersion(string path, string slot, int newVersion)
    {
        if (!File.Exists(path)) return;
        int oldVersion;
        try
        {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
            oldVersion = document.RootElement.TryGetProperty("Version", out JsonElement version) ? version.GetInt32() : 1;
        }
        catch (JsonException) { return; }
        string backup = Path.Combine(SaveDirectory, $"{slot}.v{oldVersion}-backup.json");
        if (oldVersion < newVersion && !File.Exists(backup)) File.Copy(path, backup);
    }

    public GameSaveData Load(string slot = "slot-1")
    {
        string path = Path.Combine(SaveDirectory, SafeSlot(slot) + ".json");
        if (!File.Exists(path)) throw new FileNotFoundException("Kayıt bulunamadı.", path);
        return JsonSerializer.Deserialize<GameSaveData>(File.ReadAllText(path), JsonOptions.Default)
            ?? throw new InvalidDataException("Kayıt dosyası okunamadı.");
    }

    private static string SafeSlot(string slot) => string.Concat(slot.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_'));
}
