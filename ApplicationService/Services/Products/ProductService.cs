using Domain.Aggregates.Products;
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
    /// - Returns an <see cref="IResponse{T}"/> describing success/failure for API usage
    /// </summary>
    /// <param name="postProductDto">Input DTO containing product data.</param>
    /// <returns>Success flag wrapped in a response object.</returns>
    public async Task<IResponse<bool>> Post(PostProductDto postProductDto)
    {
        if (postProductDto is null)
            return new Response<bool>("Model is null .", HttpStatusCode.BadRequest);

        var product = new Product();
        product.ProductName = postProductDto.ProductName;
        product.UnitPrice = postProductDto.UnitPrice;
        product.UnitsInStock = postProductDto.UnitsInStock;
        product.UUId = postProductDto.UUId == Guid.Empty ? Guid.NewGuid() : postProductDto.UUId;
        product.Code = postProductDto.Code;
        product.GregorianDateCreate = DateTime.Now;
        product.GregorianDateUpdate = DateTime.Now;

        var response = await _productRepository.InsertAsync(product);

        if (!response.IsSuccessful)
            return new Response<bool>(response.ErrorMessage, HttpStatusCode.InternalServerError);

        return new Response<bool>(true);
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
    public async Task<IResponse<bool>> Put(PutProductDto putProductDto)
    {
        if (putProductDto is null)
            return new Response<bool>("Model is null .", HttpStatusCode.BadRequest);
        if (putProductDto.Id <= 0)
            return new Response<bool>("Id is required .", HttpStatusCode.BadRequest);

        Product product = new();
        product.Id = putProductDto.Id;
        product.ProductName = putProductDto.ProductName;
        product.UnitsInStock = putProductDto.UnitsInStock;
        product.UnitPrice = putProductDto.UnitPrice;
        product.UUId = putProductDto.UUId == Guid.Empty ? Guid.NewGuid() : putProductDto.UUId;
        product.Code = putProductDto.Code;
        product.GregorianDateUpdate = DateTime.Now;

        var response = await _productRepository.UpdateAsync(product);

        if (!response.IsSuccessful)
            return new Response<bool>(response.ErrorMessage, HttpStatusCode.InternalServerError);

        return new Response<bool>(true);
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
    public async Task<IResponse<bool>> Delete(DeleteProductDto deleteProductDto)
    {
        if (deleteProductDto is null)
            return new Response<bool>("deleteProductDto is null .", HttpStatusCode.BadRequest);

        var response = await _productRepository.FindByIdAsync(deleteProductDto.Id);

        if (!response.IsSuccessful)
            return new Response<bool>(response.ErrorMessage, HttpStatusCode.NotFound);

        var responseDelete = await _productRepository.DeleteAsync(response.Result.Id);

        if (!responseDelete.IsSuccessful)
            return new Response<bool>(responseDelete.ErrorMessage, HttpStatusCode.InternalServerError);

        return new Response<bool>(true);
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
    /// <returns>A list wrapper DTO inside an <see cref="IResponse{T}"/>.</returns>
    public async Task<IResponse<ListProductDto>> GetAll()
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
