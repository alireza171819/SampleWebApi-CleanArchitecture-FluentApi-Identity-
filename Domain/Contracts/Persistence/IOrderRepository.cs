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
    /// A result containing a list of active orders (IsDeleted == false) on success,
    /// or an error result on failure.
    /// </returns>
    Task<Result<List<Order>>> Select(CancellationToken cancellationToken);
}
