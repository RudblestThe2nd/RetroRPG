using RetroRpg.Core.Characters;

namespace RetroRpg.Core.Story;

// Dünyanın unuttuğu ama oyuncunun hatırladığı kişi. Geçmiş seçimlerinden otomatik çıkarılır.
public sealed record RememberedPerson(string Type, string Name, string Relation)
{
    // Öncelik sırası hikâye planındaki sırayla aynıdır; ilk eşleşen bağ seçilir.
    public static RememberedPerson From(CharacterBackground background)
    {
        HashSet<string> ids = background.Choices.Select(x => x.Id).Append(background.Id).ToHashSet();
        bool Has(params string[] options) => options.Any(ids.Contains);

        if (Has("turning-loss")) return new("lost-friend", "Arin", "çocukluk arkadaşın");
        if (Has("turning-oath", "goal-protect")) return new("sworn-ward", "Mira", "korumaya yemin ettiğin kişi");
        if (Has("family-soldiers", "profession-guard", "soldier-family"))
            return new("forgotten-commander", "Komutan Doruk", "unutulan birliğinin komutanı");
        if (Has("family-nobles", "noble-family")) return new("family-envoy", "Elçi Kaan", "ailenin elçisi olan kardeşin");
        if (Has("education-academy", "profession-scribe", "goal-knowledge", "temple-student"))
            return new("lost-mentor", "Usta Neva", "sana okumayı öğreten hocan");
        if (Has("family-merchants", "profession-trader", "childhood-caravan", "wandering-merchant"))
            return new("caravan-leader", "Kervanbaşı Tamer", "kayıp kervanın lideri");
        return new("lost-traveler", "Yolcu Ela", "doğduğun bölgeden kaybolan yolcu");
    }
}
