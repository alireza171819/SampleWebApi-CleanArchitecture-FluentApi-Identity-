using Domain.Common;
using Domain.Exceptions;

namespace Domain.Aggregates.Orders;

public class Order : SoftDeletableEntity
{
    private Order() { } // for EF
    private readonly List<OrderDetial> _orderDetails = new();

    public Order(int userId, DateTime orderDate, DateTime shipedDate, string shippingAddress)
    {
        UserId = userId;
        OrderDate = orderDate;
        ShipedDate = shipedDate;
        ShippingAddress = shippingAddress;
    }
    public int UserId { get; }

    public DateTime OrderDate { get; }
    public DateTime ShipedDate { get; }
    public string ShippingAddress { get; }

    internal IReadOnlyCollection<OrderDetial> OrderDetails => _orderDetails.AsReadOnly();

    public void AddItem(int productId, decimal unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        _orderDetails.Add(new OrderDetial(productId, unitPrice, quantity));
        Touch();
    }

    public void ChangeItemQuantity(int productId, int quantity)
    {
        if (productId <= 0)
            throw new DomainException();

        var item = _orderDetails.FirstOrDefault(x => x.ProductId == productId);
        if (item is null)
            throw new DomainException("Product identifier must be greater than zero.");

        if (quantity <= 0)
        {
            _orderDetails.Remove(item);
            Touch();
            return;
        }

        item.ChangeQuantity(quantity);
        Touch();
    }
}