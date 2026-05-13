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

    #region Post()
    /// <summary>
    /// Creates a new Product.
    /// 
    /// Notes:
    /// - Generates a new UUID when not provided (Guid.Empty)
    /// - Sets creation/update timestamps on insert
    /// - Returns an <see cref="Result{T}"/> describing success/failure for API usage
    /// </summary>
    /// <param name="postProductDto">Input DTO containing product data.</param>
    /// <returns>Success flag wrapped in a response object.</returns>
    public async Task<Result<bool>> Post(PostProductDto postProductDto)
    {
        if (postProductDto is null)
            return Result<bool>.BadRequest("Model is null .");

        var product = new Product(postProductDto.ProductName, postProductDto.UnitPrice, postProductDto.UnitsInStock);
        product.Uuid = postProductDto.UUId == Guid.Empty ? Guid.NewGuid() : postProductDto.UUId;

        var response = await _productRepository.InsertAsync(product);

        if (!response.IsSuccessful)
            return Result<bool>.Failure(response.ErrorMessage, HttpStatusCode.InternalServerError);

        return Result<bool>.Success(true);
    }

    #endregion

    #region Put()
    /// <summary>
    /// Updates an existing Product by replacing editable fields.
    /// 
    /// Notes:
    /// - Validates that Id is present
    /// - Updates only the fields provided by PutProductDto
    /// - Sets update timestamp on update
    /// </summary>
    /// <param name="putProductDto">Input DTO containing updated product data.</param>
    /// <returns>Success flag wrapped in a response object.</returns>
    public async Task<Result<bool>> Put(PutProductDto putProductDto)
    {
        if (putProductDto is null)
            return Result<bool>.BadRequest("Model is null .");
        if (putProductDto.Id <= 0)
            return Result<bool>.BadRequest("Id is required .");

        Product product = new(putProductDto.ProductName, putProductDto.UnitPrice, putProductDto.UnitsInStock);
        product.Id = putProductDto.Id;
        product.UUId = putProductDto.UUId == Guid.Empty ? Guid.NewGuid() : putProductDto.UUId;

        var response = await _productRepository.UpdateAsync(product);

        if (!response.IsSuccessful)
            return Result<bool>.Failure(response.ErrorMessage, HttpStatusCode.InternalServerError);

        return Result<bool>.Success(true);
    }

    #endregion

    #region Delete()
    /// <summary>
    /// Deletes a Product by deleteProductDto.
    /// 
    /// Notes:
    /// - First checks existence via FindByIdAsync
    /// - Then performs delete by repository
    /// </summary>
    /// <param name="deleteProductDto">Input DTO containing the Id to delete.</param>
    /// <returns>Success flag wrapped in a response object.</returns>
    public async Task<Result<bool>> Delete(DeleteProductDto deleteProductDto)
    {
        if (deleteProductDto is null)
            return Result<bool>.BadRequest("deleteProductDto is null .");

        var response = await _productRepository.FindByIdAsync(deleteProductDto.Id);

        if (!response.IsSuccessful)
            return Result<bool>.NotFound(response.ErrorMessage);

        var responseDelete = await _productRepository.DeleteAsync(response.Result.Id);

        if (!responseDelete.IsSuccessful)
            return Result<bool>.Failure(responseDelete.ErrorMessage, HttpStatusCode.InternalServerError);

        return Result<bool>.Success(true);
    }

    #endregion

    #region GetAll()
    /// <summary>
    /// Retrieves all products.
    /// 
    /// Notes:
    /// - Calls repository Select()
    /// - Maps entities to SingleProductDto items (used as list item DTO here)
    /// - Wraps results in ListProductDto
    /// </summary>
    /// <returns>A list wrapper DTO inside an <see cref="Result{T}"/>.</returns>
    public async Task<Result<ListProductDto>> GetAll()
    {
        var response = await _productRepository.Select();
        if (!response.IsSuccessful || !response.Result.Any())
            return new Response<ListProductDto>(new ListProductDto { ProductDtos = new List<SingleProductDto>() }, false, "", response.ErrorMessage, HttpStatusCode.NotFound);

        var products = response.Result;
        ListProductDto listProductDto = new() { ProductDtos = new List<SingleProductDto>() };
        foreach (var product in products)
        {
            var singleProductDto = new SingleProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                UnitsInStock = product.UnitsInStock,
                UnitPrice = product.UnitPrice,
                Code = product.Code,
                UUId = product.UUId
            };
            listProductDto.ProductDtos.Add(singleProductDto);
        }
        return new Response<ListProductDto>(listProductDto, true, "The process was completed successfully.", "", HttpStatusCode.OK);
    }

    #endregion

    #region GetById()
    /// <summary>
    /// Retrieves a single product by Id.
    /// 
    /// Notes:
    /// - Validates Id is not zero
    /// - Returns NotFound when repository returns null result
    /// - Maps entity to SingleProductDto
    /// </summary>
    /// <param name="getByIdProductDto">DTO containing the Id of the product to fetch.</param>
    /// <returns>A product DTO wrapped in an <see cref="IResponse{T}"/>.</returns>
    public async Task<IResponse<SingleProductDto>> GetById(GetByIdProductDto getByIdProductDto)
    {
        if (getByIdProductDto.Id == 0)
            return new Response<SingleProductDto>("Id is empty .", HttpStatusCode.BadRequest);

        var response = await _productRepository.FindByIdAsync(getByIdProductDto.Id);

        if (response.Result is null)
            return new Response<SingleProductDto>(response.ErrorMessage, HttpStatusCode.NotFound);

        var product = response.Result;
        SingleProductDto productDto = new();
        productDto.Id = product.Id;
        productDto.ProductName = product.ProductName;
        productDto.UnitsInStock = product.UnitsInStock;
        productDto.UnitPrice = product.UnitPrice;
        productDto.Code = product.Code;
        productDto.UUId = product.UUId;

        return new Response<SingleProductDto>(productDto, true, "The process was completed successfully.", "", HttpStatusCode.OK);
    }

    #endregion
}
