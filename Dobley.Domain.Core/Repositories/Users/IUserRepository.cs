using Dobley.Domain.Core.Entities.Users;

namespace Dobley.Domain.Core.Repositories.Users;

public interface IUserRepository
{
    Task<User?> GetByLogin(string login);

    Task AddAsync(User user);
}