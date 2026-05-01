using Domain.Common;
using Domain.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Repositories;

/// <summary>
/// Base repository that implements common CRUD operations
/// for an entity type using Entity Framework Core and an IdentityDbContext.
/// </summary>
/// <typeparam name="TDbContext">
/// The type of the EF Core DbContext, constrained to IdentityDbContext with custom User/Role types.
/// </typeparam>
/// <typeparam name="TEntity">
/// The entity type represented by this repository.
/// </typeparam>
/// <typeparam name="TPrimaryKey">
/// The type of the primary key of the entity (e.g. int, Guid, string).
/// </typeparam>
public class BaseRepository<TDbContext, TEntity, TPrimaryKey> : IBaseRepository<TEntity, TPrimaryKey> where TEntity : class
                                                                          where TDbContext : DbContext
{
    #region Filds
    /// <summary>
    /// The underlying Entity Framework Core DbContext instance.
    /// Must be initialized (e.g. via DI) in a derived class or externally.
    /// </summary>
    protected virtual TDbContext DbContext { get; set; }
    /// <summary>
    /// The DbSet representing the collection of <typeparamref name="TEntity"/> in the context.
    /// Must be initialized (e.g. in a derived class constructor).
    /// </summary>
    protected virtual DbSet<TEntity> DbSet { get; set; }
    #endregion

    #region Constructor
    /// <summary>
    /// Parameterless constructor for the base repository.
    /// </summary>
    public BaseRepository(TDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }
    #endregion

    #region [- InsertAsync(T_Entity entity) -]
    /// <summary>
    /// Inserts a new entity into the DbSet and saves changes to the database.
    /// </summary>
    /// <param name="entity">The entity instance to insert.</param>
    /// <returns>
    /// A response containing the inserted entity on success, or error information on failure.
    /// </returns>
    public virtual async Task<Result<object>> InsertAsync(TEntity entity)
    {
        await DbSet.AddAsync(entity);
        await SaveChanges();

        return Result<object>.Success(entity);
    }
    #endregion

    #region [- UpdateAsync(T_Entity entity) -]
    /// <summary>
    /// Updates an existing entity in the DbSet and saves changes to the database.
    /// </summary>
    /// <param name="entity">The entity instance with updated values.</param>
    /// <returns>
    /// A result containing the updated entity on success, or error information on failure.
    /// </returns>
    public virtual async Task<Result<object>> UpdateAsync(TEntity entity)
    {
        //DbSet.Attach(entity);
        //DbContext.Entry(entity).State = EntityState.Modified;
        DbContext.Update(entity);
        await SaveChanges();
        return Result<object>.Success(entity);
    }
    #endregion

    #region [- DeleteAsync(U_PrimaryKey id) -]
    /// <summary>
    /// Deletes an entity from the DbSet by its primary key and saves changes to the database.
    /// </summary>
    /// <param name="id">The primary key value of the entity to delete.</param>
    /// <returns>
    /// A result containing the deleted entity on success,
    /// or an empty response if the entity was not found.
    /// </returns>
    public virtual async Task<Result<object>> DeleteAsync(TPrimaryKey id)
    {
        var entityToDelete = await DbSet.FindAsync(id);
        if (entityToDelete == null) return Result<object>.NotFound("Not found entity.");

        DbSet.Remove(entityToDelete);
        await SaveChanges();

        return Result<object>.Success(entityToDelete);
    }
    #endregion

    #region [- DeleteAsync(T_Entity entityToDelete) -]
    /// <summary>
    /// Deletes the specified entity instance from the DbSet and saves changes to the database.
    /// </summary>
    /// <param name="entityToDelete">The entity instance to delete.</param>
    /// <returns>
    /// A result containing the deleted entity on success, or error information on failure.
    /// </returns>
    public virtual async Task<Result<object>> DeleteAsync(TEntity entityToDelete)
    {
        if (DbContext.Entry(entityToDelete).State == EntityState.Detached)
            DbSet.Attach(entityToDelete);

        DbSet.Remove(entityToDelete);
        await SaveChanges();
        return Result<object>.Success(entityToDelete);
    }
    #endregion

    #region [- Select() -]
    /// <summary>
    /// Retrieves all entities in the DbSet without tracking (read-only) and returns them as a list.
    /// </summary>
    /// <returns>
    /// A result containing a list of all entities in the DbSet.
    /// </returns>
    public virtual async Task<Result<List<TEntity>>> Select()
    {
        var q = await DbSet.AsNoTracking().ToListAsync();
        var response = Result<List<TEntity>>.Success(q);
        return response;
    }
    #endregion

    #region [- FindByIdAsync(TPrimaryKey id) -]
    /// <summary>
    /// Finds and returns a single entity by its primary key value.
    /// </summary>
    /// <param name="id">The primary key value of the entity to retrieve.</param>
    /// <returns>
    /// A result containing the entity if found, otherwise an empty response.
    /// </returns>
    public virtual async Task<Result<TEntity>> FindByIdAsync(TPrimaryKey? id)
    {
        var entity = await DbSet.FindAsync(id);
        return entity == null
                ? Result<TEntity>.NotFound("Not found entity whit this id.")
                : Result<TEntity>.Success(entity);
    }
    #endregion

    #region [- SaveChanges() -]
    /// <summary>
    /// Saves all pending changes in the current DbContext to the database.
    /// This is called internally by CRUD methods but can also be used explicitly.
    /// </summary>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public async Task SaveChanges()
    {
        await DbContext.SaveChangesAsync();
    }


    #endregion
}