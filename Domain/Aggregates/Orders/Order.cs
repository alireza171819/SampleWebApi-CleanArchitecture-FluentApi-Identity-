using Domain.Common;
using Domain.Exceptions;

namespace Domain.Aggregates.Orders;

public class Order : SoftDeletableEntity
{
    private Order() { } // for EF
    private readonly List<OrderDetail> _orderDetails = new();

    public Order(int userId, DateTime orderDate, DateTime? shippedDate, string shippingAddress)
    {
        UserId = userId;
        OrderDate = orderDate;
        ShippedDate = shippedDate;
        ShippingAddress = shippingAddress;
    }
    public int UserId { get; }

    public DateTime OrderDate { get; private set; }
    public DateTime? ShippedDate { get; private set; }
    public string ShippingAddress { get; private set; }

    internal IReadOnlyCollection<OrderDetail> OrderDetails => _orderDetails.AsReadOnly();

    public void AddItem(int productId, decimal unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        var existingItem = _orderDetails.FirstOrDefault(x => x.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _orderDetails.Add(new OrderDetail(productId, unitPrice, quantity));
        }
        Touch();
    }

    public void RemoveItem(int productId)
    {
        if (productId <= 0)
            throw new DomainException("Product identifier must be greater than zero.");

        var item = _orderDetails.FirstOrDefault(x => x.ProductId == productId);

        if (item is null)
            throw new DomainException("Order item not found.");

        _orderDetails.Remove(item);

        Touch();
    }

    public void ChangeItemQuantity(int productId, int quantity)
    {
        if (productId <= 0)
            throw new DomainException("Product identity is invalid.");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero");

        var item = _orderDetails.FirstOrDefault(x => x.ProductId == productId);

        if (item is null)
            throw new DomainException("Order item not found.");

        item.ChangeQuantity(quantity);
        Touch();
    }

    public void ChangeShippingAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("Shipping address is invalid.");

        ShippingAddress = address;

        Touch();
    }

    public void Ship(DateTime shippedDate)
    {
        if (ShippedDate != null)
            throw new DomainException("Shipped Date is invalid.");

        ShippedDate = shippedDate;

        Touch();
    }
}