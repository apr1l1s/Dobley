using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Repositories.Storages;

namespace Dobley.Domain.Core.UseCases.Storages;

public record GetStorageUseCase(int Id, string UserName)
    : IUseCase<Storage?>;

public record GetStorageUseCaseHandler(IStorageRepository StorageRepository)
    : IUseCaseHandler<GetStorageUseCase, Storage?>
{
    public Task<Storage?> Handle(GetStorageUseCase request, CancellationToken cancellationToken)
        => StorageRepository.GetOwnedStorageAsync(request.Id, request.UserName, cancellationToken);
}
