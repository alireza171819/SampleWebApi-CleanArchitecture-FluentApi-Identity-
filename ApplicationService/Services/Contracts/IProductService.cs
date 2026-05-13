
using Domain.Common;

namespace ApplicationService.Services.Contracts;
/// <summary>
/// Defines the contract for product-related application services.
/// 
/// Responsibilities:
/// - Accepts Product DTOs from the presentation layer (e.g., controllers)
/// - Performs CRUD operations through the service implementation
/// - Returns standardized results using <see cref="Result{T}"/> for consistent API responses
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="postProductDto">DTO containing the data required to create a product.</param>
    /// <returns>
    /// A standardized result where the value indicates whether the operation succeeded.
    /// </returns>
    Task<Result<bool>> Post(PostProductDto postProductDto);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="putProductDto">DTO containing the product identifier and updated data.</param>
    /// <returns>
    /// A standardized result where the value indicates whether the operation succeeded.
    /// </returns>
    Task<Result<bool>> Put(PutProductDto putProductDto);

    /// <summary>
    /// Deletes an existing product.
    /// </summary>
    /// <param name="deleteProductDto">DTO containing the identifier of the product to delete.</param>
    /// <returns>
    /// A standardized result where the value indicates whether the operation succeeded.
    /// </returns>
    Task<Result<bool>> Delete(DeleteProductDto deleteProductDto);

    /// <summary>
    /// Retrieves all products.
    /// </summary>
    /// <returns>
    /// A standardized result containing a list wrapper DTO.
    /// </returns>
    Task<Result<ListProductDto>> GetAll();

    /// <summary>
    /// Retrieves a single product by its identifier.
    /// </summary>
    /// <param name="getByIdProductDto">DTO containing the identifier of the product to retrieve.</param>
    /// <returns>
    /// A standardized result containing the product data when found.
    /// </returns>
    Task<Result<SingleProductDto>> GetById(GetByIdProductDto getByIdProductDto);
}
