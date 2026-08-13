using Dobley.Domain.Core.Repositories;

namespace Dobley.Domain.Core.Tests.UseCases.Fakes;

public class FakeCommonRepository : ICommonRepository
{
    public int SaveChangesCount { get; private set; }

    public void FreeContext()
    {
    }

    public void FreeContext(object entity)
    {
    }

    public void FreeContext(IEnumerable<object> exceptEntities)
    {
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}
