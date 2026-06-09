using ApplicationService.Common.Models;
using ApplicationService.Dtos.Authentications;
using ApplicationService.Dtos.Users;
using Domain.Common;

namespace ApplicationService.Services.Contracts
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Registers a new user in both Identity and Domain databases.
        /// </summary>
        Task<Result> Register(UserRegisterDto userRegisterDto, CancellationToken cancellationToken);

        /// <summary>
        /// Authenticates user and returns tokens. Also syncs domain user state if needed.
        /// </summary>
        Task<Result<AuthResult>> Login(UserLoginDto userLogInDto, CancellationToken cancellationToken);

        /// <summary>
        /// Refreshes access token using refresh token.
        /// </summary>
        Task<Result<AuthResult>> RefreshToken(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken);

        /// <summary>
        /// Logs out the user (invalidates refresh token in Identity).
        /// </summary>
        Task<Result> LogoutAsync(UserByIdDto userByIdDto, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the combined user information from both Identity and Domain.
        /// </summary>
        Task<Result<UserSingleDto>> GetCurrentUserAsync(UserByIdDto userByIdDto, CancellationToken cancellationToken);
    }
}
