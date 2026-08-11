using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Products;
using Dobley.Domain.Core.Repositories.Storages;

namespace Dobley.Domain.Core.UseCases.Storages;

public record DeleteStorageUseCase(int Id, string UserName)
    : IUseCase<bool>;

public record DeleteStorageUseCaseHandler(IStorageRepository StorageRepository, IProductRepository ProductRepository,
    ICommonRepository CommonRepository)
    : IUseCaseHandler<DeleteStorageUseCase, bool>
{
    public async Task<bool> Handle(DeleteStorageUseCase request, CancellationToken cancellationToken)
    {
        var storage = await StorageRepository.GetOwnedStorageAsync(request.Id, request.UserName, cancellationToken);
        if (storage is null)
        {
            return false;
        }

        var products = await ProductRepository.GetStorageProductsAsync(storage.Id, request.UserName, cancellationToken);
        foreach (var product in products)
        {
            ProductRepository.Delete(product);
        }

        StorageRepository.Delete(storage);
        await CommonRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
