using RetroRpg.Core.Characters;
using RetroRpg.Core.Common;
using RetroRpg.Core.Game;
using RetroRpg.Core.Items;
using RetroRpg.Core.World;

namespace RetroRpg.Core.Story;

// Bir hikâye seçiminin sonucu. Her alt sınıf tek bir şeyi değiştirir.
// Summary boş değilse sonuç ekranında oyuncuya gösterilir.
public abstract class StoryConsequence
{
    public abstract void Apply(GameSession session);
    public virtual string Summary => "";
}

// Küçük hikâye ayrıntılarını bayrak olarak yazar (örneğin waystone:root:mode = cleanse).
public sealed class FlagConsequence(string key, string value = "1") : StoryConsequence
{
    public string Key { get; } = key;
    public string Value { get; } = value;
    public override void Apply(GameSession session) => session.Story.SetFlag(Key, Value);
}

// Eşya verir. İstenirse eski görev eşyasının yerine geçer; aynı görev eşyası iki kez verilmez.
public sealed class ItemConsequence(Func<Item> create, string? replacesQuestKey = null, bool equip = false) : StoryConsequence
{
    public override void Apply(GameSession session)
    {
        PlayerCharacter player = session.Player;
        if (replacesQuestKey is not null)
            foreach (QuestItem old in player.Inventory.Items.OfType<QuestItem>().Where(x => x.QuestKey == replacesQuestKey).ToArray())
                player.Inventory.Remove(old);

        Item item = create();
        bool duplicate = item switch
        {
            QuestItem quest => player.Inventory.Items.OfType<QuestItem>().Any(x => x.QuestKey == quest.QuestKey),
            FatebreakerArmor => player.Inventory.Find<FatebreakerArmor>() is not null,
            _ => false
        };
        if (duplicate) return;

        player.Inventory.Add(item);
        if (equip && item is Armor armor) player.Equipment.Equip(armor, armor.Slot);
    }

    public override string Summary => $"+{create().Name}";
}

// Bölge güvenini değiştirir. En iyi final en az iki bölgede yüksek güven ister.
public sealed class ReputationConsequence(string region, int amount) : StoryConsequence
{
    public override void Apply(GameSession session) => session.Story.AddTrust(region, amount);
    public override string Summary => amount == 0 ? "" : $"{RegionName(region)} güveni {(amount > 0 ? "+" : "")}{amount}";

    public static string RegionName(string region) => region switch
    {
        "greenford" => "Yeşilgeçit",
        "swamp" => "Bataklık",
        "mountain" => "Dağ",
        "desert" => "Çöl",
        "refugees" => "Mülteci",
        _ => region
    };
}

// Rota açar veya oyuncuyu hikâye konumuna taşır. Açılan rota kayıt bayrağıyla kalıcıdır.
public sealed class RouteConsequence : StoryConsequence
{
    private RouteConsequence(string? unlockFlag, HexCoordinate? destination, string summary)
    {
        UnlockFlag = unlockFlag;
        Destination = destination;
        _summary = summary;
    }

    private readonly string _summary;
    public string? UnlockFlag { get; }
    public HexCoordinate? Destination { get; }

    public static RouteConsequence Unlock(string flag, string summary) => new(flag, null, summary);
    public static RouteConsequence MoveTo(HexCoordinate destination, string summary) => new(null, destination, summary);

    public override void Apply(GameSession session)
    {
        if (UnlockFlag is not null) session.ApplyUnlock(UnlockFlag);
        if (Destination is HexCoordinate hex) session.PlaceAt(hex);
    }

    public override string Summary => _summary;
}

// Final uygunluğunu kaydeder ya da seçilen finali yazar.
// Uygunluk Vaelor yenildiği anda önceki kararlardan hesaplanır; rastgele zar kullanılmaz.
public sealed class EndingConsequence : StoryConsequence
{
    private EndingConsequence(string? endingId) => EndingId = endingId;

    public string? EndingId { get; }

    public static EndingConsequence UnlockAvailable() => new(null);
    public static EndingConsequence Choose(string endingId) => new(endingId);

    public override void Apply(GameSession session)
    {
        if (EndingId is null)
        {
            session.Story.UnlockedEndings.Clear();
            session.Story.UnlockedEndings.AddRange(EndingRules.Available(session.Story));
        }
        else session.Story.ChosenEnding = EndingId;
    }
}

public enum QuestChange { Start, CompleteObjective, Complete, Fail }

// Görev başlatır, hedef işaretler, tamamlar veya başarısız sayar.
public sealed class QuestConsequence(string questId, QuestChange change, string? objectiveId = null, string? note = null)
    : StoryConsequence
{
    public override void Apply(GameSession session)
    {
        Quest quest = session.Story.Quest(questId);
        switch (change)
        {
            case QuestChange.Start: quest.Start(); break;
            case QuestChange.CompleteObjective: quest.CompleteObjective(objectiveId!); break;
            case QuestChange.Complete: quest.Complete(note); break;
            case QuestChange.Fail: quest.Fail(note ?? "Başarısız oldu."); break;
        }
    }

    public override string Summary => change switch
    {
        QuestChange.Start => $"Yeni görev: {StoryCatalog.QuestTitle(questId)}",
        QuestChange.Complete => $"Tamamlandı: {StoryCatalog.QuestTitle(questId)}",
        QuestChange.Fail => $"Başarısız: {StoryCatalog.QuestTitle(questId)}",
        _ => ""
    };
}

// Başarısızlığın bedeli veya dinlenmenin kazancı: can, altın, erzak.
// Hikâye bedeli oyuncuyu öldürmez; en az 1 CAN bırakır. Ölüm yalnız gerçek savaşta olur.
public sealed class CostConsequence(int health = 0, int gold = 0, int supplies = 0, bool fullHeal = false) : StoryConsequence
{
    public static CostConsequence FullHeal() => new(fullHeal: true);

    public override void Apply(GameSession session)
    {
        PlayerCharacter player = session.Player;
        if (fullHeal) player.Heal(player.MaxHealth);
        else if (health > 0) player.Heal(health);
        else if (health < 0) player.ReceiveDamage(Math.Min(-health, Math.Max(0, player.Health - 1)));
        player.Gold = Math.Max(0, player.Gold + gold);
        for (int i = 0; i < supplies; i++)
            if (player.Inventory.Find<Consumable>() is Consumable supply) player.Inventory.Remove(supply);
    }

    public override string Summary => string.Join(" · ", new[]
    {
        fullHeal ? "CAN tamamen doldu" : health > 0 ? $"+{health} CAN" : health < 0 ? $"{health} CAN" : "",
        gold > 0 ? $"+{gold} altın" : gold < 0 ? $"{gold} altın" : "",
        supplies > 0 ? $"-{supplies} erzak" : ""
    }.Where(x => x.Length > 0));
}
