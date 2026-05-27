using Dobley.Domain.Core.Repositories;

namespace Dobley.Data.Core.Repositories;

public class CommonRepository(DobleyContext context) : ICommonRepository
{
    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}