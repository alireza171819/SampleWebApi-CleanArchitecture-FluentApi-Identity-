
namespace ApplicationService.Dtos.Products;

public class PutProductDto
{
    public int Id { get; set; }
    public Guid UUId { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int UnitsInStock { get; set; }
}
