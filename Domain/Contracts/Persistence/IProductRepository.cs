
using Domain.Aggregates.Products;

namespace Domain.Contracts.Persistence;

public interface IProductRepository : IRepositoryBase<Product, int>
{
}
