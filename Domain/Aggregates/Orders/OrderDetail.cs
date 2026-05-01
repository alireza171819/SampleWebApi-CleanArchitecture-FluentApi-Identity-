using Domain.Common;
using Domain.Exceptions;

namespace Domain.Aggregates.Orders;

internal class OrderDetial : AuditableEntity
{
    private OrderDetial() { } // for EF
    internal OrderDetial(int productId, decimal unitPrice, int quantity)
    {
        ProductId = productId;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public int ProductId { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    internal void ChangeQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        Quantity = quantity;
    }
}
