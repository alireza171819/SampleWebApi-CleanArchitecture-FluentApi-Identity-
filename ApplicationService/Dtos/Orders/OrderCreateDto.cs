
namespace ApplicationService.Dtos.Orders;

public class OrderCreateDto
{
    public Guid Uuid { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public string ShippingAddress { get; set; }

    public List<OrderDetailSingleDto>? OrderDetails { get; set; }
}
