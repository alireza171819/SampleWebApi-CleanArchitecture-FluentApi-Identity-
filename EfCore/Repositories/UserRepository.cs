using Domain.Aggregates.Users;
using Domain.Common;
using Domain.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace EfCore.Repositories;

/// <summary>
/// Repository implementation for <see cref="User"/> entity.
/// Provides CRUD operations (Insert, Update, Delete, Select) using Entity Framework Core.
/// This repository communicates with the database via <see cref="AppDbContext"/>.
/// </summary>
public class UserRepository : RepositoryBase<AppDbContext, User, int>, IUserRepository
{
    #region Constructor
    public UserRepository(AppDbContext context) : base (context)
    {
    }
    #endregion


    /// <summary>
    /// Retrieves a user by its domain unique identifier (Uuid).
    /// </summary>
    /// <param name="uuid">The domain UUID of the user (maps to IdentityUser Id).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Result{User}"/> containing the user if found;
    /// otherwise, a <see cref="Result{User}.NotFound"/> result.
    /// </returns>
    public async Task<Result<User>> FindByUuid(Guid uuid, CancellationToken cancellationToken)
    {
        if (uuid == Guid.Empty) return Result<User>.BadRequest("Unic identifier is required.");

        try
        {
            var user = await DbSet.FirstOrDefaultAsync(u => u.Uuid == uuid, cancellationToken);
            return user == null
                    ? Result<User>.NotFound("User with this id was not found.")
                    : Result<User>.Success(user);
        }
        catch (OperationCanceledException)
        {
            return Result<User>.Failure("The request was canceled.", ResultStatus.ClientClosedRequest);
        }
        catch (Exception ex)
        {
            return Result<User>.Failure(ex.Message, ResultStatus.InternalServerError);
        }
    }
    #region FindByUsername(string username)

    /// <summary>
    /// Retrieves a user by its username.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Result{User}"/> containing the user if found;
    /// otherwise, a <see cref="Result{User}.NotFound"/> result.
    /// </returns>
    public async Task<Result<User>> FindByUsername(string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(username.Trim())) return Result<User>.BadRequest("Username cannot be null or empty.");

        try
        {
            var user = await DbSet.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
            return user == null
                    ? Result<User>.NotFound("User with this username was not found.")
                    : Result<User>.Success(user);
        }
        catch (OperationCanceledException)
        {
            return Result<User>.Failure("The request was canceled.", ResultStatus.ClientClosedRequest);
        }
        catch (Exception ex)
        {
            return Result<User>.Failure(ex.Message, ResultStatus.InternalServerError);
        }
    }

    #endregion

    #region FindByEmail(string email)

    /// <summary>
    /// Retrieves a user by its email address.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Result{User}"/> containing the user if found;
    /// otherwise, a <see cref="Result{User}.NotFound"/> result.
    /// </returns>
    public async Task<Result<User>> FindByEmail(string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(email.Trim())) return Result<User>.BadRequest("Email cannot be null or empty.");

        try
        {
            var user = await DbSet.FirstOrDefaultAsync(u => u.Username == email, cancellationToken);
            return user == null
                    ? Result<User>.NotFound("User with this email was not found.")
                    : Result<User>.Success(user);
        }
        catch (OperationCanceledException)
        {
            return Result<User>.Failure("The request was canceled.", ResultStatus.ClientClosedRequest);
        }
        catch (Exception ex)
        {
            return Result<User>.Failure(ex.Message, ResultStatus.InternalServerError);
        }
    }

    #endregion

    #region Select()

    /// <summary>
    /// Retrieves all users that are not marked as deleted.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// A result containing a list of active users (IsDeleted == false) on success,
    /// or an error result on failure.
    /// </returns>
    public override async Task<Result<List<User>>> Select(CancellationToken cancellationToken)
    {
        try
        {
            var query = await DbContext.Set<User>().AsNoTracking().Where(p => p.IsDeleted == false).ToListAsync(cancellationToken);
            return Result<List<User>>.Success(query);
        }
        catch (OperationCanceledException)
        {
            return Result<List<User>>.Failure("The request was canceled by the client.", ResultStatus.ClientClosedRequest);
        }
        catch (Exception ex)
        {
            return Result<List<User>>.Failure(ex.Message, ResultStatus.InternalServerError);
        }

    }
    #endregion

    #region Select(string userName)

    /// <summary>
    /// Retrieves users whose names contain the specified search term (case‑sensitive, depends on database collation).
    /// </summary>
    /// <param name="userName">The search term to look for within user names. Cannot be null or whitespace.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// A result containing a list of matching users on success,
    /// a <see cref="Result{BadRequest}"/> if the name is invalid,
    /// or an error result on failure.
    /// </returns>
    public async Task<Result<List<User>>> Select(string userName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userName)) return Result<List<User>>.BadRequest("User name cannot be null or empty.");

        try
        {
            var query = await DbContext.Set<User>().AsNoTracking().Where(p => p.Username.Contains(userName)).ToListAsync(cancellationToken);
            return Result<List<User>>.Success(query);
        }
        catch (OperationCanceledException)
        {
            return Result<List<User>>.Failure("The request was canceled by the client.", ResultStatus.ClientClosedRequest);
        }
        catch (Exception ex)
        {
            return Result<List<User>>.Failure(ex.Message, ResultStatus.InternalServerError);
        }
    }
    #endregion

}
