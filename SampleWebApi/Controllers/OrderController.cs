using ApplicationService.Dtos.Orders;
using ApplicationService.Services.Contracts;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace SampleWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : BaseApiController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Retrieves all orders (admin only in real scenario).
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of orders.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result =
                await _orderService.GetAll(
                    cancellationToken);

            return HandleResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:int}/ship")]
        public async Task<IActionResult> Ship(int id, CancellationToken cancellationToken)
        {
            var result =
                await _orderService.Ship(
                    id,
                    cancellationToken);

            return HandleResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:int}/deliver")]
        public async Task<IActionResult> Deliver(int id, CancellationToken cancellationToken)
        {
            var result =
                await _orderService.Deliver(
                    id,
                    cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves a specific order by its ID.
        /// </summary>
        /// <param name="id">Order identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Order details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _orderService.GetById(id, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Retrieves all orders for the currently authenticated user.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of user orders.</returns>
        [HttpGet("my-orders")]
        public async Task<IActionResult> MyOrders(CancellationToken cancellationToken)
        {
            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result =
                await _orderService.GetOrdersForUser(
                    int.Parse(userId!),
                    cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Creates a new order (places an order).
        /// </summary>
        /// <param name="createDto">Order creation data (list of items, etc.).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created order.</returns>
        [HttpPost]
        public async Task<IActionResult> Create( OrderCreateDto dto, CancellationToken cancellationToken)
        {
            var result =
                await _orderService.Create(
                    dto,
                    cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Updates an existing order (e.g., change status, shipping address).
        /// </summary>
        /// <param name="id">Order identifier.</param>
        /// <param name="updateDto">Order update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderUpdateDto updateDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.Id)
                return BadRequest("ID mismatch between URL and body.");

            var result = await _orderService.UpdateAsync(updateDto, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Cancels (soft deletes) an order.
        /// </summary>
        /// <param name="id">Order identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        {
            var result = await _orderService.CancelAsync(id, cancellationToken);
            return ToActionResult(result);
        }
    }
}
