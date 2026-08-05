using Domain.Aggregates.Products;

namespace Domain.Contracts.Persistence;

/// <summary>
/// Repository implementation for <see cref="Product"/> entity.
/// Provides CRUD operations (Insert, Update, Delete, Select) using Entity Framework Core.
/// </summary>
public interface IProductRepository : IRepositoryBase<Product, int>
{

    /// <summary>
    /// Retrieves products whose names contain the specified search term (case‑sensitive, depends on database collation).
    /// </summary>
    /// <param name="productName">The search term to look for within product names. Cannot be null or whitespace.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// Return a list of matching products.
    /// </returns>
    Task<List<Product>> SelectAsync(string productName, CancellationToken cancellationToken);
}
