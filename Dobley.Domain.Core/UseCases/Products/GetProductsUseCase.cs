using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Products;

namespace Dobley.Domain.Core.UseCases.Products;

public record GetProductsUseCase(int? PageIndex, int? PageSize) : IUseCase<PaginatedCollection<Product>>;

public record GetProductsUseCaseHandler(IProductRepository ProductRepository)
    : IUseCaseHandler<GetProductsUseCase, PaginatedCollection<Product>>
{
    public Task<PaginatedCollection<Product>> Handle(GetProductsUseCase request, CancellationToken cancellationToken)
        => ProductRepository.GetPaginatedCollection(null, request.PageIndex ?? 1, request.PageSize ?? 10);
}