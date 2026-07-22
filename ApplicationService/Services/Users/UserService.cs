using ApplicationService.Dtos.Users;
using ApplicationService.Services.Contracts;
using Domain.Aggregates.Users;
using ApplicationService.Common;
using Domain.Contracts.Persistence;

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

    #region Create(UserCreateDto userCreateDto)
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
    public async Task<Result> CreateAsync(UserCreateDto userCreateDto, CancellationToken cancellationToken)
    {
        if (userCreateDto is null)
            return Result.BadRequest("Model is null.");

        if (string.IsNullOrWhiteSpace(userCreateDto.Username))
            return Result.BadRequest("User name is required.");

        var user = new User(userCreateDto.Username, userCreateDto.Email);
        user.SetUid(userCreateDto.Uuid == Guid.Empty ? Guid.NewGuid() : userCreateDto.Uuid);

        var result = await _userRepository.InsertAsync(user, cancellationToken);

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

    #region Update(UserUpdateDto userUpdateDto)
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
    public async Task<Result> UpdateAsync(UserUpdateDto userUpdateDto, CancellationToken cancellationToken)
    {
        if (userUpdateDto is null)
            return Result.BadRequest("Model is null.");

        if (userUpdateDto.Id <= 0)
            return Result.BadRequest("Id is required.");

        if (string.IsNullOrWhiteSpace(userUpdateDto.Username))
            return Result.BadRequest("User name is required.");

        User user = new(userUpdateDto.Username, userUpdateDto.Email);
        user.SetId(userUpdateDto.Id);

        var updateResult = await _userRepository.UpdateAsync(user, cancellationToken);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.ErrorMessage, updateResult.Status);

        return Result.Success();
    }

    #endregion

    #region SoftDelete(UserByIdDto userByIdDto)

    /// <summary>
    /// Soft deletes a user by setting IsDeleted to true.
    /// </summary>
    /// <param name="userByIdDto">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result or appropriate error.</returns>
    public async Task<Result> SoftDeleteAsync(UserByIdDto userByIdDto, CancellationToken cancellationToken)
    {
        if (userByIdDto is null || userByIdDto.Id <= 0)
            return Result.BadRequest("Model is null or invalid.");

        var findResult = await _userRepository.FindByIdAsync(userByIdDto.Id, cancellationToken);

        if (findResult.IsFailure)
            return Result.Failure(findResult.ErrorMessage, findResult.Status);

        var user = findResult.Value;

        if (user.IsDeleted)
            return Result.Failure("Product has already been deleted.", ResultStatus.Conflict);

        user.Delete();

        var updateResult = await _userRepository.UpdateAsync(user, cancellationToken);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.ErrorMessage, updateResult.Status);

        return Result.Success();
    }

    #endregion

    #region Delete(UserByIdDto userByIdDto)
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
    public async Task<Result> DeleteAsync(UserByIdDto userByIdDto, CancellationToken cancellationToken)
    {
        if (userByIdDto is null || userByIdDto.Id <= 0)
            return Result.BadRequest("Model is null or invalid.");

        var result = await _userRepository.DeleteAsync(userByIdDto.Id, cancellationToken);

        if (!result.IsSuccess && result.Status == ResultStatus.NotFound)
            return Result.NotFound("Not found product for delete.");

        if (result.IsFailure)
            return Result.Failure(result.ErrorMessage, result.Status);

        return Result.Success();
    }

    #endregion

    #region GetById(UserByIdDto userByIdDto)

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
    public async Task<Result<UserSingleDto>> GetByIdAsync(UserByIdDto userByIdDto, CancellationToken cancellationToken)
    {
        if (userByIdDto is null || userByIdDto.Id <= 0)
            return Result<UserSingleDto>.BadRequest("Model is null or invalid.");

        var result = await _userRepository.FindByIdAsync(userByIdDto.Id, cancellationToken);

        if (result.IsFailure)
            return Result<UserSingleDto>.Failure("User not found.", result.Status);

        return Result<UserSingleDto>.Success(ToDto(result.Value));
    }

    #endregion

    #region GetByUuid(UserByUuidDto userByUuidDto)

    /// <summary>
    /// Retrieves a single user by its unique UUID.
    /// </summary>
    /// <param name="userByUuidDto">DTO containing the user's UUID to fetch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description>The <see cref="UserSingleDto"/> if the user exists.</description></item>
    /// <item><description>A <c>NotFound</c> result if the user does not exist.</description></item>
    /// </list>
    /// </returns>
    public async Task<Result<UserSingleDto>> GetByUuidAsync(UserByUuidDto userByUuidDto, CancellationToken cancellationToken)
    {
        if (userByUuidDto is null || userByUuidDto.Uuid == Guid.Empty)
            return Result<UserSingleDto>.BadRequest("Model is null or invalid.");

        var result = await _userRepository.FindByUuidAsync(userByUuidDto.Uuid, cancellationToken);

        if (result.IsFailure)
            return Result<UserSingleDto>.Failure("User not found.", result.Status);

        return Result<UserSingleDto>.Success(ToDto(result.Value));
    }
    #endregion

    #region GetByEmail(UserByEmailDto userByEmailDto)

    /// <summary>
    /// Retrieves a single user by its email address.
    /// </summary>
    /// <param name="userByEmailDto">DTO containing the user's email address to fetch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description>The <see cref="UserSingleDto"/> if the user exists.</description></item>
    /// <item><description>A <c>NotFound</c> result if the user does not exist.</description></item>
    /// </list>
    /// </returns>
    public async Task<Result<UserSingleDto>> GetByEmailAsync(UserByEmailDto userByEmailDto, CancellationToken cancellationToken)
    {
        if (userByEmailDto is null || string.IsNullOrWhiteSpace(userByEmailDto.Email))
            return Result<UserSingleDto>.BadRequest("Model is null or invalid.");

        var result = await _userRepository.FindByEmailAsync(userByEmailDto.Email, cancellationToken);

        if (result.IsFailure)
            return Result<UserSingleDto>.Failure("User not found.", result.Status);

        var user = result.Value;

        var userDto = new UserSingleDto
        {
            Id = user.Id,
            Uuid = user.Uuid,
            Username = user.Username,
            Email = user.Email
        };

        return Result<UserSingleDto>.Success(userDto);
    }

    #endregion

    #region GetByUsername(UserByUsernameDto userByUsernameDto)

    /// <summary>
    /// Retrieves a single user by its username.
    /// </summary>
    /// <param name="userByUsernameDto">DTO containing the user's username to fetch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description>The <see cref="UserSingleDto"/> if the user exists.</description></item>
    /// <item><description>A <c>NotFound</c> result if the user does not exist.</description></item>
    /// </list>
    /// </returns>
    public async Task<Result<UserSingleDto>> GetByUsernameAsync(UserByUsernameDto userByUsernameDto, CancellationToken cancellationToken)
    {
        if (userByUsernameDto is null || string.IsNullOrWhiteSpace(userByUsernameDto.Username))
            return Result<UserSingleDto>.BadRequest("Model is null or invalid.");

        var result = await _userRepository.FindByUsernameAsync(userByUsernameDto.Username, cancellationToken);

        if (result.IsFailure)
            return Result<UserSingleDto>.Failure("User not found.", result.Status);

        var user = result.Value;

        var userDto = new UserSingleDto
        {
            Id = user.Id,
            Uuid = user.Uuid,
            Username = user.Username,
            Email = user.Email
        };

        return Result<UserSingleDto>.Success(userDto);
    }

    #endregion

    #region GetAll()
    /// <summary>
    /// Retrieves all users from the data source.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A standardized result containing a <see cref="UserListDto"/> with all users.
    /// If no users exist, returns a successful result with an empty list (not NotFound).
    /// In case of a database or infrastructure error, returns a failure result.
    /// </returns>
    public async Task<Result<UserListDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await _userRepository.SelectAsync(cancellationToken);

        if (result.IsFailure)
            return Result<UserListDto>.Failure(result.ErrorMessage, ResultStatus.InternalServerError);

        if (result.Value == null || !result.Value.Any())
            return Result<UserListDto>.Success(new UserListDto { SingleUserDtos = new List<UserSingleDto>() });

        var userDtos = result.Value.Select(user => new UserSingleDto
        {
            Id = user.Id,
            Uuid = user.Uuid,
            Username= user.Username,
            Email = user.Email
        }).ToList();

        var listUserDto = new UserListDto { SingleUserDtos = userDtos };
        return Result<UserListDto>.Success(listUserDto);
    }

    #endregion


    private static UserSingleDto ToDto(User user) => new()
    {
        Id = user.Id,
        Uuid = user.Uuid,
        Username = user.Username,
        Email = user.Email
    };
}
