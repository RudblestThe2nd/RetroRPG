using RetroRpg.Core.Common;
using RetroRpg.Core.Persistence;

namespace RetroRpg.Core.Story;

public enum VaelorState { Unmet, FirstVictory, Defeated, Killed, Persuaded, Imprisoned }

// Hikâye bayraklarının ortak anahtarları. Plan belgesindeki adlarla aynıdır.
public static class StoryFlags
{
    public const string RememberedType = "story:remembered-type";
    public const string RememberedName = "story:remembered-name";
    public const string RememberedRelation = "story:remembered-relation";
    public const string PurpleGateOpen = "route:purple-gate";
    public const string ScriptedDefeat = "vaelor:scripted-defeat";
    public const string Fatebreaker = "item:fatebreaker";
    public const string PrisonerRescued = "prisoner:rescued";
    public const string MemoryAnchored = "memory:anchored";
    public const string VaelorState = "vaelor:state";

    public static string WaystoneMode(string stoneId) => $"waystone:{stoneId}:mode";
}

// Ana hikâyenin bütün ilerlemesi. Kalıcı durumun tek kaynağı budur ve kayıt sürüm 3'e yazılır.
public sealed class StoryProgress
{
    private readonly Dictionary<string, Quest> _quests;
    private readonly HashSet<string> _completedEvents = [];

    public StoryProgress()
    {
        _quests = StoryCatalog.CreateQuests().ToDictionary(x => x.Id);
    }

    // 0 prolog, 1 Silinen Kervan, 2 Üç Yol Taşı, 3 mor bölge, 4 Göz Kulesi, 5 final.
    // Görev durumlarından hesaplanır; böylece bölüm bilgisi görevlerle hiçbir zaman çelişmez.
    public int Chapter =>
        !PrologueCompleted ? 0 :
        _quests[StoryCatalog.MainOne].Status != QuestStatus.Completed ? 1 :
        _quests[StoryCatalog.MainTwo].Status != QuestStatus.Completed ? 2 :
        _quests[StoryCatalog.MainThree].Status != QuestStatus.Completed ? 3 :
        _quests[StoryCatalog.ReturnQuest].Status != QuestStatus.Completed ? 4 : 5;

    // Şu anda oynanan veya kayıttan devam edilecek hikâye düğümü.
    public string? ActiveNodeId { get; set; }
    public IReadOnlyDictionary<string, Quest> Quests => _quests;
    public IReadOnlyCollection<string> CompletedEvents => _completedEvents;
    public Dictionary<string, string> Flags { get; } = [];
    public Dictionary<string, int> Trust { get; } = [];
    public List<string> UnlockedEndings { get; } = [];

    // Bayrakta tutulur; böylece FlagConsequence da Vaelor'un durumunu yazabilir.
    public VaelorState VaelorState
    {
        get => Enum.TryParse(Flags.GetValueOrDefault(StoryFlags.VaelorState), out VaelorState state) ? state : VaelorState.Unmet;
        set => Flags[StoryFlags.VaelorState] = value.ToString();
    }
    public string? ChosenEnding { get; set; }

    public bool PrologueCompleted => HasCompleted(StoryCatalog.PrologueId);
    public bool IsFinished => ChosenEnding is not null;
    public bool PurpleGateOpen => HasFlag(StoryFlags.PurpleGateOpen);
    public bool ScriptedDefeatPlayed => HasFlag(StoryFlags.ScriptedDefeat);
    public bool FatebreakerGranted => HasFlag(StoryFlags.Fatebreaker);
    public bool PrisonerRescued => HasFlag(StoryFlags.PrisonerRescued);
    public bool MemoryAnchored => HasFlag(StoryFlags.MemoryAnchored);

    public RememberedPerson? Remembered =>
        Flags.TryGetValue(StoryFlags.RememberedType, out string? type)
            ? new RememberedPerson(type, Flags.GetValueOrDefault(StoryFlags.RememberedName, ""),
                Flags.GetValueOrDefault(StoryFlags.RememberedRelation, ""))
            : null;

    public int CompletedWaystones => _quests.Values.OfType<WaystoneQuest>().Count(x => x.Status == QuestStatus.Completed);

