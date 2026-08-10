using Dobley.Data.Core.Context;
using Dobley.Domain.Core.Entities.Users;
using Dobley.Domain.Core.Repositories.Users;
using Microsoft.EntityFrameworkCore;

namespace Dobley.Data.Core.Repositories.Users;

public class UserRepository(DobleyContext context)
    : IUserRepository
{
    public Task<User?> GetByLogin(string login, CancellationToken cancellationToken = default)
        => context.Users.FirstOrDefaultAsync(u => u.Login == login, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await context.Users.AddAsync(user, cancellationToken);
}
