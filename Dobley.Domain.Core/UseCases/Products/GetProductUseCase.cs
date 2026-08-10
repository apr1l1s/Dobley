using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Repositories.Products;

namespace Dobley.Domain.Core.UseCases.Products;

public record GetProductUseCase(int Id, string UserName)
    : IUseCase<Product?>;

public record GetProductUseCaseHandler(IProductRepository ProductRepository)
    : IUseCaseHandler<GetProductUseCase, Product?>
{
    public Task<Product?> Handle(GetProductUseCase request, CancellationToken cancellationToken)
        => ProductRepository.GetOwnedProductAsync(request.Id, request.UserName, cancellationToken);
}
