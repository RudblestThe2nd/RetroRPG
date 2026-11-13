using RetroRpg.Core.Characters;
using RetroRpg.Core.Combat;
using RetroRpg.Core.Common;
using RetroRpg.Core.Game;
using RetroRpg.Core.Items;
using RetroRpg.Core.World;

namespace RetroRpg.Core.Story;

// Start: yeni oyun / prolog. Arrive: rota o hexte biterse. Pass: hexten geçerken bile.
// Search: ÇEVREYİ ARA. Inn: yerleşimin hanı. Chain: önceki olayın hemen ardından.
public enum StoryTriggerKind { Start, Arrive, Pass, Search, Inn, Chain }

public sealed record StoryTrigger(StoryTriggerKind Kind, HexCoordinate? Hex = null, string? LocationId = null)
{
    public static StoryTrigger Start { get; } = new(StoryTriggerKind.Start);
    public static StoryTrigger Chain { get; } = new(StoryTriggerKind.Chain);
    public static StoryTrigger Arrive(HexCoordinate hex) => new(StoryTriggerKind.Arrive, hex);
    public static StoryTrigger Pass(HexCoordinate hex) => new(StoryTriggerKind.Pass, hex);
    public static StoryTrigger Search(HexCoordinate hex) => new(StoryTriggerKind.Search, hex);
    public static StoryTrigger Inn(string locationId) => new(StoryTriggerKind.Inn, LocationId: locationId);
}

// Sahne görseli: manifestteki bir sinematik dizinin kareleri ya da tek bir sabit sahne görseli.
// ChoiceFrame seçimin göründüğü karedir; sonraki kareler sonuçtan sonra oynar.
// FrameMeaning doluysa kare manifestin frameMeanings alanından bulunur.
public sealed record StoryScene(string? SequenceId, int FirstFrame, int FrameCount, int ChoiceFrame, string? ImagePath,
    string? FrameMeaning = null)
{
    public static StoryScene Sequence(string id, int firstFrame = 0, int frameCount = 4, int? choiceFrame = null) =>
        new(id, firstFrame, frameCount, choiceFrame ?? frameCount - 1, null);

    public static StoryScene Meaning(string id, string frameMeaning) => new(id, 0, 1, 0, null, frameMeaning);

    public static StoryScene Image(string path) => new(null, 0, 1, 0, path);
}

public sealed record StoryCheck(BackgroundStat Stat, int Difficulty, BackgroundStat? AlternateStat = null,
    Func<GameSession, int>? Bonus = null);

// Bir hikâye seçeneği. Check yoksa zar atılmaz; Requirement sağlanmazsa seçenek pasif görünür.
public sealed record StoryChoice(string Id, string Label, string SuccessText, IReadOnlyList<StoryConsequence> Success)
{
    public static StoryChoice Continue(string label = "DEVAM ET") => new("continue", label, "", []);

    public StoryCheck? Check { get; init; }
    public string FailureText { get; init; } = "";
    public IReadOnlyList<StoryConsequence> Failure { get; init; } = [];
    public Func<GameSession, bool>? Requirement { get; init; }
    public string RequirementText { get; init; } = "";

    // false ise sonuçtan sonraki kareler (örneğin taşın uyanışı) bu seçimde oynamaz;
    // seçim yalnız bir savaşı başlatır ve uyanış zaferden sonra ayrı olayda gösterilir.
    public bool PlaysAfterFrames { get; init; } = true;

    public bool IsAvailable(GameSession session) => Requirement?.Invoke(session) ?? true;
}

public sealed record StoryOutcome(bool Success, string Text, IReadOnlyList<StoryConsequence> Consequences, D20Roll? Roll)
{
    public string Summary => string.Join(" · ",
        new[] { Text }.Concat(Consequences.Select(x => x.Summary)).Where(x => x.Length > 0));
}

// Bütün hikâye olaylarının tabanı: kararlı kimlik, sahne, metin, tetik ve tek-sefer bilgisi.
public abstract class StoryEvent
{
    protected StoryEvent(string id, string title, StoryScene scene, IReadOnlyList<string> pages)
    {
        Id = id;
        Title = title;
        Scene = scene;
        Pages = pages;
    }

    public string Id { get; }
    public string Title { get; }
    public StoryScene Scene { get; }

    // Seçimden önceki karelerin anlatımı; AfterPages sonuçtan sonra oynayan kareler içindir.
    public IReadOnlyList<string> Pages { get; }
    public IReadOnlyList<string> AfterPages { get; init; } = [];
    public StoryTrigger Trigger { get; init; } = StoryTrigger.Chain;
    public Func<GameSession, bool>? Requirement { get; init; }
    public bool OneShot { get; init; } = true;

    // Oyuncu konum olaylarını erteleyebilir; zincir ve geçiş olayları ertelenemez.
    public bool CanPostpone { get; init; }

    // Sonuçtan sonra autosave alınır. Kuleden düşüşte can 0 olduğu için kayıt diriltmeden sonra alınır.
    public bool SaveAfter { get; init; } = true;

