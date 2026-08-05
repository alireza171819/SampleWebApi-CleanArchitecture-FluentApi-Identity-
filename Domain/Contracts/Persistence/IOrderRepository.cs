using Domain.Aggregates.Orders;
using Domain.Common;

namespace Domain.Contracts.Persistence;

/// <summary>
/// Repository implementation for <see cref="Order"/> entity.
/// Provides CRUD operations (Insert, Update, Delete, Select) using Entity Framework Core.
/// </summary>
public interface IOrderRepository : IRepositoryBase<Order, int>
{
    /// <summary>
    /// Retrieves all orders that are not marked as deleted.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// Return a list of active orders (IsDeleted == false).
    /// </returns>
    Task<List<Order>> SelectAsync(CancellationToken cancellationToken);
}
