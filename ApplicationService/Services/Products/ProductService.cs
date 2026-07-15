using ApplicationService.Dtos.Products;
using ApplicationService.Services.Contracts;
using Domain.Aggregates.Products;
using Domain.Common;
using Domain.Contracts.Persistence;
using FluentValidation;

namespace ApplicationService.Services.Products;

/// <summary>
/// Application service for managing <see cref="Product"/> entities.
/// Acts as a bridge between the repository layer (<see cref="IProductRepository"/>)
/// and higher-level layers such as controllers or APIs.
/// Provides business logic and DTO mapping for CRUD operations.
/// </summary>
public class ProductService : IProductService
{
    #region Private Fields
    private readonly IProductRepository _productRepository;
    private readonly IValidator<ProductCreateDto> _createValidator;
    private readonly IValidator<ProductUpdateDto> _updateValidator;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new instance of <see cref="ProductService"/>.
    /// </summary>
    /// <param name="productRepository">Repository used for Product persistence operations.</param>
    /// <param name="createValidator">The validator used to validate <see cref="ProductCreateDto"/> when creating a new product.</param>
    /// <param name="updateValidator">The validator used to validate <see cref="ProductUpdateDto"/> when updateing an existing product.</param>
    public ProductService(IProductRepository productRepository,
        IValidator<ProductCreateDto> createValidator,
        IValidator<ProductUpdateDto> updateValidator)
    {
        _productRepository = productRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }
    #endregion

    #region Create(ProductCreateDto productCreateDto)
    /// <summary>
    /// Create new product
    /// </summary>
    /// <param name="productCreateDto">The DTO containing the product data to be created.</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A <see cref="Result"/> indicating whether the product was created successfully,
    /// or containing error details if the operation fails.
    /// </returns>
    public async Task<Result> CreateAsync(ProductCreateDto productCreateDto, CancellationToken cancellationToken)
    {
        if (productCreateDto is null)
            return Result.BadRequest("Model is null.");

        var validationResult = await _createValidator.ValidateAsync( productCreateDto, cancellationToken);

        if (!validationResult.IsValid)
            return Result.BadRequest(string.Join(" | ", validationResult.Errors.Select(x => x.ErrorMessage)));

        var product = new Product(productCreateDto.ProductName, productCreateDto.UnitPrice, productCreateDto.UnitsInStock);
        product.SetUid(Guid.NewGuid());

        var result = await _productRepository.InsertAsync(product, cancellationToken);

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

    #region Update(ProductUpdateDto productUpdateDto)
    /// <summary>
    /// Update an existing product.
    /// <param name="productUpdateDto">The DTO containing the product data to be updated.</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A <see cref="Result"/> indicating whether the product was updated successfully,
    /// or containing error details if the operation fails.
    /// </returns>
    public async Task<Result> UpdateAsync(ProductUpdateDto productUpdateDto, CancellationToken cancellationToken)
    {
        if (productUpdateDto is null)
            return Result.BadRequest("Model is null.");

        var validationResult = await _updateValidator.ValidateAsync( productUpdateDto, cancellationToken);

        if (!validationResult.IsValid)
            return Result.BadRequest(string.Join(" | ", validationResult.Errors.Select(x => x.ErrorMessage)));

        var result = await _productRepository.FindByIdAsync(productUpdateDto.Id, cancellationToken);

        if (result.IsFailure)
            return Result.NotFound("Not found product for update.");

        var product = result.Value;
        product.SetName(productUpdateDto.ProductName);
        product.ChangePrice(productUpdateDto.UnitPrice);

        var updateResult = await _productRepository.UpdateAsync(product, cancellationToken);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.ErrorMessage, updateResult.Status);

        return Result.Success();
    }

    #endregion

    #region IncreaseStock( IncreaseProductStockDto dto)

    /// <summary>
    /// 
    /// </summary>
    /// <param name="increaseProductStockDto"></param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A <see cref="Result"/> indicating whether the product was increase successfully,
    /// or containing error details if the operation fails.
    /// </returns>
    public async Task<Result> IncreaseStockAsync( IncreaseProductStockDto increaseProductStockDto, CancellationToken cancellationToken)
    {
        if (increaseProductStockDto is null)
            return Result.BadRequest("Model is null.");

        if (increaseProductStockDto.Amount <= 0)
            return Result.BadRequest("Amount is invalid.");

        var result = await _productRepository.FindByIdAsync(increaseProductStockDto.ProductId, cancellationToken);

        if (result.IsFailure)
            return Result.NotFound("Not found product for increase stock.");

        var product = result.Value;
        product.IncreaseStock(increaseProductStockDto.Amount);

        var updateResult = await _productRepository.UpdateAsync(product, cancellationToken);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.ErrorMessage, updateResult.Status);

