using Domain.Common;
using Domain.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;
namespace EfCore.Repositories;

/// <summary>
/// Base abstract repository implementation for CRUD operations using Entity Framework Core.
/// </summary>
/// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
/// <typeparam name="TEntity">
/// The entity type represented by this repository.
/// </typeparam>
/// <typeparam name="TPrimaryKey">
/// The type of the primary key of the entity (e.g. int, Guid, string).
/// </typeparam>
public abstract class RepositoryBase<TDbContext, TEntity, TPrimaryKey> : IRepositoryBase<TEntity, TPrimaryKey> where TEntity : class
                                                                          where TDbContext : DbContext
{
    #region Filds
    /// <summary>
    /// The underlying Entity Framework Core DbContext instance.
    /// Must be initialized (e.g. via DI) in a derived class or externally.
    /// </summary>
    protected virtual TDbContext DbContext { get; }
    /// <summary>
    /// The DbSet representing the collection of <typeparamref name="TEntity"/> in the context.
    /// Must be initialized (e.g. in a derived class constructor).
    /// </summary>
    protected virtual DbSet<TEntity> DbSet { get; }
    #endregion

    #region Constructor
    /// <summary>
    /// Parameterless constructor for the base repository.
    /// </summary>
    public RepositoryBase(TDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }
    #endregion

    #region [- Insert(TEntity entity) -]
    /// <summary>
    /// Inserts a new entity into the database.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public virtual async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        await SaveChanges(cancellationToken);
    }
    #endregion

    #region [- Update(TEntity entity) -]
    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        DbSet.Update(entity);
        await SaveChanges(cancellationToken);
    }
    #endregion

    #region [- Delete(TEntity entityToDelete) -]

    /// <summary>
    /// Deletes the specified entity instance.
    /// </summary>
    /// <param name="entityToDelete">The entity instance to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public virtual async Task DeleteAsync(TEntity entityToDelete, CancellationToken cancellationToken)
    {

        if (DbContext.Entry(entityToDelete).State == EntityState.Detached)
            DbSet.Attach(entityToDelete);

        DbSet.Remove(entityToDelete);
        await SaveChanges(cancellationToken);
    }
    #endregion

    #region [- Select() -]
    /// <summary>
    /// Retrieves all entities as a read-only list.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the list of all entities (empty if none).</returns>
    public virtual async Task<List<TEntity>> SelectAsync(CancellationToken cancellationToken) => await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    #endregion

    #region [- FindById(TPrimaryKey id) -]
    /// <summary>
    /// Finds an entity by its primary key.
    /// </summary>
    /// <param name="id">The primary key value (can be null).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the entity if found, otherwise NotFound.</returns>
    public virtual async Task<TEntity> FindByIdAsync(TPrimaryKey? id, CancellationToken cancellationToken) => await DbSet.FindAsync(id, cancellationToken);
    #endregion

    #region [- SaveChanges() -]
    /// <summary>
    /// Saves pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of affected rows.</returns>
    private async Task<int> SaveChanges(CancellationToken cancellationToken) => await DbContext.SaveChangesAsync();

    #endregion
}