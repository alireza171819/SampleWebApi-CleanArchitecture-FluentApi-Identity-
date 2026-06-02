using Domain.Aggregates.Products;
using Domain.Common;
using Domain.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Repositories;

/// <summary>
/// Repository implementation for <see cref="Product"/> entity.
/// Provides CRUD operations (Insert, Update, Delete, Select) using Entity Framework Core.
/// This repository communicates with the database via <see cref="AppDbContext"/>.
/// </summary>
public class ProductRepository : RepositoryBase<AppDbContext, Product, int> , IProductRepository
{
    #region Constructor
    public ProductRepository(AppDbContext context) : base(context)
    {

    }
    #endregion

    #region Select()

    /// <summary>
    /// Retrieves all products that are not marked as deleted.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// A result containing a list of active products (IsDeleted == false) on success,
    /// or an error result on failure.
    /// </returns>
    public override async Task<Result<List<Product>>> Select(CancellationToken cancellationToken)
    {
        try
        {
            var query = await DbContext.Set<Product>().AsNoTracking().Where(p => p.IsDeleted == false).ToListAsync(cancellationToken);
            return Result<List<Product>>.Success(query);
        }
        catch (OperationCanceledException)
        {
            return Result<List<Product>>.Failure("The request was canceled by the client.", ResultStatus.ClientClosedRequest);
        }
        catch (Exception ex)
        {
            return Result<List<Product>>.Failure(ex.Message, ResultStatus.InternalServerError);
        }
       
    }
    #endregion

    #region Select(string productName)

    /// <summary>
    /// Retrieves products whose names contain the specified search term (case‑sensitive, depends on database collation).
    /// </summary>
    /// <param name="productName">The search term to look for within product names. Cannot be null or whitespace.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// A result containing a list of matching products on success,
    /// a <see cref="Result{BadRequest}"/> if the name is invalid,
    /// or an error result on failure.
    /// </returns>
    public async Task<Result<List<Product>>> Select(string productName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(productName)) return Result<List<Product>>.BadRequest("Product name cannot be null or empty.");

        try
        {
            var query = await DbContext.Set<Product>().AsNoTracking().Where(p => p.ProductName.Contains(productName)).ToListAsync(cancellationToken);
            return Result<List<Product>>.Success(query);
        }
        catch (OperationCanceledException)
        {
            return Result<List<Product>>.Failure("The request was canceled by the client.", ResultStatus.ClientClosedRequest);
        }
        catch (Exception ex)
        {
            return Result<List<Product>>.Failure(ex.Message, ResultStatus.InternalServerError);
        }
    }
    #endregion
}