        return Result.Success();
    }
    #endregion

    #region DecreaseStock( DecreaseProductStockDto decreaseProductStockDto)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="decreaseProductStockDto"></param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns></returns>
    public async Task<Result> DecreaseStockAsync(DecreaseProductStockDto decreaseProductStockDto, CancellationToken cancellationToken)
    {
        if (decreaseProductStockDto is null)
            return Result.BadRequest("Model is null.");

        if (decreaseProductStockDto.Amount <= 0)
            return Result.BadRequest("Amount is invalid.");

        var result = await _productRepository.FindByIdAsync(decreaseProductStockDto.ProductId, cancellationToken);

        if (result.IsFailure)
            return Result.NotFound("Not found product for increase stock.");

        var product = result.Value;
        product.DecreaseStock(decreaseProductStockDto.Amount);

        var updateResult = await _productRepository.UpdateAsync(product, cancellationToken);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.ErrorMessage, updateResult.Status);

        return Result.Success();
    }
    #endregion

    #region SoftDelete(ProductByIdDto productByIdDto)

    /// <summary>
    /// Soft deletes a product by setting IsDeleted to true.
    /// </summary>
    /// <param name="productByIdDto">Product identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>Success result or appropriate error.</returns>
    public async Task<Result> SoftDeleteAsync(ProductByIdDto productByIdDto, CancellationToken cancellationToken)
    {
        if (productByIdDto is null || productByIdDto.Id <= 0)
            return Result.BadRequest("Model is null or invalid.");

        var findResult = await _productRepository.FindByIdAsync(productByIdDto.Id, cancellationToken);

        if (findResult.IsFailure)
            return Result.Failure(findResult.ErrorMessage, findResult.Status);

        var product = findResult.Value;

        if (product.IsDeleted)
            return Result.Failure("Product has already been deleted.", ResultStatus.Conflict);

        product.Delete();

        var updateResult = await _productRepository.UpdateAsync(product, cancellationToken);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.ErrorMessage, updateResult.Status);

        return Result.Success();
    }

    #endregion

    #region Delete(ProductByIdDto deleteProductDto)
    /// <summary>
    /// Deletes an product by its identifier.
    /// </summary>
    /// <param name="productByIdDto">DTO containing the ID of the product to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the product was found and deleted successfully.</description></item>
    /// <item><description><c>false</c> if no product with the given ID exists.</description></item>
    /// </list>
    /// </returns>
    public async Task<Result> DeleteAsync(ProductByIdDto productByIdDto, CancellationToken cancellationToken)
    {
        if (productByIdDto is null || productByIdDto.Id <= 0)
            return Result.BadRequest("Model is null or invalid.");

        var result = await _productRepository.DeleteAsync(productByIdDto.Id, cancellationToken);

        if (!result.IsSuccess && result.Status == ResultStatus.NotFound)
            return Result.NotFound("Not found product for delete.");

        if (result.IsFailure)
            return Result.Failure(result.ErrorMessage, result.Status);

        return Result.Success();
    }

    #endregion

    #region GetById(ProductByIdDto productByIdDto)

    /// <summary>
    /// Retrieves a single product by its unique identifier.
    /// </summary>
    /// <param name="productByIdDto">DTO containing the product ID to fetch.</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description>The <see cref="ProductSingleDto"/> if the product exists.</description></item>
    /// <item><description>A <c>NotFound</c> result if the product does not exist.</description></item>
    /// </list>
    /// </returns>
    public async Task<Result<ProductSingleDto>> GetByIdAsync(ProductByIdDto productByIdDto, CancellationToken cancellationToken)
    {
        if (productByIdDto is null || productByIdDto.Id <= 0)
            return Result<ProductSingleDto>.BadRequest("Model is null or invalid.");

        var result = await _productRepository.FindByIdAsync(productByIdDto.Id, cancellationToken);

        if (result.IsFailure)
            return Result<ProductSingleDto>.Failure("Product not found.", result.Status);

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
    public async Task<Result<ListProductDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await _productRepository.SelectAsync(cancellationToken);

        if (result.IsFailure)
            return Result<ListProductDto>.Failure(result.ErrorMessage, ResultStatus.InternalServerError);

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
