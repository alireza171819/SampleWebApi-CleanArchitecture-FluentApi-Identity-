namespace Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// Provides identity handling and equality logic based on domain identity,
/// not object reference or database identifiers.
/// </summary>
public abstract class  BaseEntity
{
    /// <summary>
    /// Initializes a new entity instance and assigns a domain-level unique identifier.
    /// This identifier is independent of persistence concerns and exists
    /// from the moment the entity is created.
    /// </summary>
    protected BaseEntity()
    {
        Uuid = Guid.NewGuid();
    }
    
    /// <summary>
    /// Database identifier.
    /// This value is assigned by the persistence layer and should not be
    /// used for domain equality comparisons.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Domain identity of the entity.
    /// Used to determine equality between entities regardless of their state.
    /// </summary>
    public Guid Uuid { get; set; }
}

