namespace Dobley.Domain.Core.Repositories;

public interface ICommonRepository
{
    void FreeContext();

    void FreeContext(object entity);

    void FreeContext(IEnumerable<object> exceptEntities);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
