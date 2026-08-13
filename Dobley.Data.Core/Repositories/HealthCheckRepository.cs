using Dobley.Data.Core.Context;
using Dobley.Domain.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Dobley.Data.Core.Repositories;

public class HealthCheckRepository(DobleyContext db, IDistributedCache cache)
    : IHealthCheckRepository
{
    public async Task<bool> CheckCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.GetStringAsync("health:probe", cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task<bool> CheckDatabaseAsync(CancellationToken cancellationToken = default)
        => db.Database.CanConnectAsync(cancellationToken);
}
