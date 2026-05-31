using ApplicationService.Dtos.Orders;
using ApplicationService.Dtos.Products;
using ApplicationService.Services.Contracts;
using Domain.Aggregates.Orders;
using Domain.Common;
using Domain.Contracts.Persistence;
using System.Net;

namespace ApplicationService.Services.Orders;

/// <summary>
/// Application service for managing <see cref="Order"/> entities.
/// Acts as a bridge between the repository layer (<see cref="IOrderRepository"/>)
/// and higher-level layers such as controllers or APIs.
/// Provides business logic and DTO mapping for CRUD operations.
/// </summary>
public class OrderService : IOrderService
{
    #region Privet Fields
    private readonly IOrderRepository _orderRepository;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new instance of <see cref="OrderService"/>.
    /// </summary>
    /// <param name="orderRepository">Repository used for Order persistence operations.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="orderRepository"/> is null.</exception>
    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
    #endregion

    #region Create(OrderCreateDto orderCreateDto)
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
    public async Task<Result> Create(OrderCreateDto orderCreateDto, CancellationToken cancellationToken)
    {
        if (orderCreateDto is null)
            return Result.BadRequest("Model is null .");

        if (orderCreateDto.UserId <= 0)
            return Result.BadRequest("UserId is Required .");

        if (orderCreateDto.OrderDate == default || orderCreateDto.OrderDate > DateTime.UtcNow)
            return Result.BadRequest("OrderDate is invalid.");

        if (orderCreateDto.ShipedDate == default || orderCreateDto.ShipedDate < orderCreateDto.OrderDate)
            return Result.BadRequest("ShippedDate invalid and cannot be before OrderDate.");

        if (string.IsNullOrWhiteSpace(orderCreateDto.ShipAddress))
            return Result.BadRequest("ShipAddress is required.");

        var order = new Order(orderCreateDto.UserId, orderCreateDto.OrderDate, orderCreateDto.ShipedDate, orderCreateDto.ShipAddress);
        order.SetUid(orderCreateDto.Uuid == Guid.Empty ? Guid.NewGuid() : orderCreateDto.Uuid);

        var result = await _orderRepository.Insert(order, cancellationToken);

        if (result.IsFailure)
        {
            // To detect the error of the user sending a duplicate uuid.
            if (result.ErrorMessage?.Contains("duplicate") == true || result.ErrorMessage?.Contains("unique") == true)
                return Result.Failure("Duplicate Uuid provided.", HttpStatusCode.Conflict);

            return Result.Failure(result.ErrorMessage, result.StatusCode);
        }

        return Result.Success();
    }

    #endregion

    #region Update(OrderUpdateDto orderUpdateDto)
    /// <summary>
    /// Update an existing order.
    /// <param name="orderUpdateDto">DTO containing the order ID and fields to update .</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the order was found and successfully updated.</description></item>
    /// <item><description><c>false</c> if the order with the specified ID does not exist (logical failure).</description></item>
    /// </list>
    /// </returns>
    public async Task<Result> Update(OrderUpdateDto orderUpdateDto, CancellationToken cancellationToken)
    {
        if (orderUpdateDto is null)
            return Result.BadRequest("Model is null .");

        if (orderUpdateDto.Id <= 0)
            return Result.BadRequest("Id is required .");

        if (orderUpdateDto.OrderDate == default || orderUpdateDto.OrderDate > DateTime.UtcNow)
            return Result.BadRequest("OrderDate is invalid.");

        if (orderUpdateDto.ShipedDate == default || orderUpdateDto.ShipedDate < orderUpdateDto.OrderDate)
            return Result.BadRequest("ShippedDate invalid and cannot be before OrderDate.");

        Order order = new(orderUpdateDto.UserId, orderUpdateDto.OrderDate, orderUpdateDto.ShipedDate, orderUpdateDto.ShipAddress);
        order.SetId(orderUpdateDto.Id);
        order.SetUid(orderUpdateDto.UUId == Guid.Empty ? Guid.NewGuid() : orderUpdateDto.UUId);

        var result = await _orderRepository.Update(order, cancellationToken);

        if (result.IsFailure)
            return Result.Failure(result.ErrorMessage, result.StatusCode);

        return Result.Success();
    }
    #endregion

