namespace RetroRpg.Core.Entities;

public abstract class GameEntity
{
    protected GameEntity(string name)
    {
        Id = Guid.NewGuid();
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Ad boş olamaz.", nameof(name)) : name.Trim();
    }

    public Guid Id { get; init; }
    public string Name { get; protected set; }
    public bool IsActive { get; private set; } = true;

    public virtual void Deactivate() => IsActive = false;
}
