using Domain.Common;
using Domain.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net;

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

    #region [- InsertAsync(TEntity entity) -]
    /// <summary>
    /// Inserts a new entity into the database.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public virtual async Task<Result> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.BadRequest("Entity to insert cannot be null.");
        try
        {
            await DbSet.AddAsync(entity, cancellationToken);
            await SaveChanges(cancellationToken);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            return Result.Failure("The request was canceled by the client.", HttpStatusCode.RequestTimeout);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message, HttpStatusCode.InternalServerError);
        }
       
    }
    #endregion

    #region [- UpdateAsync(TEntity entity) -]
    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public virtual async Task<Result> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        if (entity == null)
            return Result.BadRequest("Entity to update cannot be null.");

        try
        {
            DbSet.Update(entity);
            await SaveChanges(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure("The entity was modified or deleted by another process.", HttpStatusCode.Conflict);
        }
        catch (Exception ex) 
        {
            return Result.Failure(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
    #endregion

    #region [- DeleteAsync(TPrimaryKey id) -]
    /// <summary>
    /// Deletes an entity by its primary key.
    /// </summary>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success, or NotFound if entity does not exist.</returns>
    public virtual async Task<Result> DeleteAsync(TPrimaryKey id, CancellationToken cancellationToken)
    {
        if (id == null) return Result.BadRequest("Identifier is required.");

        var entityToDelete = await DbSet.FindAsync(id, cancellationToken);

        if (entityToDelete == null) return Result.NotFound("Not found entity to delete.");

        try
        {
            DbSet.Remove(entityToDelete);
            await SaveChanges(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure("The entity was modified or deleted by another process.", HttpStatusCode.Conflict);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
    #endregion

    #region [- DeleteAsync(TEntity entityToDelete) -]

    /// <summary>
    /// Deletes the specified entity instance.
    /// </summary>
    /// <param name="entityToDelete">The entity instance to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public virtual async Task<Result> DeleteAsync(TEntity entityToDelete, CancellationToken cancellationToken)
    {
        if (entityToDelete == null)
            return Result.BadRequest("Entity to delete cannot be null.");

        try
        {
            if (DbContext.Entry(entityToDelete).State == EntityState.Detached)
                DbSet.Attach(entityToDelete);

            DbSet.Remove(entityToDelete);
            await SaveChanges(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure("The entity was modified or deleted by another process.", HttpStatusCode.Conflict);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
    #endregion

    #region [- Select() -]
    /// <summary>
    /// Retrieves all entities as a read-only list.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the list of all entities (empty if none).</returns>
    public virtual async Task<Result<List<TEntity>>> Select(CancellationToken cancellationToken)
    {
        try
        {
            var entities = await DbSet.AsNoTracking().ToListAsync(cancellationToken);
            return Result<List<TEntity>>.Success(entities);
        }
        catch (OperationCanceledException)
        {
            return Result<List<TEntity>>.Failure("The request was canceled by the client.", HttpStatusCode.RequestTimeout);
        }
        catch (Exception ex)
        {
            return Result<List<TEntity>>.Failure(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
    #endregion

    #region [- FindByIdAsync(TPrimaryKey id) -]
    /// <summary>
    /// Finds an entity by its primary key.
    /// </summary>
    /// <param name="id">The primary key value (can be null).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the entity if found, otherwise NotFound.</returns>
    public virtual async Task<Result<TEntity>> FindByIdAsync(TPrimaryKey? id, CancellationToken cancellationToken)
    {
        if (id == null) return Result<TEntity>.BadRequest("Identifier is required.");

        try
        {
            var entity = await DbSet.FindAsync(id, cancellationToken);
            return entity == null
                    ? Result<TEntity>.NotFound("Entity with this id was not found.")
                    : Result<TEntity>.Success(entity);
        }
        catch (OperationCanceledException)
        {
            return Result<TEntity>.Failure("The request was canceled.", HttpStatusCode.RequestTimeout);
        }
        catch (Exception ex)
        {
            return Result<TEntity>.Failure(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
    #endregion

    #region [- SaveChanges() -]
    /// <summary>
    /// Saves pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of affected rows.</returns>
    private async Task<int> SaveChanges(CancellationToken cancellationToken)
    {
        return await DbContext.SaveChangesAsync();
    }

    #endregion
}