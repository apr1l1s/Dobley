using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Products;

namespace Dobley.Domain.Core.UseCases.Products;

public record DeleteProductCommand(int Id, string UserName)
    : IUseCase<bool>;

public record DeleteProductCommandHandler(IProductRepository ProductRepository, ICommonRepository CommonRepository)
    : IUseCaseHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await ProductRepository.GetOwnedProductAsync(request.Id, request.UserName, cancellationToken);
        if (product is null)
        {
            return false;
        }

        ProductRepository.Delete(product);
        await CommonRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
