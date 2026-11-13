
namespace RetroRpg.Core.Characters;

// Normal düşmandan güçlü karşılaşma düşmanı. Özel saldırısı saldırı bonusunu artırır.
public class MiniBossNpc : EnemyNpc
{
    public MiniBossNpc(string name, string visualAssetId, int maxHealth, int armorClass, CharacterClass characterClass,
        int rewardGold, string specialAttack)
        : base(name, visualAssetId, maxHealth, armorClass, characterClass, rewardGold)
    {
        SpecialAttack = specialAttack;
    }

    public string SpecialAttack { get; }

    public override int AttackBonus => base.AttackBonus + 2;
}
