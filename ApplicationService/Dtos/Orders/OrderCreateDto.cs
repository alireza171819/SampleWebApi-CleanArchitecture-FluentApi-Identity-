
namespace ApplicationService.Dtos.Orders;

public class OrderCreateDto
{
    public Guid Uuid { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ShipedDate { get; set; }
    public string? ShipAddress { get; set; }

    public List<OrderDetailSingleDto>? OrderDetailsDtos { get; set; }
}
