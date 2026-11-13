using RetroRpg.Core.Common;

namespace RetroRpg.Core.Scenes;

public abstract class SceneNode(string id, string title, SceneKind kind)
{
    public string Id { get; } = id;
    public string Title { get; } = title;
    public SceneKind Kind { get; } = kind;
    public virtual bool CanEnter() => true;
}

public sealed class TravelScene(string id, string title, string biomeId) : SceneNode(id, title, SceneKind.Travel)
{
    public string BiomeId { get; } = biomeId;
}

public sealed class CombatScene(string id, string title, string enemyId) : SceneNode(id, title, SceneKind.Combat)
{
    public string EnemyId { get; } = enemyId;
}

public sealed class CampScene(string id, string title) : SceneNode(id, title, SceneKind.Camp);
public sealed class ShopScene(string id, string title) : SceneNode(id, title, SceneKind.Inventory);
