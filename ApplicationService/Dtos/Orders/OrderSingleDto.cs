
namespace ApplicationService.Dtos.Orders;

public class OrderSingleDto
{
    public int Id { get; set; }
    public Guid Uuid { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ShippedDate { get; set; }
    public string ShippAddress { get; set; }
}
