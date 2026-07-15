
namespace ApplicationService.Dtos.Orders;

public class OrderDetailSingleDto
{
    public Guid Uuid { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
