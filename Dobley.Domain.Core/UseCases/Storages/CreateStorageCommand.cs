using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Forms;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Storages;

namespace Dobley.Domain.Core.UseCases.Storages;

public record CreateStorageCommand(StorageForm Form, string UserName)
    : IUseCase<Storage?>;

public record CreateStorageCommandHandler(IStorageRepository StorageRepository, ICommonRepository CommonRepository)
    : IUseCaseHandler<CreateStorageCommand, Storage?>
{
    public async Task<Storage?> Handle(CreateStorageCommand request, CancellationToken cancellationToken)
    {
        var storage = request.Form.ToEntity(request.UserName);
        await StorageRepository.AddAsync(storage, cancellationToken);
        await CommonRepository.SaveChangesAsync(cancellationToken);

        return storage;
    }
}
