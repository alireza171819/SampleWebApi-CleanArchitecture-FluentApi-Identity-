using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EfCore.Extensions;

/// <summary>
/// Provides extension methods for ModelBuilder to enhance Entity Framework Core configuration capabilities
/// </summary>
/// <remarks>
/// This class contains helper extension methods that automate the entity configuration process
/// and eliminate the need for manual entity registration in DbContext
/// </remarks>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Automatically registers all classes that inherit from the specified base type into the ModelBuilder
    /// </summary>
    /// <typeparam name="BaseEntity">The base entity type (e.g., BaseEntity or IEntity)</typeparam>
    /// <param name="modelBuilder">The ModelBuilder instance used for model configuration</param>
    /// <param name="assemblies">The assemblies to search for entity types</param>
    /// <remarks>
    /// This method finds all concrete, non-abstract classes in the specified assemblies that inherit from BaseEntity,
    /// and registers them with the ModelBuilder by calling Entity{T}() for each type.
    /// 
    /// Usage example:
    /// <code>
    /// protected override void OnModelCreating(ModelBuilder modelBuilder)
    /// {
    ///     modelBuilder.RegisterAllEntities&lt;IEntity&gt;(Assembly.GetExecutingAssembly());
    ///     base.OnModelCreating(modelBuilder);
    /// }
    /// </code>
    /// </remarks>
    public static void RegisterAllEntities<BaseEntity>(this ModelBuilder modelBuilder, params Assembly[] assemblies)
    {
        // Get all concrete, non-abstract types from the specified assemblies that inherit from BaseEntity
        IEnumerable<Type> types = assemblies.SelectMany(a => a.GetExportedTypes()).Where(c => c.IsClass && !c.IsAbstract &&
            typeof(BaseEntity).IsAssignableFrom(c));
        // Register each entity type with the ModelBuilder
        foreach (Type type in types)
            modelBuilder.Entity(type);
    }
}