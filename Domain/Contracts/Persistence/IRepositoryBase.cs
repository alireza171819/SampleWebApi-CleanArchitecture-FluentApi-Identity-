
namespace Domain.Contracts.Persistence;

/// <summary>
/// Defines a base contract for asynchronous repository operations.
/// for a given entity type with a specified primary key type.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by the repository. Must be a reference type (class).</typeparam>
/// <typeparam name="TPrimaryKey">The type of the entity's primary key (e.g., <c>int</c>, <c>Guid</c>, <c>string</c>).</typeparam>
public interface IRepositoryBase<TEntity, in TPrimaryKey> where TEntity : class
{
    /// <summary>
    /// Inserts a new entity into the database.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InsertAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the specified entity instance.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all entities.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<List<TEntity>> SelectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Finds an entity by its primary key.
    /// </summary>
    /// <param name="id">The primary key (can be null).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity if found, otherwise error.</returns>
    Task<TEntity> FindByIdAsync(TPrimaryKey? id, CancellationToken cancellationToken);

}