    #region SoftDelete(OrderByIdDto orderByIdDto)

    /// <summary>
    /// Soft deletes a order by setting IsDeleted to true.
    /// </summary>
    /// <param name="orderByIdDto">Order identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result or appropriate error.</returns>
    public async Task<Result> SoftDeleteAsync(OrderByIdDto orderByIdDto, CancellationToken cancellationToken)
    {
        if (orderByIdDto is null || orderByIdDto.Id <= 0)
            return Result.BadRequest("Model is null or invalid.");

        var findResult = await _orderRepository.FindById(orderByIdDto.Id, cancellationToken);

        if (findResult.IsFailure)
            return Result.Failure(findResult.ErrorMessage, findResult.StatusCode);

        var order = findResult.Value;

        if (order.IsDeleted)
            return Result.Failure("Product has already been deleted.", HttpStatusCode.Conflict);

        order.Delete();

        var updateResult = await _orderRepository.Update(order, cancellationToken);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.ErrorMessage, updateResult.StatusCode);

        return Result.Success();
    }

    #endregion

    #region Delete(OrderByIdDto orderByIdDto)
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
    public async Task<Result> Delete(OrderByIdDto orderByIdDto, CancellationToken cancellationToken)
    {
        if (orderByIdDto is null || orderByIdDto.Id <= 0)
            return Result.BadRequest("Model is null or invalid.");

        var result = await _orderRepository.Delete(orderByIdDto.Id, cancellationToken);

        if (result.IsFailure && result.StatusCode == HttpStatusCode.NotFound)
            return Result.NotFound("Order for delete not found.");

        if (result.IsFailure)
            return Result.Failure(result.ErrorMessage, HttpStatusCode.InternalServerError);

        return Result.Success();
    }
    #endregion

    #region GetById(OrderByIdDto orderByIdDto)
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
    public async Task<Result<OrderSingleDto>> GetById(OrderByIdDto orderByIdDto, CancellationToken cancellationToken)
    {
        if (orderByIdDto is null || orderByIdDto.Id <= 0)
            return Result<OrderSingleDto>.BadRequest("Model is null or invalid.");

        var result = await _orderRepository.FindById(orderByIdDto.Id, cancellationToken);

        if (result.IsFailure || result.Value is null)
            return Result<OrderSingleDto>.NotFound("Order not found."); 

        var order = result.Value;
        OrderSingleDto orderDto = new();
        orderDto.Id = order.Id;
        orderDto.OrderDate = order.OrderDate;
        orderDto.UserId = order.UserId;
        orderDto.ShippAddress = order.ShippingAddress;
        orderDto.ShippedDate = order.ShipedDate;
        orderDto.Uuid = order.Uuid;

        return Result<OrderSingleDto>.Success(orderDto);
    }
    #endregion

    #region GetAll()
    /// <summary>
    /// Retrieves all orders from the data source.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A standardized result containing a <see cref="OrderListDto"/> with all orders.
    /// If no orders exist, returns a successful result with an empty list (not NotFound).
    /// In case of a database or infrastructure error, returns a failure result.
    /// </returns>
    public async Task<Result<OrderListDto>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _orderRepository.Select(cancellationToken);

        if (result.IsFailure)
            return Result<OrderListDto>.Failure(result.ErrorMessage, HttpStatusCode.InternalServerError);

        if (result.Value == null || !result.Value.Any())
            return Result<OrderListDto>.Success(new OrderListDto() { OrderDtos = new List<OrderSingleDto>()});

        var OrderDtos = result.Value.Select(order => new OrderSingleDto
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            ShippedDate = order.ShipedDate,
            Uuid = order.Uuid,
        }).ToList();

        var listOrderDto = new OrderListDto { OrderDtos = OrderDtos };
        return Result<OrderListDto>.Success(listOrderDto);
    }
    #endregion

}
