using ApplicationService.Dtos.Products;
using ApplicationService.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace SampleWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Retrieves all products.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of products.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _productService.GetAll(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a product by its ID.
    /// </summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Product details.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(ProductByIdDto productByIdDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(productByIdDto);

        var result = await _productService.GetById(productByIdDto, cancellationToken);
        if (result.IsFailure)
            return NotFound();

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="createDto">Product creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created product.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductCreateDto createDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productService.Create(createDto, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="productUpdateDto">Product update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ProductUpdateDto productUpdateDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (productUpdateDto.Id <= 0)
            return BadRequest("ID mismatch between URL and body.");

        var result = await _productService.Update(productUpdateDto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a product by its ID.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody]ProductByIdDto productByIdDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productService.Delete(productByIdDto, cancellationToken);
        return Ok(result);
    }

}
