
namespace ApplicationService.Dtos.Products;

public class ProductCreateDto
{
    public Guid UUId { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int UnitsInStock { get; set; }
}
