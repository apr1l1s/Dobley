using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Forms;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Products;

namespace Dobley.Domain.Core.UseCases.Products;

public record CreateProductUseCase(ProductForm Form) : IUseCase<Product?>;

public record CreateProductUseCaseHandler(IProductRepository ProductRepository,
    ICommonRepository CommonRepository)
    : IUseCaseHandler<CreateProductUseCase, Product?>
{
    public async Task<Product?> Handle(CreateProductUseCase request, CancellationToken cancellationToken)
    {
        if (request.Form.ToEntity() is { } product)
        {
            if (await ProductRepository.AddAsync(product) is { } _)
            {
                await CommonRepository.SaveChangesAsync();
                return product;
            }
        }

        return null;
    }
}