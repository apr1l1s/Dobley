using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Forms;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Products;

namespace Dobley.Domain.Core.UseCases.Products;

public record PutProductUseCase(int Id, ProductForm Form) : IUseCase<Product?>;

public record PutProductUseCaseHandler(IProductRepository ProductRepository,
    ICommonRepository CommonRepository)
    : IUseCaseHandler<PutProductUseCase, Product?>
{
    public async Task<Product?> Handle(PutProductUseCase request, CancellationToken cancellationToken)
    {
        var (id, form) = request;
        var product = await ProductRepository.GetItemNullable(id);
        if (product is null)
        {
            return null;
        }

        product = product.Update(form.Name, form.Description, form.Category, form.Unit, form.UnitType, form.Price,
            form.Barcode);

        await CommonRepository.SaveChangesAsync();
        return product;
    }
}