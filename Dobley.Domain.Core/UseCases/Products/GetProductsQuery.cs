using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Products;

namespace Dobley.Domain.Core.UseCases.Products;

public record GetProductsQuery(string UserName, int? PageIndex, int? PageSize)
    : IUseCase<PaginatedCollection<Product>>;

public record GetProductsQueryHandler(IProductRepository ProductRepository)
    : IUseCaseHandler<GetProductsQuery, PaginatedCollection<Product>>
{
    public Task<PaginatedCollection<Product>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        => ProductRepository.GetPaginatedCollection(new ProductFilter().SetUserNames([request.UserName]),
            request.PageIndex ?? 1, request.PageSize ?? 10, cancellationToken);
}
