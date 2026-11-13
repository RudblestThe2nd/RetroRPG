using RetroRpg.Core.Common;
using RetroRpg.Core.Story;

namespace RetroRpg.Core.Persistence;

// Amount türe göre anlam kazanır: silahta hasar zarı, zırhta AC bonusu, iksirde iyileştirme,
// Kaderbozan Zırhı'nda kalan son direniş hakkı.
public sealed record ItemSaveData(
    string Kind, string Name, int Value, double Weight,
    string? AssetId = null, int Amount = 0, EquipmentSlot? Slot = null, string? QuestKey = null, bool Equipped = false);

// Items, sürüm 1 kayıtlarında yoktur ve null okunur.
public sealed record CharacterSaveData(
    string Name, Gender Gender, string RaceId, string ClassId, string SubclassId, string BackgroundId,
    int Strength, int Dexterity, int Constitution, int Intelligence, int Wisdom, int Charisma,
    int Health, int Gold,
    IReadOnlyList<ItemSaveData>? Items = null);

// Story, sürüm 1 ve 2 kayıtlarında yoktur ve null okunur; oyun güvenli başlangıç değerlerini kurar.
public sealed record GameSaveData(
    int Version,
    CharacterSaveData Character,
    int CurrentQ,
    int CurrentR,
    IReadOnlyList<string> DiscoveredHexes,
    IReadOnlyDictionary<string, string> Flags,
    string StorySlotId,
    DateTimeOffset SavedAt,
    StoryProgressSaveData? Story = null);

public sealed record QuestSaveData(
    string Id, QuestStatus Status, IReadOnlyList<string>? CompletedObjectives, string? Note, bool UsedAlternative);

// Sürüm 3 hikâye kaydı. Açık alanlar kayıt dosyasını okunur tutar; Flags küçük hikâye ayrıntılarını taşır.
public sealed record StoryProgressSaveData(
    int Chapter,
    string? ActiveNodeId,
    IReadOnlyList<QuestSaveData>? Quests,
    IReadOnlyList<string>? CompletedEvents,
    IReadOnlyDictionary<string, string>? Flags,
    IReadOnlyDictionary<string, int>? Trust,
    IReadOnlyDictionary<string, string>? WaystoneModes,
    bool PurpleGateOpen,
    string? RememberedType,
    string? RememberedName,
    string? RememberedRelation,
    bool ScriptedDefeatPlayed,
    bool FatebreakerGranted,
    bool PrisonerRescued,
    bool MemoryAnchored,
    VaelorState VaelorState,
    IReadOnlyList<string>? UnlockedEndings,
    string? ChosenEnding);
