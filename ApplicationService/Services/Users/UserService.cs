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

        try
        {
            await _userRepository.InsertAsync(user, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message, ResultStatus.InternalServerError);
        }
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

        try
        {
            await _userRepository.UpdateAsync(user, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message, ResultStatus.InternalServerError);
        }
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
        if (userByIdDto is null || userByIdDto.Id <= 0 && userByIdDto.Uuid == Guid.Empty)
            return Result.BadRequest("Model is null or invalid.");

        User user;
        if (userByIdDto.Id <= 0)
            user = await _userRepository.FindByUuidAsync(userByIdDto.Uuid, cancellationToken);
        else
            user = await _userRepository.FindByIdAsync(userByIdDto.Id, cancellationToken);

        if (user == null)
            return Result.NotFound("Not found user for delete.");

        if (user.IsDeleted)
            return Result.Failure("Product has already been deleted.", ResultStatus.Conflict);

        user.Delete();

        try
        {
            await _userRepository.UpdateAsync(user, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure( ex.Message, ResultStatus.InternalServerError);
        }
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
        if (userByIdDto is null || userByIdDto.Id <= 0 && userByIdDto.Uuid == Guid.Empty)
            return Result.BadRequest("Model is null or invalid.");

        User user;
        if (userByIdDto.Id <= 0)
            user = await _userRepository.FindByUuidAsync(userByIdDto.Uuid, cancellationToken);
        else
            user = await _userRepository.FindByIdAsync(userByIdDto.Id, cancellationToken);

        if (user == null)
            return Result.NotFound("Not found user for delete.");

        try
        {
            await _userRepository.DeleteAsync(user, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure( ex.Message, ResultStatus.InternalServerError);
        }
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
        if (userByIdDto is null || userByIdDto.Id <= 0 && userByIdDto.Uuid == Guid.Empty)
            return Result<UserSingleDto>.BadRequest("Model is null or invalid.");

        var user = await _userRepository.FindByIdAsync(userByIdDto.Id, cancellationToken);

        if (user == null)
            return Result<UserSingleDto>.NotFound("User not found.");

        return Result<UserSingleDto>.Success(ToDto(user));
    }

    #endregion

    #region GetByUuid(UserByUuidDto userByUuidDto)

    /// <summary>
    /// Retrieves a single user by its unique UUID.
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
    public async Task<Result<UserSingleDto>> GetByUuidAsync(UserByIdDto userByIdDto, CancellationToken cancellationToken)
    {
        if (userByIdDto is null || userByIdDto.Id <= 0 && userByIdDto.Uuid == Guid.Empty)
            return Result<UserSingleDto>.BadRequest("Model is null or invalid.");

        var user = await _userRepository.FindByUuidAsync(userByIdDto.Uuid, cancellationToken);

        if (user == null)
            return Result<UserSingleDto>.NotFound("User not found.");

        return Result<UserSingleDto>.Success(ToDto(user));
    }
    #endregion

    #region GetByEmail(UserByEmailDto userByEmailDto)

    /// <summary>
    /// Retrieves a single user by its email address.
    /// </summary>
    /// <param name="userEmail">User's email address to fetch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description>The <see cref="UserSingleDto"/> if the user exists.</description></item>
    /// <item><description>A <c>NotFound</c> result if the user does not exist.</description></item>
    /// </list>
    /// </returns>
    public async Task<Result<UserSingleDto>> GetByEmailAsync(string userEmail, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userEmail))
            return Result<UserSingleDto>.BadRequest("Model is null or invalid.");

        var user = await _userRepository.FindByEmailAsync(userEmail, cancellationToken);

        if (user == null)
            return Result<UserSingleDto>.NotFound("User not found.");

        return Result<UserSingleDto>.Success(ToDto(user));
    }

    #endregion

    #region GetByUsername(UserByUsernameDto userByUsernameDto)

    /// <summary>
    /// Retrieves a single user by its username.
    /// </summary>
    /// <param name="userUsername">DTO containing the user's username to fetch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A standardized result containing:
    /// <list type="bullet">
    /// <item><description>The <see cref="UserSingleDto"/> if the user exists.</description></item>
    /// <item><description>A <c>NotFound</c> result if the user does not exist.</description></item>
    /// </list>
    /// </returns>
    public async Task<Result<UserSingleDto>> GetByUsernameAsync(string userUsername, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userUsername))
            return Result<UserSingleDto>.BadRequest("Model is null or invalid.");

        var user = await _userRepository.FindByUsernameAsync(userUsername, cancellationToken);

        if (user == null)
            return Result<UserSingleDto>.NotFound("User not found.");

        return Result<UserSingleDto>.Success(ToDto(user));
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
        var users = await _userRepository.SelectAsync(cancellationToken);

        if (users == null || !users.Any())
            return Result<UserListDto>.Success(new UserListDto { SingleUserDtos = new List<UserSingleDto>() });

        var userDtos = users.Select(user => new UserSingleDto
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
