using Dobley.Domain.Core.Entities.Storages;

namespace Dobley.Domain.Core.Repositories.Storages;

public interface IStorageRepository : IRepository<Storage, StorageFilter>
{
    Task<Storage?> GetOwnedStorageAsync(int id, string userName, CancellationToken cancellationToken = default);
}
