using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Repositories.Storages;

namespace Dobley.Domain.Core.UseCases.Storages;

public record GetStorageQuery(int Id, string UserName)
    : IUseCase<Storage?>;

public record GetStorageQueryHandler(IStorageRepository StorageRepository)
    : IUseCaseHandler<GetStorageQuery, Storage?>
{
    public Task<Storage?> Handle(GetStorageQuery request, CancellationToken cancellationToken)
        => StorageRepository.GetOwnedStorageAsync(request.Id, request.UserName, cancellationToken);
}
