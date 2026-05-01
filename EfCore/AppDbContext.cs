using Domain.Common;
using EfCore.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EfCore;

/// <summary>
/// Provides the database context for a web API application.
/// </summary>
/// <remarks>
/// The context is configured via dependency injection and exposes
/// <see cref="DbSet{TEntity}"/> properties for each entity type.
/// </remarks>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class
    /// using the supplied options.
    /// </summary>
    /// <param name="options">The options used to configure the context.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }


    #region [- OnModelCreating(ModelBuilder modelBuilder) -]
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.RegisterAllEntities<BaseEntity>(typeof(BaseEntity).Assembly);

        base.OnModelCreating(modelBuilder);
    }
    #endregion
}
