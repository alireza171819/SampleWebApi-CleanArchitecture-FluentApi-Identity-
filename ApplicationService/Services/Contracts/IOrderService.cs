using ApplicationService.Dtos.Orders;
using Domain.Common;

namespace ApplicationService.Services.Contracts;

/// <summary>
/// Defines the contract for order-related application services.
/// 
/// Responsibilities:
/// - Accepts Order DTOs from the presentation layer (e.g., controllers)
/// - Performs CRUD operations through the service implementation
/// - Returns standardized results using <see cref="Result{T}"/> for consistent API responses
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="orderCreateDto">Data transfer object containing required fields for creating an order.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed (e.g., due to client disconnection or timeout).</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the order was successfully created and persisted.</description></item>
    /// <item><description><c>false</c> if the operation logically failed (e.g., duplicate UUID) — note that validation errors typically return <c>Result.BadRequest</c> without a value.</description></item>
    /// </list>
    /// </returns>
    Task<Result> CreateAsync(OrderCreateDto orderCreateDto, CancellationToken cancellationToken);

    /// <summary>
    /// Update an existing order.
    /// <param name="orderUpdateDto">DTO containing the order ID and fields to update (UserId, OrderDate, ShippedDate, ShipAddress, Uuid).</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the order was found and successfully updated.</description></item>
    /// <item><description><c>false</c> if the order with the specified ID does not exist (logical failure).</description></item>
    /// </list>
    /// </returns>
    Task<Result> UpdateAsync(OrderUpdateDto orderUpdateDto, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an order by its identifier.
    /// </summary>
    /// <param name="orderByIdDto">DTO containing the ID of the order to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the order was found and deleted successfully.</description></item>
    /// <item><description><c>false</c> if no order with the given ID exists.</description></item>
    /// </list>
    /// </returns>
    Task<Result> DeleteAsync(OrderByIdDto orderByIdDto, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a single order by its unique identifier.
    /// </summary>
    /// <param name="orderByIdDto">DTO containing the order ID to fetch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description>The <see cref="OrderSingleDto"/> if the order exists.</description></item>
    /// <item><description>A <c>NotFound</c> result if the order does not exist.</description></item>
    /// </list>
    /// </returns>
    Task<Result<OrderSingleDto>> GetByIdAsync(OrderByIdDto orderByIdDto, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all orders from the data source.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A standardized result containing a <see cref="OrderListDto"/> with all orders.
    /// If no orders exist, returns a successful result with an empty list (not NotFound).
    /// In case of a database or infrastructure error, returns a failure result.
    /// </returns>
    Task<Result<OrderListDto>> GetAllAsync(CancellationToken cancellationToken);
}