    // Zincir olayı yarım kalırsa (örneğin savaştan kaçış) oyuncu bu hexe yeniden girince devam eder.
    public HexCoordinate? ResumeAt { get; init; }

    // Seçimden bağımsız, her sonuçta uygulanan kesin sonuçlar.
    public IReadOnlyList<StoryConsequence> Always { get; init; } = [];

    // Bu olaydan sonra denenecek olaylar; ilk tetiklenebilen oynar.
    public IReadOnlyList<string> Next { get; init; } = [];

    public virtual IReadOnlyList<StoryChoice> Choices => [StoryChoice.Continue()];

    public virtual bool CanTrigger(GameSession session) =>
        (!OneShot || !session.Story.HasCompleted(Id)) && (Requirement?.Invoke(session) ?? true);

    // Seçimi uygular. Zar gereken seçimlerde ekranda gösterilen D20Roll nesnesinin kendisi verilir.
    public abstract StoryOutcome Resolve(GameSession session, StoryChoice choice, D20Roll? roll);

    public void Complete(GameSession session) => session.Story.MarkCompleted(Id);

    protected StoryOutcome Apply(GameSession session, bool success, string text, IReadOnlyList<StoryConsequence> chosen, D20Roll? roll)
    {
        IReadOnlyList<StoryConsequence> all = chosen.Concat(Always).ToArray();
        foreach (StoryConsequence consequence in all) consequence.Apply(session);
        return new StoryOutcome(success, StoryText.Format(session, text), all, roll);
    }
}

// Zarsız konuşma ve anlatım. Seçenekler yalnız yönü belirler.
public sealed class DialogueStoryEvent(string id, string title, StoryScene scene, IReadOnlyList<string> pages,
    IReadOnlyList<StoryChoice>? choices = null) : StoryEvent(id, title, scene, pages)
{
    private readonly IReadOnlyList<StoryChoice> _choices = choices is { Count: > 0 } ? choices : [StoryChoice.Continue()];

    public override IReadOnlyList<StoryChoice> Choices => _choices;

    public override StoryOutcome Resolve(GameSession session, StoryChoice choice, D20Roll? roll) =>
        Apply(session, true, choice.SuccessText, choice.Success, null);
}

// d20 + nitelik kontrolü. Başarısızlık hikâyeyi kilitlemez; yalnız bedel veya alternatif yol üretir.
public sealed class SkillCheckStoryEvent(string id, string title, StoryScene scene, IReadOnlyList<string> pages,
    IReadOnlyList<StoryChoice> choices) : StoryEvent(id, title, scene, pages)
{
    public override IReadOnlyList<StoryChoice> Choices => choices;

    public static int CheckBonus(GameSession session, StoryCheck check)
    {
        int bonus = WorldEncounter.CheckBonus(session.Player, check.Stat);
        if (check.AlternateStat is BackgroundStat alternate)
            bonus = Math.Max(bonus, WorldEncounter.CheckBonus(session.Player, alternate));
        return bonus + (check.Bonus?.Invoke(session) ?? 0);
    }

    public static bool Succeeds(D20Roll roll, int difficulty) =>
        roll.IsNaturalTwenty || (!roll.IsNaturalOne && roll.Total >= difficulty);

    public override StoryOutcome Resolve(GameSession session, StoryChoice choice, D20Roll? roll)
    {
        if (choice.Check is not StoryCheck check) return Apply(session, true, choice.SuccessText, choice.Success, null);
        if (roll is null) throw new InvalidOperationException("Bu seçim d20 kontrolü gerektirir.");
        bool success = Succeeds(roll, check.Difficulty);
        return success
            ? Apply(session, true, choice.SuccessText, choice.Success, roll)
            : Apply(session, false, choice.FailureText, choice.Failure, roll);
    }
}

public enum EnemyTurnKind { Attack, PlayerDown, ScriptedDefeat }

// Düşman sırasının sonucu. Scripted yenilgi normal ölüm değildir ve ölüm ekranı açmaz.
public sealed record EnemyTurnResult(EnemyTurnKind Kind, AttackRoll? Roll)
{
    public bool OpensDeathScreen => Kind == EnemyTurnKind.PlayerDown;
}

