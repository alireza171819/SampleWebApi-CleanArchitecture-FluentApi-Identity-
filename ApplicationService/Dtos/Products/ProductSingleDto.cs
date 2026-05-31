
namespace ApplicationService.Dtos.Products;

public class ProductSingleDto
{
    public int Id { get; set; }
    public Guid Uuid { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int UnitsInStock { get; set; }
}
