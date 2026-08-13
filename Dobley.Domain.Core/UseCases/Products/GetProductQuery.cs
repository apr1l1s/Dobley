using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Repositories.Products;

namespace Dobley.Domain.Core.UseCases.Products;

public record GetProductQuery(int Id, string UserName)
    : IUseCase<Product?>;

public record GetProductQueryHandler(IProductRepository ProductRepository)
    : IUseCaseHandler<GetProductQuery, Product?>
{
    public Task<Product?> Handle(GetProductQuery request, CancellationToken cancellationToken)
        => ProductRepository.GetOwnedProductAsync(request.Id, request.UserName, cancellationToken);
}
