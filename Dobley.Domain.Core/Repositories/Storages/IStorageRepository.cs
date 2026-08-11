using Dobley.Domain.Core.Entities.Storages;

namespace Dobley.Domain.Core.Repositories.Storages;

public interface IStorageRepository : IRepository<Storage, StorageFilter>
{
    Task<Storage?> GetOwnedStorageAsync(int id, string userName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetOwnedStorageIdsAsync(string userName, IReadOnlyCollection<int> storageIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetStorageIdsAsync(string userName, CancellationToken cancellationToken = default);
}
