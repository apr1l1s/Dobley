namespace Dobley.Domain.Core.Repositories.Users;

public interface IAuthService
{
    Task<AuthTokenPair?> Login(string login, string password, CancellationToken cancellationToken = default);

    Task<AuthTokenPair?> Refresh(string refreshToken, CancellationToken cancellationToken = default);

    Task Logout(string refreshToken, CancellationToken cancellationToken = default);

    Task<bool> Register(string login, string password, CancellationToken cancellationToken = default);
}
