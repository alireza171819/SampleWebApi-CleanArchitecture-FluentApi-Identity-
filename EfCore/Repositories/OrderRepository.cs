using Domain.Aggregates.Orders;
using Domain.Contracts.Persistence;

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


}
