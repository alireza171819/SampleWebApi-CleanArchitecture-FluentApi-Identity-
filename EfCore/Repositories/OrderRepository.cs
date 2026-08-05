using Domain.Aggregates.Orders;
using Domain.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Repositories;

/// <summary>
/// Repository implementation for <see cref="Order"/> entity.
/// Provides CRUD operations (Insert, Update, Delete, Select) using Entity Framework Core.
/// This repository communicates with the database via <see cref="AppDbContext"/>.
/// </summary>
public class OrderRepository : RepositoryBase<AppDbContext, Order, int>, IOrderRepository
{
    #region Constructor

    public OrderRepository(AppDbContext context) : base(context)
    {
        
    }

    #endregion

    #region Select()

    /// <summary>
    /// Retrieves all orders that are not marked as deleted.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// A result containing a list of active orders (IsDeleted == false).
    /// </returns>
    public override async Task<List<Order>> SelectAsync(CancellationToken cancellationToken) => await DbContext.Set<Order>().AsNoTracking().Where(p => p.IsDeleted == false).ToListAsync(cancellationToken);
    #endregion

}
