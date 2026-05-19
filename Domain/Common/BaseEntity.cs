using Domain.Exceptions;

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
    }

    /// <summary>
    /// Database identifier.
    /// This value is assigned by the persistence layer and should not be
    /// used for domain equality comparisons.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Domain identity of the entity.
    /// Used to determine equality between entities regardless of their state.
    /// </summary>
    public Guid Uuid { get; private set; }

    /// <summary>
    /// Sets the database identifier (Id). 
    /// This should typically be called only by the repository after persistence.
    /// </summary>
    /// <param name="id">Positive integer value.</param>
    /// <exception cref="DomainException">Thrown if id is less than or equal to zero.</exception>
    public void SetId(int id)
    {
        if (id <= 0)
            throw new DomainException("Id must be a positive integer.");

        Id = id;
    }

    /// <summary>
    /// Sets the domain unique identifier (Uuid).
    /// This value should be immutable after entity creation.
    /// </summary>
    /// <param name="uid">A non-empty Guid.</param>
    /// <exception cref="DomainException">Thrown if uid is Guid.Empty.</exception>
    public void SetUid(Guid uid)
    {
        if (uid == Guid.Empty)
            throw new DomainException("Uuid cannot be an empty Guid.");

        Uuid = uid;
    }
}

