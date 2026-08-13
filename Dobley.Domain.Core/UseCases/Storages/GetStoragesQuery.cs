using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Storages;

namespace Dobley.Domain.Core.UseCases.Storages;

public record GetStoragesQuery(string UserName, int? PageIndex, int? PageSize)
    : IUseCase<PaginatedCollection<Storage>>;

public record GetStoragesQueryHandler(IStorageRepository StorageRepository)
    : IUseCaseHandler<GetStoragesQuery, PaginatedCollection<Storage>>
{
    public Task<PaginatedCollection<Storage>> Handle(GetStoragesQuery request, CancellationToken cancellationToken)
        => StorageRepository.GetPaginatedCollection(new StorageFilter().SetUserNames([request.UserName]),
            request.PageIndex ?? 1, request.PageSize ?? 10, cancellationToken);
}
