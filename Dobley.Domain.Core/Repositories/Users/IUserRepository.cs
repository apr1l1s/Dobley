using Dobley.Domain.Core.Entities.Users;

namespace Dobley.Domain.Core.Repositories.Users;

public interface IUserRepository
{
    Task<User?> GetByLogin(string login, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
