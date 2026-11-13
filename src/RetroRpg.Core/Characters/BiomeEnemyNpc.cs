using RetroRpg.Core.Common;

namespace RetroRpg.Core.Characters;

// Belirli bir biyoma bağlı normal düşman.
public class BiomeEnemyNpc : EnemyNpc
{
    public BiomeEnemyNpc(string name, string visualAssetId, int maxHealth, int armorClass, CharacterClass characterClass,
        int rewardGold, Biome biome)
        : base(name, visualAssetId, maxHealth, armorClass, characterClass, rewardGold)
    {
        Biome = biome;
    }

    public Biome Biome { get; }
}
