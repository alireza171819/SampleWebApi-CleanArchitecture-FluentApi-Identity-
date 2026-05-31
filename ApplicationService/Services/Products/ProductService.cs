using ApplicationService.Dtos.Products;
using ApplicationService.Services.Contracts;
using Domain.Aggregates.Products;
using Domain.Common;
using Domain.Contracts.Persistence;
using System.Net;

namespace ApplicationService.Services.Products;

/// <summary>
/// Application service for managing <see cref="Product"/> entities.
/// Acts as a bridge between the repository layer (<see cref="IProductRepository"/>)
/// and higher-level layers such as controllers or APIs.
/// Provides business logic and DTO mapping for CRUD operations.
/// </summary>
public class ProductService : IProductService
{
    #region Privet Fields
    private readonly IProductRepository _productRepository;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new instance of <see cref="ProductService"/>.
    /// </summary>
    /// <param name="productRepository">Repository used for Product persistence operations.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="productRepository"/> is null.</exception>
    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    #endregion

    #region Create(ProductCreateDto productCreateDto)
    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="productCreateDto">Data transfer object containing required fields for creating an product.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed (e.g., due to client disconnection or timeout).</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the product was successfully created and persisted.</description></item>
    /// <item><description><c>false</c> if the operation logically failed (e.g., duplicate UUID) — note that validation errors typically return <c>Result.BadRequest</c> without a value.</description></item>
    /// </list>
    /// </returns>
    public async Task<Result> Create(ProductCreateDto productCreateDto, CancellationToken cancellationToken)
    {
        if (productCreateDto is null)
            return Result.BadRequest("Model is null.");

        if (string.IsNullOrWhiteSpace(productCreateDto.ProductName))
            return Result.BadRequest("Product name is required.");
        if (productCreateDto.UnitPrice < 0)
            return Result.BadRequest("Unit price cannot be negative.");
        if (productCreateDto.UnitsInStock < 0)
            return Result.BadRequest("Units in stock cannot be negative.");

        var product = new Product(productCreateDto.ProductName, productCreateDto.UnitPrice, productCreateDto.UnitsInStock);
        product.SetUid(productCreateDto.UUId == Guid.Empty ? Guid.NewGuid() : productCreateDto.UUId);

        var result = await _productRepository.Insert(product, cancellationToken);

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

    #region Update(ProductUpdateDto productUpdateDto)
    /// <summary>
    /// Update an existing product.
    /// <param name="productUpdateDto">DTO containing the product ID and fields to update .</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the product was found and successfully updated.</description></item>
    /// <item><description><c>false</c> if the product with the specified ID does not exist (logical failure).</description></item>
    /// </list>
    /// </returns>
    public async Task<Result> Update(ProductUpdateDto productUpdateDto, CancellationToken cancellationToken)
    {
        if (productUpdateDto is null)
            return Result.BadRequest("Model is null.");

        if (productUpdateDto.Id <= 0)
            return Result.BadRequest("Id is required.");

        if (string.IsNullOrWhiteSpace(productUpdateDto.ProductName))
            return Result.BadRequest("Product name is required.");

        if (productUpdateDto.UnitPrice < 0)
            return Result.BadRequest("Unit price cannot be negative.");

        if (productUpdateDto.UnitsInStock < 0)
            return Result.BadRequest("Units in stock cannot be negative.");

        Product product = new(productUpdateDto.ProductName, productUpdateDto.UnitPrice, productUpdateDto.UnitsInStock);
        product.SetId(productUpdateDto.Id);
        product.SetUid(productUpdateDto.UUId == Guid.Empty ? Guid.NewGuid() : productUpdateDto.UUId);

        var updateResult = await _productRepository.Update(product, cancellationToken);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.ErrorMessage, updateResult.StatusCode);

        return Result.Success();
    }

    #endregion

    #region SoftDelete(ProductByIdDto productByIdDto)

