using Domain.Common;

namespace Domain.Aggregates.Orders;

public class OrderDetial : AuditableEntity
{
    private OrderDetial() { } // for EF
    internal OrderDetial(int productId, decimal unitPrice, int quantity)
    {
        ProductId = productId;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public int ProductId { get; }
    public decimal UnitPrice { get; }
    public int Quantity { get; }
}
