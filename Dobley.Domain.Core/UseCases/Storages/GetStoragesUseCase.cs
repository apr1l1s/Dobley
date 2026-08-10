using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Storages;

namespace Dobley.Domain.Core.UseCases.Storages;

public record GetStoragesUseCase(string UserName, int? PageIndex, int? PageSize)
    : IUseCase<PaginatedCollection<Storage>>;

public record GetStoragesUseCaseHandler(IStorageRepository StorageRepository)
    : IUseCaseHandler<GetStoragesUseCase, PaginatedCollection<Storage>>
{
    public Task<PaginatedCollection<Storage>> Handle(GetStoragesUseCase request, CancellationToken cancellationToken)
        => StorageRepository.GetPaginatedCollection(new StorageFilter().SetUserNames([request.UserName]),
            request.PageIndex ?? 1, request.PageSize ?? 10, cancellationToken);
}
