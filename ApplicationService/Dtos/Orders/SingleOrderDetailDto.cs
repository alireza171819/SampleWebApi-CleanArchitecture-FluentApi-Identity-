
namespace ApplicationService.Dtos.Orders;

public class SingleOrderDetailDto
{
    public Guid UUId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int UnitPrice { get; set; }
    public int Quantity { get; set; }
}