    public Quest Quest(string id) => _quests[id];
    public bool IsActive(string questId) => _quests[questId].Status == QuestStatus.Active;
    public bool HasCompleted(string eventId) => _completedEvents.Contains(eventId);
    public void MarkCompleted(string eventId) => _completedEvents.Add(eventId);
    public bool HasFlag(string key) => Flags.ContainsKey(key);
    public void SetFlag(string key, string value = "1") => Flags[key] = value;
    public string? WaystoneMode(string stoneId) => Flags.GetValueOrDefault(StoryFlags.WaystoneMode(stoneId));
    public int TrustIn(string region) => Trust.GetValueOrDefault(region);
    public void AddTrust(string region, int amount) => Trust[region] = TrustIn(region) + amount;

    public void Remember(RememberedPerson person)
    {
        SetFlag(StoryFlags.RememberedType, person.Type);
        SetFlag(StoryFlags.RememberedName, person.Name);
        SetFlag(StoryFlags.RememberedRelation, person.Relation);
    }

    public StoryProgressSaveData ToSaveData() => new(
        Chapter,
        ActiveNodeId,
        _quests.Values.Select(x => new QuestSaveData(x.Id, x.Status, x.CompletedObjectives.ToArray(), x.Note, x.UsedAlternative)).ToArray(),
        _completedEvents.ToArray(),
        new Dictionary<string, string>(Flags),
        new Dictionary<string, int>(Trust),
        new[] { "root", "sky", "sun" }.Where(x => WaystoneMode(x) is not null).ToDictionary(x => x, x => WaystoneMode(x)!),
        PurpleGateOpen,
        Remembered?.Type,
        Remembered?.Name,
        Remembered?.Relation,
        ScriptedDefeatPlayed,
        FatebreakerGranted,
        PrisonerRescued,
        MemoryAnchored,
        VaelorState,
        UnlockedEndings.ToArray(),
        ChosenEnding);

    // Eksik alanlar güvenli başlangıç değerleriyle kalır; açık alanlar bayraklardan önceliklidir.
    public static StoryProgress FromSaveData(StoryProgressSaveData? data)
    {
        var story = new StoryProgress();
        if (data is null) return story;

        // Chapter kayda okunurluk için yazılır; yüklemede görev durumlarından yeniden hesaplanır.
        story.ActiveNodeId = data.ActiveNodeId;
        foreach (KeyValuePair<string, string> flag in data.Flags ?? new Dictionary<string, string>()) story.Flags[flag.Key] = flag.Value;
        foreach (KeyValuePair<string, int> trust in data.Trust ?? new Dictionary<string, int>()) story.Trust[trust.Key] = trust.Value;
        foreach (KeyValuePair<string, string> mode in data.WaystoneModes ?? new Dictionary<string, string>())
            story.SetFlag(StoryFlags.WaystoneMode(mode.Key), mode.Value);
        foreach (string id in data.CompletedEvents ?? []) story._completedEvents.Add(id);
        foreach (QuestSaveData quest in data.Quests ?? [])
            if (story._quests.TryGetValue(quest.Id, out Quest? target))
                target.Restore(quest.Status, quest.CompletedObjectives ?? [], quest.Note, quest.UsedAlternative);

        if (data.RememberedType is not null)
            story.Remember(new RememberedPerson(data.RememberedType, data.RememberedName ?? "", data.RememberedRelation ?? ""));
        if (data.PurpleGateOpen) story.SetFlag(StoryFlags.PurpleGateOpen);
        if (data.ScriptedDefeatPlayed) story.SetFlag(StoryFlags.ScriptedDefeat);
        if (data.FatebreakerGranted) story.SetFlag(StoryFlags.Fatebreaker);
        if (data.PrisonerRescued) story.SetFlag(StoryFlags.PrisonerRescued);
        if (data.MemoryAnchored) story.SetFlag(StoryFlags.MemoryAnchored);
        story.VaelorState = data.VaelorState;
        story.UnlockedEndings.AddRange(data.UnlockedEndings ?? []);
        story.ChosenEnding = data.ChosenEnding;
        return story;
    }
}
