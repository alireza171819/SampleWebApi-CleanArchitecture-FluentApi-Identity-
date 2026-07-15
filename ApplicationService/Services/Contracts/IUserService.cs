
using ApplicationService.Dtos.Users;
using Domain.Common;

namespace ApplicationService.Services.Contracts;

public interface IUserService
{
    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="userCreateDto">Data transfer object containing required fields for creating an user.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed (e.g., due to client disconnection or timeout).</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the user was successfully created and persisted.</description></item>
    /// <item><description><c>false</c> if the operation logically failed (e.g., duplicate UUID) — note that validation errors typically return <c>Result.BadRequest</c> without a value.</description></item>
    /// </list>
    /// </returns>
    Task<Result> CreateAsync(UserCreateDto userCreateDto, CancellationToken cancellationToken);

    /// <summary>
    /// Update an existing user.
    /// <param name="userUpdateDto">DTO containing the user ID and fields to update .</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the user was found and successfully updated.</description></item>
    /// <item><description><c>false</c> if the user with the specified ID does not exist (logical failure).</description></item>
    /// </list>
    /// </returns>
    Task<Result> UpdateAsync(UserUpdateDto userUpdateDto, CancellationToken cancellationToken);

    /// <summary>
    /// Soft deletes a user by setting IsDeleted to true.
    /// </summary>
    /// <param name="userByIdDto">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result or appropriate error.</returns>
    Task<Result> SoftDeleteAsync(UserByIdDto userByIdDto, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an user by its identifier.
    /// </summary>
    /// <param name="userByIdDto">DTO containing the ID of the user to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description><c>true</c> if the user was found and deleted successfully.</description></item>
    /// <item><description><c>false</c> if no user with the given ID exists.</description></item>
    /// </list>
    /// </returns>
    Task<Result> DeleteAsync(UserByIdDto userByIdDto, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a single user by its unique identifier.
    /// </summary>
    /// <param name="userByIdDto">DTO containing the user ID to fetch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description>The <see cref="UserSingleDto"/> if the user exists.</description></item>
    /// <item><description>A <c>NotFound</c> result if the user does not exist.</description></item>
    /// </list>
    /// </returns>
    Task<Result<UserSingleDto>> GetByIdAsync(UserByIdDto userByIdDto, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all users from the data source.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A standardized result containing a <see cref="UserListDto"/> with all users.
    /// If no users exist, returns a successful result with an empty list (not NotFound).
    /// In case of a database or infrastructure error, returns a failure result.
    /// </returns>
    Task<Result<UserListDto>> GetAllAsync(CancellationToken cancellationToken);
}
