using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Forms;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Products;
using Dobley.Domain.Core.Repositories.Storages;

namespace Dobley.Domain.Core.UseCases.Products;

public record CreateProductUseCase(ProductForm Form, string UserName)
    : IUseCase<Product?>;

public record CreateProductUseCaseHandler(IProductRepository ProductRepository, IStorageRepository StorageRepository,
    ICommonRepository CommonRepository)
    : IUseCaseHandler<CreateProductUseCase, Product?>
{
    public async Task<Product?> Handle(CreateProductUseCase request, CancellationToken cancellationToken)
    {
        if (request.Form.StorageId is null ||
            await StorageRepository.GetOwnedStorageAsync(request.Form.StorageId.Value, request.UserName,
                cancellationToken) is null)
        {
            return null;
        }

        var product = request.Form.ToEntity();
        await ProductRepository.AddAsync(product, cancellationToken);
        await CommonRepository.SaveChangesAsync(cancellationToken);

        return product;
    }
}
