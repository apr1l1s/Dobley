using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Forms;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Products;

namespace Dobley.Domain.Core.UseCases.Products;

public record PutProductUseCase(int Id, ProductForm Form, string UserName)
    : IUseCase<Product?>;

public record PutProductUseCaseHandler(IProductRepository ProductRepository, ICommonRepository CommonRepository)
    : IUseCaseHandler<PutProductUseCase, Product?>
{
    public async Task<Product?> Handle(PutProductUseCase request, CancellationToken cancellationToken)
    {
        var product = await ProductRepository.GetOwnedProductAsync(request.Id, request.UserName, cancellationToken);
        if (product is null)
        {
            return null;
        }

        product = product.Update(request.Form.Name, request.Form.Description, request.Form.Category, request.Form.Unit,
            request.Form.UnitType, request.Form.Price, request.Form.Barcode);

        await CommonRepository.SaveChangesAsync(cancellationToken);
        return product;
    }
}
