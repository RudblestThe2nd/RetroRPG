namespace RetroRpg.Core.Entities;

public abstract class WorldObject : GameEntity
{
    protected WorldObject(string name, bool blocksMovement) : base(name) => BlocksMovement = blocksMovement;

    public bool BlocksMovement { get; protected set; }
}
