using Domain.Aggregates.Orders;
using Domain.Aggregates.Products;
using Domain.Common;
using Domain.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net;

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
    /// A result containing a list of active orders (IsDeleted == false) on success,
    /// or an error result on failure.
    /// </returns>
    public override async Task<Result<List<Order>>> Select(CancellationToken cancellationToken)
    {
        try
        {
            var query = await DbContext.Set<Order>().AsNoTracking().Where(p => p.IsDeleted == false).ToListAsync(cancellationToken);
            return Result<List<Order>>.Success(query);
        }
        catch (OperationCanceledException)
        {
            return Result<List<Order>>.Failure("The request was canceled by the client.", ResultStatus.ClientClosedRequest);
        }
        catch (Exception ex)
        {
            return Result<List<Order>>.Failure(ex.Message, ResultStatus.InternalServerError);
        }

    }
    #endregion

}
