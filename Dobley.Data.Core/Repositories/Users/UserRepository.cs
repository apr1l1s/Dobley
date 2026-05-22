using Dobley.Domain.Core.Entities.Users;
using Dobley.Domain.Core.Repositories.Users;
using Microsoft.EntityFrameworkCore;

namespace Dobley.Data.Core.Repositories.Users;

public class UserRepository(DobleyContext context)
    : IUserRepository
{
    public Task<User?> GetByLogin(string login) => context.Users.FirstOrDefaultAsync(u => u.Login == login);

    public async Task AddAsync(User user) => await context.Users.AddAsync(user);
}