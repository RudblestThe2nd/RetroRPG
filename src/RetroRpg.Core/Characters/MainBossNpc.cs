
namespace RetroRpg.Core.Characters;

// Ana boss. Canı azaldıkça bir sonraki aşamaya geçer ve her aşamada daha isabetli vurur.
public class MainBossNpc : EnemyNpc
{
    public MainBossNpc(string name, string visualAssetId, int maxHealth, int armorClass, CharacterClass characterClass,
        int rewardGold, int phaseCount)
        : base(name, visualAssetId, maxHealth, armorClass, characterClass, rewardGold)
    {
        PhaseCount = Math.Max(1, phaseCount);
    }

    public int PhaseCount { get; }

    // İki aşamalı bir boss canı yarıya indiğinde ikinci aşamaya geçer.
    public int CurrentPhase => Math.Min(PhaseCount, 1 + (MaxHealth - Health) * PhaseCount / MaxHealth);

    public override int AttackBonus => base.AttackBonus + (CurrentPhase - 1) * 2;
}
