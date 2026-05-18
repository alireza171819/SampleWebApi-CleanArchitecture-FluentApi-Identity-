
using Domain.Aggregates.Orders;

namespace Domain.Contracts.Persistence;

public interface IOrderRepository : IRepositoryBase<Order, int>
{
}
