using RetroRpg.Core.Characters;

namespace RetroRpg.Core.Combat;

// Her sınıfın savaşta kullanabildiği tek, kolay anlaşılır aktif güç.
// Alt türler etkinin nasıl çalıştığını açıkça gösterir; ekran yalnız bu ortak tabanı bilir.
public abstract class ClassPower
{
    protected ClassPower(string id, string name, string description, int cost = 1)
    {
        Id = id;
        Name = name;
        Description = description;
        Cost = Math.Max(1, cost);
    }

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public int Cost { get; }
}

public sealed class AttackClassPower : ClassPower
{
    public AttackClassPower(string id, string name, string description,
        int attackAdjustment, int damageDieAdjustment, int damageBonusAdjustment)
        : base(id, name, description)
    {
        AttackAdjustment = attackAdjustment;
        DamageDieAdjustment = damageDieAdjustment;
        DamageBonusAdjustment = damageBonusAdjustment;
    }

    public int AttackAdjustment { get; }
    public bool IsRanged => Id is "marked-shot" or "thorn-burst" or "nature-magic" or "chaos-bolt"
        or "dragon-breath" or "arcane-bolt" or "hell-ray" or "dark-ray" or "evocation-bolt";
    public int DamageDieAdjustment { get; }
    public int DamageBonusAdjustment { get; }
    public int DamageDie(PlayerCharacter player) => Math.Clamp(player.DamageDie + DamageDieAdjustment, 4, 20);
}

public sealed class HealingClassPower : ClassPower
{
    public HealingClassPower(string id, string name, string description, int dieSides, int fixedBonus)
        : base(id, name, description)
    {
        DieSides = Math.Max(4, dieSides);
        FixedBonus = fixedBonus;
    }

    public int DieSides { get; }
    public int FixedBonus { get; }
}

public sealed class GuardClassPower : ClassPower
{
    public GuardClassPower(string id, string name, string description, int armorBonus)
        : base(id, name, description) => ArmorBonus = Math.Max(1, armorBonus);

    public int ArmorBonus { get; }
}

