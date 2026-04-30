using Domain.Common;

namespace Domain.Aggregates.Orders;

public class Order : SoftDeletableEntity
{
    private Order() { } // for EF
    private readonly List<OrderDetial> _orderDetails = new();

    public Order(int userId, int cartId, DateTime orderDate, DateTime shipedDate, string shippingAddress)
    {
        UserId = userId;
        CartId = cartId;
        OrderDate = orderDate;
        ShipedDate = shipedDate;
        ShippingAddress = shippingAddress;
    }
    public int UserId { get; }
    public int CartId { get; }

    public DateTime OrderDate { get; }
    public DateTime ShipedDate { get; }
    public string ShippingAddress { get; }

    internal IReadOnlyCollection<OrderDetial> OrderDetails => _orderDetails.AsReadOnly();
}