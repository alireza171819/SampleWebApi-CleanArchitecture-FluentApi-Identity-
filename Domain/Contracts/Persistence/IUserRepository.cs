using Domain.Aggregates.Users;

namespace Domain.Contracts.Persistence;

/// <summary>
/// Repository implementation for <see cref="User"/> entity.
/// Provides CRUD operations (Insert, Update, Delete, Select) using Entity Framework Core.
/// </summary>
public interface IUserRepository : IRepositoryBase<User, int>
{
    /// <summary>
    /// Retrieves a user by its domain unique identifier (Uuid).
    /// </summary>
    /// <param name="uuid">The domain UUID of the user (maps to IdentityUser Id).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// Return the user if found.
    /// </returns>
    Task<User> FindByUuidAsync(Guid uuid, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a user by its username.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// Return the user if found.
    /// </returns>
    Task<User> FindByUsernameAsync(string username, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a user by its email address.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// Return the user if found.
    /// </returns>
    Task<User> FindByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves users whose names contain the specified search term (case‑sensitive, depends on database collation).
    /// </summary>
    /// <param name="userName">The search term to look for within user names. Cannot be null or whitespace.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// Return a list of matching users.
    /// </returns>
    Task<List<User>> SelectAsync(string userName, CancellationToken cancellationToken);
}
