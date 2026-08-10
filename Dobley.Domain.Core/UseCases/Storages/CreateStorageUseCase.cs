using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Forms;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Storages;

namespace Dobley.Domain.Core.UseCases.Storages;

public record CreateStorageUseCase(StorageForm Form, string UserName)
    : IUseCase<Storage?>;

public record CreateStorageUseCaseHandler(IStorageRepository StorageRepository, ICommonRepository CommonRepository)
    : IUseCaseHandler<CreateStorageUseCase, Storage?>
{
    public async Task<Storage?> Handle(CreateStorageUseCase request, CancellationToken cancellationToken)
    {
        var storage = request.Form.ToEntity(request.UserName);
        await StorageRepository.AddAsync(storage, cancellationToken);
        await CommonRepository.SaveChangesAsync(cancellationToken);

        return storage;
    }
}
