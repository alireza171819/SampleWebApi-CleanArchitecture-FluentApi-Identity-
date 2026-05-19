using ApplicationService.Dtos.Products;
using ApplicationService.Services.Contracts;
using Domain.Aggregates.Products;
using Domain.Common;
using Domain.Contracts.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationService.Services.Users;

public class UserService : IUserService
{
    #region Privet Fields
    private readonly IUserRepository _userRepository;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new instance of <see cref="UserService"/>.
    /// </summary>
    /// <param name="userRepository">Repository used for User persistence operations.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="userRepository"/> is null.</exception>
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    #endregion

    #region Create(CreateProductDto createProductDto)
    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="createProductDto">Data transfer object containing required fields for creating an product.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed (e.g., due to client disconnection or timeout).</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the product was successfully created and persisted.</description></item>
    /// <item><description><c>false</c> if the operation logically failed (e.g., duplicate UUID) — note that validation errors typically return <c>Result.BadRequest</c> without a value.</description></item>
    /// </list>
    /// </returns>
    public async Task<Result> Create(CreateProductDto createProductDto, CancellationToken cancellationToken)
    {
        if (createProductDto is null)
            return Result.BadRequest("Model is null.");

        if (string.IsNullOrWhiteSpace(createProductDto.ProductName))
            return Result.BadRequest("Product name is required.");
        if (createProductDto.UnitPrice < 0)
            return Result.BadRequest("Unit price cannot be negative.");
        if (createProductDto.UnitsInStock < 0)
            return Result.BadRequest("Units in stock cannot be negative.");

        var product = new Product(createProductDto.ProductName, createProductDto.UnitPrice, createProductDto.UnitsInStock);
        product.SetUid(createProductDto.UUId == Guid.Empty ? Guid.NewGuid() : createProductDto.UUId);

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

    #region Update(UpdateProductDto updateProductDto)
    /// <summary>
    /// Update an existing product.
    /// <param name="updateProductDto">DTO containing the product ID and fields to update .</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the product was found and successfully updated.</description></item>
    /// <item><description><c>false</c> if the product with the specified ID does not exist (logical failure).</description></item>
    /// </list>
    /// </returns>
    public async Task<Result> Update(UpdateProductDto updateProductDto, CancellationToken cancellationToken)
    {
        if (updateProductDto is null)
            return Result.BadRequest("Model is null.");

        if (updateProductDto.Id <= 0)
            return Result.BadRequest("Id is required.");

        if (string.IsNullOrWhiteSpace(updateProductDto.ProductName))
            return Result.BadRequest("Product name is required.");

        if (updateProductDto.UnitPrice < 0)
            return Result.BadRequest("Unit price cannot be negative.");

        if (updateProductDto.UnitsInStock < 0)
            return Result.BadRequest("Units in stock cannot be negative.");

        Product product = new(updateProductDto.ProductName, updateProductDto.UnitPrice, updateProductDto.UnitsInStock);
        product.SetId(updateProductDto.Id);
        product.SetUid(updateProductDto.UUId == Guid.Empty ? Guid.NewGuid() : updateProductDto.UUId);

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
    /// <item><description>The <see cref="SingleProductDto"/> if the product exists.</description></item>
    /// <item><description>A <c>NotFound</c> result if the product does not exist.</description></item>
    /// </list>
    /// </returns>
    public async Task<Result<SingleProductDto>> GetById(ProductByIdDto productByIdDto, CancellationToken cancellationToken)
    {
        if (productByIdDto is null || productByIdDto.Id <= 0)
            return Result<SingleProductDto>.BadRequest("Model is null or invalid.");

        var result = await _productRepository.FindById(productByIdDto.Id, cancellationToken);

        if (result.IsFailure)
            return Result<SingleProductDto>.Failure("Product not found.", result.StatusCode);

        var product = result.Value;
        var productDto = new SingleProductDto
        {
            Id = product.Id,
            ProductName = product.ProductName,
            UnitsInStock = product.UnitsInStock,
            UnitPrice = product.UnitPrice,
            Uuid = product.Uuid
        };

        return Result<SingleProductDto>.Success(productDto);
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
            return Result<ListProductDto>.Success(new ListProductDto { ProductDtos = new List<SingleProductDto>() });

        var productDtos = result.Value.Select(product => new SingleProductDto
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
