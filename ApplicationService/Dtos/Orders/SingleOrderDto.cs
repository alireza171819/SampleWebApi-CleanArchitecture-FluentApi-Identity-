
namespace ApplicationService.Dtos.Orders;

public class SingleOrderDto
{
    public int Id { get; set; }
    public Guid UUId { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ShipedDate { get; set; }
    public string ShipAddress { get; set; }
}
