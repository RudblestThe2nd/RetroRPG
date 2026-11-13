namespace RetroRpg.Core.Story;

public sealed record StoryDefinition(string Id, string Title, string Description, bool IsLocked);

// Hikâye seçim ekranının üç yuvası. İlk sürümde yalnız ana hikâye açıktır.
public static class StorySlotCatalog
{
    public const string MainStoryId = "story-slot-1";

    public static IReadOnlyList<StoryDefinition> Slots { get; } =
    [
        new(MainStoryId, "Kader Yolları: Göz Kulesi", "Haritadan silinen yolların ve Göz Kulesi'nin hikâyesi.", false),
        new("story-slot-2", "Hikâye II", "Henüz hazır değil.", true),
        new("story-slot-3", "Hikâye III", "Henüz hazır değil.", true)
    ];
}
