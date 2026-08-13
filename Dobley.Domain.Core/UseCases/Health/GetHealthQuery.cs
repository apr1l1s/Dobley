using Dobley.Domain.Core.Repositories;

namespace Dobley.Domain.Core.UseCases.Health;

public record GetHealthQuery
    : IUseCase<HealthCheckResult>;

public record HealthCheckResult(bool DatabaseAvailable, bool CacheAvailable)
{
    public string Status => DatabaseAvailable && CacheAvailable ? "Healthy" : "Unhealthy";
}

public record GetHealthQueryHandler(IHealthCheckRepository HealthCheckRepository)
    : IUseCaseHandler<GetHealthQuery, HealthCheckResult>
{
    public async Task<HealthCheckResult> Handle(GetHealthQuery request, CancellationToken cancellationToken)
    {
        var databaseAvailable = await HealthCheckRepository.CheckDatabaseAsync(cancellationToken);
        var cacheAvailable = await HealthCheckRepository.CheckCacheAsync(cancellationToken);

        return new HealthCheckResult(databaseAvailable, cacheAvailable);
    }
}
