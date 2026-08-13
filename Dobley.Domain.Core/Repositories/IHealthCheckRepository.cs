namespace Dobley.Domain.Core.Repositories;

public interface IHealthCheckRepository
{
    Task<bool> CheckCacheAsync(CancellationToken cancellationToken = default);

    Task<bool> CheckDatabaseAsync(CancellationToken cancellationToken = default);
}