    /// <summary>
    /// Soft deletes a product by setting IsDeleted to true.
    /// </summary>
    /// <param name="productByIdDto">Product identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result or appropriate error.</returns>
    public async Task<Result> SoftDeleteAsync(ProductByIdDto productByIdDto, CancellationToken cancellationToken)
    {
        if (productByIdDto is null || productByIdDto.Id <= 0)
            return Result.BadRequest("Model is null or invalid.");

        var findResult = await _productRepository.FindById(productByIdDto.Id, cancellationToken);

        if (findResult.IsFailure)
            return Result.Failure(findResult.ErrorMessage, findResult.StatusCode);

        var product = findResult.Value;

        if (product.IsDeleted)
            return Result.Failure("Product has already been deleted.", HttpStatusCode.Conflict);

        product.Delete();

        var updateResult = await _productRepository.Update(product, cancellationToken);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.ErrorMessage, updateResult.StatusCode);

        return Result.Success();
    }

    #endregion

    #region Delete(ProductByIdDto deleteProductDto)
    /// <summary>
    /// Deletes an product by its identifier.
    /// </summary>
    /// <param name="productByIdDto">DTO containing the ID of the product to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the product was found and deleted successfully.</description></item>
    /// <item><description><c>false</c> if no product with the given ID exists.</description></item>
    /// </list>
    /// </returns>
    public async Task<Result> Delete(ProductByIdDto productByIdDto, CancellationToken cancellationToken)
    {
        if (productByIdDto is null || productByIdDto.Id <= 0)
            return Result.BadRequest("Model is null or invalid.");

        var result = await _productRepository.Delete(productByIdDto.Id, cancellationToken);

        if (!result.IsSuccess && result.StatusCode == HttpStatusCode.NotFound)
            return Result.NotFound("Not found product for delete.");

        if (result.IsFailure)
            return Result.Failure(result.ErrorMessage, result.StatusCode);

        return Result.Success();
    }

    #endregion

    #region GetById(ProductByIdDto productByIdDto)

    /// <summary>
    /// Retrieves a single product by its unique identifier.
    /// </summary>
    /// <param name="productByIdDto">DTO containing the product ID to fetch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description>The <see cref="ProductSingleDto"/> if the product exists.</description></item>
    /// <item><description>A <c>NotFound</c> result if the product does not exist.</description></item>
    /// </list>
    /// </returns>
    public async Task<Result<ProductSingleDto>> GetById(ProductByIdDto productByIdDto, CancellationToken cancellationToken)
    {
        if (productByIdDto is null || productByIdDto.Id <= 0)
            return Result<ProductSingleDto>.BadRequest("Model is null or invalid.");

        var result = await _productRepository.FindById(productByIdDto.Id, cancellationToken);

        if (result.IsFailure)
            return Result<ProductSingleDto>.Failure("Product not found.", result.StatusCode);

        var product = result.Value;
        var productDto = new ProductSingleDto
        {
            Id = product.Id,
            ProductName = product.ProductName,
            UnitsInStock = product.UnitsInStock,
            UnitPrice = product.UnitPrice,
            Uuid = product.Uuid
        };

        return Result<ProductSingleDto>.Success(productDto);
    }

    #endregion

    #region GetAll()
    /// <summary>
    /// Retrieves all products from the data source.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A standardized result containing a <see cref="ListProductDto"/> with all products.
    /// If no products exist, returns a successful result with an empty list (not NotFound).
    /// In case of a database or infrastructure error, returns a failure result.
    /// </returns>
    public async Task<Result<ListProductDto>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _productRepository.Select(cancellationToken);

        if (result.IsFailure)
            return Result<ListProductDto>.Failure(result.ErrorMessage, HttpStatusCode.InternalServerError);

        if (result.Value == null || !result.Value.Any())
            return Result<ListProductDto>.Success(new ListProductDto { ProductDtos = new List<ProductSingleDto>() });

        var productDtos = result.Value.Select(product => new ProductSingleDto
        {
            Id = product.Id,
            ProductName = product.ProductName,
            UnitsInStock = product.UnitsInStock,
            UnitPrice = product.UnitPrice,
            Uuid = product.Uuid
        }).ToList();

        var listProductDto = new ListProductDto { ProductDtos = productDtos };
        return Result<ListProductDto>.Success(listProductDto);
    }

    #endregion
}
