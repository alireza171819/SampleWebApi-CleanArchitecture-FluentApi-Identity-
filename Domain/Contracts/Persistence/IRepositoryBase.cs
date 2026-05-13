using Domain.Common;

namespace Domain.Contracts.Persistence;

/// <summary>
/// Defines a base contract for repository operations
/// (CRUD + save) on a given entity type.
/// </summary>
/// <typeparam name="TEntity">
/// The entity type managed by the repository.
/// </typeparam>
/// <typeparam name="TPrimaryKey">
/// The type of the primary key of the entity (e.g. int, Guid, string).
/// </typeparam>
public interface IRepositoryBase<TEntity, in TPrimaryKey> where TEntity : class
{
    /// <summary>
    /// Asynchronously inserts a new entity instance into the underlying data store.
    /// </summary>
    /// <param name="entity">The entity instance to insert.</param>
    /// <returns>
    /// A response that contains the inserted entity on success,
    /// or error information if the operation fails.
    /// </returns>
    Task<Result<object>> InsertAsync(TEntity entity);
    /// <summary>
    /// Asynchronously updates an existing entity instance in the underlying data store.
    /// </summary>
    /// <param name="entity">The entity instance with updated values.</param>
    /// <returns>
    /// A response that contains the updated entity on success,
    /// or error information if the operation fails.
    /// </returns>
    Task<Result<object>> UpdateAsync(TEntity entity);
    /// <summary>
    /// Asynchronously deletes an entity identified by its primary key value
    /// from the underlying data store.
    /// </summary>
    /// <param name="id">The primary key value of the entity to delete.</param>
    /// <returns>
    /// A response that contains the deleted entity on success,
    /// or error information (or an empty response) if the entity is not found
    /// or the operation fails.
    /// </returns>
    Task<Result<object>> DeleteAsync(TPrimaryKey id);
    /// <summary>
    /// Asynchronously deletes the specified entity instance from the underlying data store.
    /// </summary>
    /// <param name="entity">The entity instance to delete.</param>
    /// <returns>
    /// A response that contains the deleted entity on success,
    /// or error information if the operation fails.
    /// </returns>
    Task<Result<object>> DeleteAsync(TEntity entity);
    /// <summary>
    /// Asynchronously retrieves all entities of the specified type from the data store.
    /// </summary>
    /// <returns>
    /// A response that contains a list of all entities on success,
    /// or error information if the operation fails.
    /// </returns>
    Task<Result<List<TEntity>>> Select();
    /// <summary>
    /// Asynchronously finds and returns a single entity by its primary key value.
    /// </summary>
    /// <param name="id">The primary key value of the entity to retrieve; can be null.</param>
    /// <returns>
    /// A response that contains the entity if found,
    /// or an empty / error response if the entity does not exist or the operation fails.
    /// </returns>
    Task<Result<TEntity>> FindByIdAsync(TPrimaryKey? id);
    /// <summary>
    /// Asynchronously saves all pending changes in the underlying data context to the data store.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous save operation.
    /// </returns>
    Task<int> SaveChanges();

}
