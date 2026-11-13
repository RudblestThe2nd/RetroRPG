using RetroRpg.Core.Entities;

namespace RetroRpg.Core.Characters;

// Bütün NPC türlerinin tabanı: yalnız ad, görsel ve temel etkileşim.
public class Npc : GameEntity
{
    public Npc(string name, string visualAssetId) : base(name)
    {
        VisualAssetId = string.IsNullOrWhiteSpace(visualAssetId)
            ? throw new ArgumentException("Görsel kimliği boş olamaz.", nameof(visualAssetId))
            : visualAssetId;
    }

    public string VisualAssetId { get; }

    public virtual string Interact() => $"{Name} sana bakıyor.";
}
