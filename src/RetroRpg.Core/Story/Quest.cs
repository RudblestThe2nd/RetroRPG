using RetroRpg.Core.Common;
using RetroRpg.Core.World;

namespace RetroRpg.Core.Story;

// Label günlükteki kısa satırdır; Text parşömende tam hedef olarak görünür.
// Site doluysa hedef haritada görev işareti alır.
public sealed record QuestObjective(string Id, string Label, string Text, HexCoordinate? Site = null);

// Bütün görevlerin tabanı: kimlik, başlık, açıklama, hedefler ve durum.
public abstract class Quest
{
    private readonly HashSet<string> _completedObjectives = [];

    protected Quest(string id, string title, string description, IReadOnlyList<QuestObjective> objectives)
    {
        Id = id;
        Title = title;
        Description = description;
        Objectives = objectives;
    }

    public string Id { get; }
    public string Title { get; }
    public string Description { get; }
    public IReadOnlyList<QuestObjective> Objectives { get; }
    public QuestStatus Status { get; private set; } = QuestStatus.NotStarted;
    public string? Note { get; protected set; }
    public IReadOnlyCollection<string> CompletedObjectives => _completedObjectives;
    public abstract string KindName { get; }

    public string StatusName => Status switch
    {
        QuestStatus.Active => "AKTİF",
        QuestStatus.Completed => "TAMAMLANDI",
        QuestStatus.Failed => "BAŞARISIZ",
        _ => "BAŞLAMADI"
    };

    public virtual QuestObjective? ActiveObjective =>
        Status == QuestStatus.Active ? Objectives.FirstOrDefault(x => !_completedObjectives.Contains(x.Id)) : null;

    public bool IsObjectiveDone(string objectiveId) => _completedObjectives.Contains(objectiveId);

    public void Start()
    {
        if (Status == QuestStatus.NotStarted) Status = QuestStatus.Active;
    }

    public void CompleteObjective(string objectiveId)
    {
        Start();
        _completedObjectives.Add(objectiveId);
    }

    public void Complete(string? note = null)
    {
        Status = QuestStatus.Completed;
        Note = note ?? Note;
        foreach (QuestObjective objective in Objectives) _completedObjectives.Add(objective.Id);
    }

    public virtual void Fail(string note)
    {
        Status = QuestStatus.Failed;
        Note = note;
    }

    // Kayıttan geri yükleme; kurallardan geçmeden durumu aynen kurar.
    internal virtual void Restore(QuestStatus status, IEnumerable<string> completedObjectives, string? note, bool usedAlternative)
    {
        Status = status;
        Note = note;
        _completedObjectives.Clear();
        foreach (string id in completedObjectives) _completedObjectives.Add(id);
    }

    internal virtual bool UsedAlternative => false;
}

// Ana görev başarısız olunca kapanmaz: alternatif hedef açılır ve hikâye kilitlenmez.
public sealed class MainQuest(string id, string title, string description, QuestObjective? alternative,
    params QuestObjective[] objectives) : Quest(id, title, description, objectives)
{
    private bool _usingAlternative;

    public QuestObjective? Alternative { get; } = alternative;
    public bool UsingAlternative => _usingAlternative;
    public override string KindName => "ANA GÖREV";

    public override QuestObjective? ActiveObjective =>
        _usingAlternative && Status == QuestStatus.Active && Alternative is not null && !IsObjectiveDone(Alternative.Id)
            ? Alternative
            : base.ActiveObjective;

    public override void Fail(string note)
    {
        if (Alternative is null || _usingAlternative)
        {
            base.Fail(note);
            return;
        }
        Start();
        _usingAlternative = true;
        Note = note;
    }

    internal override void Restore(QuestStatus status, IEnumerable<string> completedObjectives, string? note, bool usedAlternative)
    {
        base.Restore(status, completedObjectives, note, usedAlternative);
        _usingAlternative = usedAlternative && Alternative is not null;
    }

    internal override bool UsedAlternative => _usingAlternative;
}

// Üç Yol Taşı görevi; serbest sırayla tamamlanır.
public sealed class WaystoneQuest(string id, string stoneId, string title, string description, QuestObjective objective)
    : Quest(id, title, description, [objective])
{
    public string StoneId { get; } = stoneId;
    public override string KindName => "YOL TAŞI";
}

// Ana hikâyeyi kilitlemeyen yan görev; başarısız kalabilir.
public sealed class SideQuest(string id, string title, string description, params QuestObjective[] objectives)
    : Quest(id, title, description, objectives)
{
    public override string KindName => "YAN GÖREV";
}
