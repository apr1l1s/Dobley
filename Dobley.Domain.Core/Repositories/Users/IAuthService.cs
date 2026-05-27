namespace Dobley.Domain.Core.Repositories.Users;

public interface IAuthService
{
    Task<string?> Login(string login, string password);

    Task<bool> Register(string login, string password);
}