// Normal savaş ekranında oynanan hikâye savaşı. Düşman, görsel ve zafer sonuçları burada tanımlanır.
public sealed class CombatStoryEvent(string id, string title, Func<GameSession, EnemyNpc> createEnemy,
    string enemySprite, string enemyAttackSprite, IReadOnlyList<StoryConsequence> victory)
    : StoryEvent(id, title, StoryScene.Image(enemySprite), [])
{
    public string EnemySprite { get; } = enemySprite;
    public string EnemyAttackSprite { get; } = enemyAttackSprite;
    public IReadOnlyList<StoryConsequence> VictoryConsequences { get; } = victory;
    public bool FleeAllowed { get; init; }

    // Kaçınca silinecek bayrak; böylece olayın sahibi tekrar denenebilir ve hikâye kilitlenmez.
    public string? FleeClearsFlag { get; init; }

    // Doluysa düşman, oyuncunun ilk gerçek eyleminden sonra bu scripted yenilgiyi uygular.
    public string? FateStrikeEventId { get; init; }

    // Yenilen düşman çözülüp yok olmaz; final kararı için sahnede kalır.
    public bool EnemyRemainsOnDefeat { get; init; }
    public Func<GameSession, int>? PlayerAttackBonus { get; init; }
    public Func<GameSession, int>? PlayerDamageBonus { get; init; }

    public EnemyNpc CreateEnemy(GameSession session) => createEnemy(session);

    public EnemyTurnResult EnemyTurn(GameSession session, EnemyNpc enemy, CombatService combat)
    {
        if (FateStrikeEventId is string strikeId && !session.Story.ScriptedDefeatPlayed &&
            StoryCatalog.Find(strikeId) is ScriptedDefeatStoryEvent strike)
        {
            strike.Strike(session);
            return new EnemyTurnResult(EnemyTurnKind.ScriptedDefeat, null);
        }
        AttackRoll roll = combat.Attack(enemy, session.Player);
        return new EnemyTurnResult(session.Player.IsAlive ? EnemyTurnKind.Attack : EnemyTurnKind.PlayerDown, roll);
    }

    public StoryOutcome Victory(GameSession session) =>
        Apply(session, true, $"{Title} kazanıldı.", VictoryConsequences, null);

    public void Flee(GameSession session)
    {
        if (FleeClearsFlag is not null) session.Story.Flags.Remove(FleeClearsFlag);
        session.Story.ActiveNodeId = null;
    }

    public override StoryOutcome Resolve(GameSession session, StoryChoice choice, D20Roll? roll) => Victory(session);
}

public enum FateStrikeResult { Fallen, Blocked }

// Kaderin Hükmü. AC'yi, zırhı ve hasar azaltmayı yok sayar; normal ölüm sayılmaz.
// Kaderbozan Zırhı kuşanılmışsa aynı darbe kırılır. Kuleden düşüş ve zırhlı dönüş bu sınıfı kullanır.
public sealed class ScriptedDefeatStoryEvent(string id, string title, StoryScene scene, IReadOnlyList<string> pages)
    : StoryEvent(id, title, scene, pages)
{
    public FateStrikeResult Strike(GameSession session)
    {
        PlayerCharacter player = session.Player;
        if (player.Equipment.Get(EquipmentSlot.StoryLayer) is not FatebreakerArmor &&
            player.Inventory.Find<FatebreakerArmor>() is FatebreakerArmor owned)
            player.Equipment.Equip(owned, owned.Slot);

        if (player.Equipment.Get(EquipmentSlot.StoryLayer) is FatebreakerArmor) return FateStrikeResult.Blocked;

        player.FallToFate();
        session.Story.SetFlag(StoryFlags.ScriptedDefeat);
        session.Story.VaelorState = VaelorState.FirstVictory;
        return FateStrikeResult.Fallen;
    }

    public override StoryOutcome Resolve(GameSession session, StoryChoice choice, D20Roll? roll)
    {
        bool blocked = Strike(session) == FateStrikeResult.Blocked;
        string text = blocked ? "Kaderbozan Zırhı Kaderin Hükmü'nü kırdı." : choice.SuccessText;
        return Apply(session, blocked, text, choice.Success, null);
    }
}

// Dört finalden biri. Uygunluk kuralı tek bir zara değil önceki kararlara bağlıdır.
public sealed class EndingStoryEvent(string endingId, string title, Func<StoryProgress, bool> unlocked,
    string requirementText) : StoryEvent($"ending-{endingId}", title, StoryScene.Meaning("endings-v1", endingId), [])
{
    public string EndingId { get; } = endingId;
    public string RequirementText { get; } = requirementText;

    public bool IsUnlocked(StoryProgress story) => unlocked(story);

    public override bool CanTrigger(GameSession session) =>
        session.Story.VaelorState == VaelorState.Defeated && !session.Story.IsFinished;

    public override StoryOutcome Resolve(GameSession session, StoryChoice choice, D20Roll? roll)
    {
        if (!IsUnlocked(session.Story)) throw new InvalidOperationException($"{Title} finali için koşullar sağlanmadı.");
        return Apply(session, true, EndingRules.Epilogue(session, EndingId),
            [EndingConsequence.Choose(EndingId),
             new QuestConsequence(StoryCatalog.FinalChoiceQuest, QuestChange.Complete, note: Title)], null);
    }
}

// Metinlerdeki {kisi}, {bag} ve {kaynak} yer tutucularını oyuncunun hikâyesiyle doldurur.
public static class StoryText
{
    public static string Format(GameSession session, string text)
    {
        RememberedPerson person = session.Story.Remembered ?? RememberedPerson.From(session.Player.Background);
        string source = session.Story.Flags.GetValueOrDefault("archive:source") switch
        {
            "officer" => "göz işaretli subayın itirafı",
            "plaque" => "anı levhasının yankısı",
            _ => "arşiv görevlisinin bıraktığı not"
        };
        return text.Replace("{kisi}", person.Name).Replace("{bag}", person.Relation).Replace("{kaynak}", source);
    }
}