// 12 ana sınıf ve 24 alt sınıf aynı sade güç ailesini kullanır.
// Alt sınıf seçimi yalnız adı ve küçük sayısal farkları değiştirir.
public static class ClassPowerCatalog
{
    public static ClassPower For(CharacterClass characterClass)
    {
        string id = characterClass.SubclassId.Length > 0 ? characterClass.SubclassId : characterClass.Id;
        return id switch
        {
            "berserker" => Attack("rage", "Gözü Kara Öfke", "Daha ağır vurursun; isabet biraz zorlaşır.", -1, 4, 2),
            "totem-warrior" => Guard("totem-guard", "Ayı Totemi", "Bir düşman saldırısı boyunca AC kazanırsın.", 4),
            "barbarian" => Attack("rage", "Öfke", "Öfken silah hasarını artırır.", 0, 2, 2),

            "lore-bard" => Heal("lore-song", "Bilgelik Ezgisi", "Eski bir ezgiyle yaralarını sararsın.", 8, 1),
            "valor-bard" => Guard("valor-song", "Cesaret Marşı", "Bir düşman saldırısı boyunca AC kazanırsın.", 3),
            "bard" => Heal("inspiration", "İlham", "Kendine cesaret verip can yenilersin.", 6, 1),

            "life-cleric" => Heal("life-prayer", "Yaşam Duası", "Güçlü bir kutsal iyileştirme uygularsın.", 10, 2),
            "war-cleric" => Attack("war-blessing", "Savaş Kutsaması", "Kutsanmış bir silah darbesi indirirsin.", 2, 2, 2),
            "cleric" => Heal("healing-word", "Şifa Sözü", "Kutsal sözle can yenilersin.", 8, 1),

            "land-druid" => Attack("thorn-burst", "Diken Patlaması", "Topraktan yükselen dikenlerle saldırırsın.", 1, 2, 1),
            "moon-druid" => Guard("wild-shape", "Vahşi Biçim", "Bir saldırı boyunca dayanıklı bir biçime girersin.", 4),
            "druid" => Attack("nature-magic", "Doğa Büyüsü", "Doğanın gücünü tek darbede toplarsın.", 1, 2, 1),

            "champion" => Attack("heroic-strike", "Kahramanca Darbe", "Yüksek isabetli güçlü bir saldırı yaparsın.", 2, 2, 1),
            "battle-master" => Attack("maneuver", "Savaş Manevrası", "Dengeli bir manevrayla ağır vurursun.", 1, 4, 1),
            "fighter" => Attack("power-strike", "Güçlü Saldırı", "Eğitiminle daha güçlü bir darbe indirirsin.", 1, 2, 1),

            "open-hand" => Attack("open-hand-flurry", "Açık El Serisi", "Hızlı ve isabetli bir darbe serisi yaparsın.", 2, 2, 1),
            "shadow-monk" => Guard("shadow-step", "Gölge Adımı", "Bir saldırı boyunca gölgede kaybolursun.", 4),
            "monk" => Attack("flurry", "Seri Darbe", "Hızlı bir darbe serisi yaparsın.", 2, 0, 2),

            "devotion-paladin" => Guard("sacred-guard", "Kutsal Koruma", "İnancın bir saldırı boyunca seni korur.", 4),
            "vengeance-paladin" => Attack("vengeful-smite", "İntikam Darbesi", "Yemin gücüyle ağır bir darbe indirirsin.", 1, 4, 2),
            "paladin" => Attack("divine-smite", "Kutsal Darbe", "Silahına kutsal güç yüklersin.", 1, 2, 2),

            "hunter" => Attack("marked-shot", "İşaretli Atış", "Avını işaretleyip isabetli vurursun.", 3, 0, 2),
            "beast-master" => Attack("companion-strike", "Yoldaş Saldırısı", "Hayvan yoldaşın saldırına eşlik eder.", 1, 2, 2),
            "ranger" => Attack("marked-shot", "İşaretli Atış", "Avını işaretleyip vurursun.", 2, 0, 2),

            "thief" => Guard("quick-step", "Hızlı Adım", "Bir saldırı boyunca erişilmesi zor hale gelirsin.", 3),
            "assassin" => Attack("assassinate", "Ölümcül Açılış", "İsabetli ve ölümcül bir vuruş denersin.", 2, 4, 2),
            "rogue" => Attack("sneak-attack", "Gizli Saldırı", "Açık yakaladığın hedefe ek hasar verirsin.", 2, 2, 2),

            "wild-magic" => Attack("chaos-bolt", "Kaos Oku", "Kararsız ama güçlü bir büyü oku atarsın.", 0, 6, 2),
            "draconic-bloodline" => Attack("dragon-breath", "Ejder Nefesi", "Ejder soyunun gücünü hedefe boşaltırsın.", 1, 4, 2),
            "sorcerer" => Attack("arcane-bolt", "Doğuştan Büyü", "Doğuştan gelen büyü gücünü salarsın.", 1, 4, 1),

            "fiend-pact" => Attack("hell-ray", "Cehennem Işını", "Paktından gelen yakıcı ışını fırlatırsın.", 1, 4, 2),
            "great-old-one" => Guard("mind-veil", "Zihin Perdesi", "Bir saldırı boyunca düşmanın algısını bozarsın.", 4),
            "warlock" => Attack("dark-ray", "Karanlık Işın", "Karanlık bir enerji ışını fırlatırsın.", 2, 2, 2),

            "evocation" => Attack("evocation-bolt", "Çağırma Oku", "Saf büyü enerjisini hedefe yöneltirsin.", 2, 6, 1),
            "abjuration" => Guard("arcane-shield", "Büyü Kalkanı", "Bir saldırı boyunca büyülü AC kazanırsın.", 5),
            _ => Attack("arcane-bolt", "Büyü Kitabı", "Hazırlanmış bir büyü oku fırlatırsın.", 1, 4, 1)
        };
    }

    private static AttackClassPower Attack(string id, string name, string text, int hit, int die, int damage) =>
        new(id, name, text, hit, die, damage);
    private static HealingClassPower Heal(string id, string name, string text, int die, int bonus) =>
        new(id, name, text, die, bonus);
    private static GuardClassPower Guard(string id, string name, string text, int ac) =>
        new(id, name, text, ac);
}
