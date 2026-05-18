using Domain.Common;

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
    /// <returns>Result of the operation.</returns>
    Task<Result> InsertAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the operation.</returns>
    Task<Result> UpdateAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an entity by its primary key.
    /// </summary>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the operation.</returns>
    Task<Result> DeleteAsync(TPrimaryKey id, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the specified entity instance.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the operation.</returns>
    Task<Result> DeleteAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all entities.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing a list of entities on success.</returns>
    Task<Result<List<TEntity>>> Select(CancellationToken cancellationToken);

    /// <summary>
    /// Finds an entity by its primary key.
    /// </summary>
    /// <param name="id">The primary key (can be null).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the entity if found, otherwise error.</returns>
    Task<Result<TEntity>> FindByIdAsync(TPrimaryKey? id, CancellationToken cancellationToken);

}
