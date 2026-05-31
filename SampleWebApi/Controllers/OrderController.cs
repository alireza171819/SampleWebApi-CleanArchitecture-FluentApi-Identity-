using ApplicationService.Dtos.Orders;
using ApplicationService.Services.Contracts;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net;


namespace SampleWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
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
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _orderService.GetAllAsync(cancellationToken);
            return ToActionResult(result);
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
            var result = await _orderService.GetByIdAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Retrieves all orders for the currently authenticated user.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of user orders.</returns>
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
        {
            // فرض می‌کنیم یک سرویس دیگر یا متد در IOrderService وجود دارد که userId را از توکن دریافت می‌کند.
            // یا می‌توانید userId را از طریق ICurrentUserService دریافت کنید و به سرویس پاس دهید.
            var result = await _orderService.GetOrdersByCurrentUserAsync(cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Creates a new order (places an order).
        /// </summary>
        /// <param name="createDto">Order creation data (list of items, etc.).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created order.</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderCreateDto createDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _orderService.CreateAsync(createDto, cancellationToken);
            if (result.IsSuccess && result.Data != null)
                return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);

            return ToActionResult(result);
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
