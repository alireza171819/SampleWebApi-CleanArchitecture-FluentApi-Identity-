using Domain.Common;

namespace Domain.Aggregates.Products;

public class Product : SoftDeletableEntity
{
    private Product() { } // for EF

    public Product(string name, decimal price, int stock)
    {
        ProductName = name;
        UnitPrice = price;
        UnitPrice = stock;
    }

    public string ProductName { get; } = default!;
    public decimal UnitPrice { get; }
    public int UnitsInStock { get; }

}
