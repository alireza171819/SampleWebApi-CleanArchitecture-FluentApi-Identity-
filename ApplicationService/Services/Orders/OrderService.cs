using ApplicationService.Dtos.Orders;
using ApplicationService.Services.Contracts;
using Domain.Aggregates.Orders;
using Domain.Common;
using Domain.Contracts.Persistence;
using FluentValidation;

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
    private readonly IValidator<OrderCreateDto> _createValidator;
    private readonly IValidator<OrderUpdateDto> _updateValidator;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new instance of <see cref="OrderService"/>.
    /// </summary>
    /// <param name="orderRepository">Repository used for Order persistence operations.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="orderRepository"/> is null.</exception>
    public OrderService(IOrderRepository orderRepository,
        IValidator<OrderCreateDto> createValidator,
        IValidator<OrderUpdateDto> updateValidator)
    {
        _orderRepository = orderRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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
    public async Task<Result> CreateAsync(OrderCreateDto orderCreateDto, CancellationToken cancellationToken)
    {
        if (orderCreateDto is null)
            return Result.BadRequest("Model is null .");

        var validationResult = await _createValidator.ValidateAsync( orderCreateDto, cancellationToken);

        if (validationResult.IsValid)
            return Result.BadRequest(string.Join(" | ", validationResult.Errors.Select(x => x.ErrorMessage)));

        var order = new Order(orderCreateDto.UserId, orderCreateDto.OrderDate, orderCreateDto.ShippedDate, orderCreateDto.ShippingAddress);
        order.SetUid(orderCreateDto.Uuid == Guid.Empty ? Guid.NewGuid() : orderCreateDto.Uuid);

        var result = await _orderRepository.Insert(order, cancellationToken);

        if (result.IsFailure)
        {
            // To detect the error of the user sending a duplicate uuid.
            if (result.ErrorMessage?.Contains("duplicate") == true || result.ErrorMessage?.Contains("unique") == true)
                return Result.Failure("Duplicate Uuid provided.", ResultStatus.Conflict);

            return Result.Failure(result.ErrorMessage, result.Status);
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
    public async Task<Result> UpdateAsync(OrderUpdateDto orderUpdateDto, CancellationToken cancellationToken)
    {
        if (orderUpdateDto is null)
            return Result.BadRequest("Model is null .");

        var validationResult = await _updateValidator.ValidateAsync( orderUpdateDto, cancellationToken);

        if (validationResult.IsValid)
            return Result.BadRequest(string.Join(" | ", validationResult.Errors.Select(x => x.ErrorMessage)));

        Order order = new(orderUpdateDto.UserId, orderUpdateDto.OrderDate, orderUpdateDto.ShippedDate, orderUpdateDto.ShippingAddress);
        order.SetId(orderUpdateDto.Id);

        var result = await _orderRepository.Update(order, cancellationToken);

        if (result.IsFailure)
            return Result.Failure(result.ErrorMessage, result.Status);

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
            return Result.Failure(findResult.ErrorMessage, findResult.Status);

        var order = findResult.Value;

        if (order.IsDeleted)
            return Result.Failure("Product has already been deleted.", ResultStatus.Conflict);

        order.Delete();

        var updateResult = await _orderRepository.Update(order, cancellationToken);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.ErrorMessage, updateResult.Status);

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
    public async Task<Result> DeleteAsync(OrderByIdDto orderByIdDto, CancellationToken cancellationToken)
    {
        if (orderByIdDto is null || orderByIdDto.Id <= 0)
            return Result.BadRequest("Model is null or invalid.");

        var result = await _orderRepository.Delete(orderByIdDto.Id, cancellationToken);

        if (result.IsFailure && result.Status == ResultStatus.NotFound)
            return Result.NotFound("Order for delete not found.");

        if (result.IsFailure)
            return Result.Failure(result.ErrorMessage, ResultStatus.InternalServerError);

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
    public async Task<Result<OrderSingleDto>> GetByIdAsync(OrderByIdDto orderByIdDto, CancellationToken cancellationToken)
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
        orderDto.ShippedDate = order.ShippedDate;
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
    public async Task<Result<OrderListDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await _orderRepository.Select(cancellationToken);

        if (result.IsFailure)
            return Result<OrderListDto>.Failure(result.ErrorMessage, ResultStatus.InternalServerError);

        if (result.Value == null || !result.Value.Any())
            return Result<OrderListDto>.Success(new OrderListDto() { OrderDtos = new List<OrderSingleDto>()});

        var OrderDtos = result.Value.Select(order => new OrderSingleDto
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            ShippedDate = order.ShippedDate,
            Uuid = order.Uuid,
        }).ToList();

        var listOrderDto = new OrderListDto { OrderDtos = OrderDtos };
        return Result<OrderListDto>.Success(listOrderDto);
    }
    #endregion

}
