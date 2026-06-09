using ApplicationService.Dtos.Products;
using ApplicationService.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleWebApi.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : BaseApiController
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result =
            await _productService.GetAll(
                cancellationToken);

        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result =
            await _productService.GetById(
                new ProductByIdDto { Id = id },
                cancellationToken);

        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        ProductCreateDto dto,
        CancellationToken cancellationToken)
    {
        var result =
            await _productService.Create(
                dto,
                cancellationToken);

        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        ProductUpdateDto dto,
        CancellationToken cancellationToken)
    {
        dto.Id = id;

        var result =
            await _productService.Update(
                dto,
                cancellationToken);

        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var result =
            await _productService.Delete(
                new ProductByIdDto { Id = id },
                cancellationToken);

        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/soft-delete")]
    public async Task<IActionResult> SoftDelete(
        int id,
        CancellationToken cancellationToken)
    {
        var result =
            await _productService.SoftDelete(
                new ProductByIdDto { Id = id },
                cancellationToken);

        return HandleResult(result);
    }
}
