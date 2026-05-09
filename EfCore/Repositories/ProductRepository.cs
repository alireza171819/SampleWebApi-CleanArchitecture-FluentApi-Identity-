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
    public override async Task<Result<List<Product>>> Select()
    {
        var query = await DbContext.Set<Product>().AsNoTracking().Where(p => p.IsDeleted == false).ToListAsync();
        return Result<List<Product>>.Success(query);
    }
    #endregion

    #region SelectByProductName()

    public async Task<Result<List<Product>>> Select(string name)
    {
        var query = await DbContext.Set<Product>().AsNoTracking().Where(p => p.ProductName.Contains(name)).ToListAsync();
        return Result<List<Product>>.Success(query);
    }
    #endregion
}
