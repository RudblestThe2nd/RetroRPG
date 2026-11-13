using RetroRpg.Core.Common;
using RetroRpg.Core.Game;
using RetroRpg.Core.World;

namespace RetroRpg.Core.Story;

// Haritada görev işareti: hex ve günlükteki kısa hedef adı.
public sealed record StoryMarker(HexCoordinate Hex, string Label);

// Hangi olayın ne zaman oynayacağını seçer. Kurallar ekrandan bağımsızdır ve testlerde aynen kullanılır.
public static class StoryDirector
{
    public static bool UsesMainStory(GameSession session) => session.StorySlotId == StorySlotCatalog.MainStoryId;

    // Yeni oyun, kayıt yükleme veya ölüm sonrası autosave: yarım kalan düğüm ya da prolog döner.
    // Sürüm 1/2 kayıtlarında hikâye yoktur; bu durumda da prolog başlar.
    public static StoryEvent? ResumeEvent(GameSession session)
    {
        if (!UsesMainStory(session) || session.Story.IsFinished) return null;
        if (session.Story.Remembered is null) session.Story.Remember(RememberedPerson.From(session.Player.Background));

        if (session.Story.ActiveNodeId is string id && StoryCatalog.Find(id) is StoryEvent active && active.CanTrigger(session))
            return active;
        session.Story.ActiveNodeId = null;
        return Matching(session, StoryTriggerKind.Start, null, null);
    }

    // Rota adımında çağrılır. Pass olayları her geçişte, Arrive olayları yalnız rota orada bitince oynar.
    public static StoryEvent? EventOnStep(GameSession session, HexCoordinate hex, bool routeEnd)
    {
        if (!UsesMainStory(session)) return null;
        return Matching(session, StoryTriggerKind.Pass, hex, null)
               ?? (routeEnd ? Matching(session, StoryTriggerKind.Arrive, hex, null) : null)
               ?? Resumable(session, hex);
    }

    // ÇEVREYİ ARA: arama olayı veya bulunduğun hexte ertelenmiş varış olayı.
    public static StoryEvent? EventOnSearch(GameSession session, HexCoordinate hex)
    {
        if (!UsesMainStory(session)) return null;
        return Matching(session, StoryTriggerKind.Search, hex, null)
               ?? Matching(session, StoryTriggerKind.Arrive, hex, null)
               ?? Resumable(session, hex);
    }

    public static StoryEvent? EventAtInn(GameSession session, string locationId) =>
        UsesMainStory(session) ? Matching(session, StoryTriggerKind.Inn, null, locationId) : null;

    public static void Begin(GameSession session, StoryEvent storyEvent) => session.Story.ActiveNodeId = storyEvent.Id;

    // Olay biter: tek-sefer bilgisi yazılır ve sıradaki zincir olayı seçilir.
    // Sıradaki olay kayda yazılır; böylece autosave'den devam edilince aynı noktadan sürer.
    public static StoryEvent? Finish(GameSession session, StoryEvent storyEvent)
    {
        storyEvent.Complete(session);
        StoryEvent? next = storyEvent.Next.Select(StoryCatalog.Find).OfType<StoryEvent>().FirstOrDefault(x => x.CanTrigger(session));
        session.Story.ActiveNodeId = next?.Id;
        return next;
    }

    // Ertelenen olay tamamlanmaz; oyuncu daha sonra aynı yere dönebilir.
    public static void Postpone(GameSession session) => session.Story.ActiveNodeId = null;

    // Final kararı: önce Vaelor'un kaderi, sonra açık finallerden biri. Kilitli final seçilemez.
    public static StoryOutcome ChooseEnding(GameSession session, VaelorState vaelorFate, EndingStoryEvent ending)
    {
        if (!ending.CanTrigger(session)) throw new InvalidOperationException("Final kararı için Vaelor gerçek savaşta yenilmelidir.");
        if (vaelorFate is not (VaelorState.Killed or VaelorState.Persuaded or VaelorState.Imprisoned))
            throw new ArgumentOutOfRangeException(nameof(vaelorFate));
        if (vaelorFate == VaelorState.Persuaded && !EndingRules.CanPersuadeVaelor(session.Story))
            throw new InvalidOperationException("Vaelor'u ikna etmek için kurtarılan tanık veya uzlaşmacı kararlar gerekir.");
        if (!ending.IsUnlocked(session.Story)) throw new InvalidOperationException($"{ending.Title} finali kilitli.");

        session.Story.VaelorState = vaelorFate;
        StoryOutcome outcome = ending.Resolve(session, StoryChoice.Continue(), null);
        ending.Complete(session);
        session.Story.ActiveNodeId = null;
        return outcome;
    }

    // Aktif görevlerin haritadaki hedefleri. Düşman ve tehlike işareti burada yoktur.
    public static IReadOnlyList<StoryMarker> Markers(GameSession session)
    {
        if (!UsesMainStory(session)) return [];
        return session.Story.Quests.Values
            .Where(x => x.Status == QuestStatus.Active)
            .SelectMany(x => x is WaystoneQuest || x is SideQuest
                ? x.Objectives.Where(o => !x.IsObjectiveDone(o.Id))
                : x.ActiveObjective is QuestObjective active ? [active] : [])
            .Where(x => x.Site is not null)
            .GroupBy(x => x.Site!.Value)
            .Select(x => new StoryMarker(x.Key, x.First().Label))
            .ToArray();
    }

    private static StoryEvent? Matching(GameSession session, StoryTriggerKind kind, HexCoordinate? hex, string? locationId) =>
        StoryCatalog.All.FirstOrDefault(x =>
            x.Trigger.Kind == kind &&
            (hex is null || x.Trigger.Hex == hex) &&
            (locationId is null || x.Trigger.LocationId == locationId) &&
            x.CanTrigger(session));

    private static StoryEvent? Resumable(GameSession session, HexCoordinate hex) =>
        StoryCatalog.All.FirstOrDefault(x => x.Trigger.Kind == StoryTriggerKind.Chain && x.ResumeAt == hex && x.CanTrigger(session));
}
