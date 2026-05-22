using Dobley.Domain.Core.Repositories;

namespace Dobley.Data.Core.Repositories;

public class CommonRepository(DobleyContext context) : ICommonRepository
{
    public Task SaveChangesAsync() => context.SaveChangesAsync();
}