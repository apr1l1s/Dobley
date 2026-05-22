using Dobley.Domain.Core.Entities.Products;

namespace Dobley.Domain.Core.Repositories.Products;

public interface IProductRepository : IRepository<Product, ProductFilter>;