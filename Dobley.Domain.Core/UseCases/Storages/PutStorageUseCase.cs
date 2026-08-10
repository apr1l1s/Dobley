using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Forms;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Storages;

namespace Dobley.Domain.Core.UseCases.Storages;

public record PutStorageUseCase(int Id, StorageForm Form, string UserName)
    : IUseCase<Storage?>;

public record PutStorageUseCaseHandler(IStorageRepository StorageRepository, ICommonRepository CommonRepository)
    : IUseCaseHandler<PutStorageUseCase, Storage?>
{
    public async Task<Storage?> Handle(PutStorageUseCase request, CancellationToken cancellationToken)
    {
        var storage = await StorageRepository.GetOwnedStorageAsync(request.Id, request.UserName, cancellationToken);
        if (storage is null)
        {
            return null;
        }

        storage.Update(request.Form.Name, request.Form.Description, null);

        await CommonRepository.SaveChangesAsync(cancellationToken);
        return storage;
    }
}
