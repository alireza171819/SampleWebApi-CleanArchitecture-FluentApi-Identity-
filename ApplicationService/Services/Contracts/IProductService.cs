
using ApplicationService.Dtos.Products;
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
    /// <param name="productCreateDto">DTO containing product data .</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., client disconnect or timeout).</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the product was successfully created and persisted.</description></item>
    /// <item><description><c>false</c> if a logical conflict occurs (e.g., duplicate product code). Validation errors return <c>BadRequest</c>.</description></item>
    /// </list>
    /// </returns>
    Task<Result> Create(ProductCreateDto productCreateDto, CancellationToken cancellationToken);

    /// <summary>
    /// Replaces an existing product with the provided data.
    /// </summary>
    /// <param name="productUpdateDto">DTO containing product ID and all updatable fields.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the product was found and successfully updated.</description></item>
    /// <item><description><c>false</c> if no product with the specified ID exists (logical failure).</description></item>
    /// </list>
    /// </returns>
    Task<Result> Update(ProductUpdateDto productUpdateDto, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a product by its identifier.
    /// </summary>
    /// <param name="productByIdDto">DTO containing the product ID to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the product was found and deleted successfully.</description></item>
    /// <item><description><c>false</c> if no product with the given ID exists.</description></item>
    /// </list>
    /// </returns>
    Task<Result> Delete(ProductByIdDto productByIdDto, CancellationToken cancellationToken);

    /// <summary>
    /// Soft deletes a product by setting IsDeleted to true.
    /// </summary>
    /// <param name="productByIdDto">Product identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result or appropriate error.</returns>
    Task<Result> SoftDelete(ProductByIdDto productByIdDto, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all products from the data source.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A standardized result containing a <see cref="ListProductDto"/> with all products.
    /// If no products exist, returns a successful result with an empty list (not <c>NotFound</c>).
    /// On database or infrastructure error, returns a failure result.
    /// </returns>
    Task<Result<ListProductDto>> GetAll(CancellationToken cancellationToken);

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
    Task<Result<ProductSingleDto>> GetById(ProductByIdDto productByIdDto, CancellationToken cancellationToken);
}
