
namespace ApplicationService.Dtos.Orders;

public class CreateOrderDto
{
    public Guid Uuid { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ShipedDate { get; set; }
    public string? ShipAddress { get; set; }

    public List<SingleOrderDetailDto>? OrderDetailsDtos { get; set; }
}
