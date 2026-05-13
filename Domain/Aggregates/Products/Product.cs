using Domain.Common;
using Domain.Exceptions;

namespace Domain.Aggregates.Products;

public class Product : SoftDeletableEntity
{
    private Product() { } // for EF

    public Product(string name, decimal price, int stock)
    {
        SetName(name);
        ChangePrice(price);
        IncreaseStock(stock);
    }

    public string ProductName { get; private set; } = default!;
    public decimal UnitPrice { get; private set; }
    public int UnitsInStock { get; private set; }

    public void SetName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new DomainException("Product name is required.");

        ProductName = name;
        Touch();
    }

    public void ChangePrice(decimal price)
    {
        if (price <= 0)
            throw new DomainException("Price must be greater than zero.");

        UnitPrice = price;
        Touch();
    }

    public void IncreaseStock(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Stock increment must be positive.");

        UnitsInStock += amount;
        Touch();
    }

    public void DecreaseStock(int amount)
    {
        if (amount <= 0)
            throw new DomainException();

        if (UnitsInStock < amount)
            throw new DomainException("Insufficient stock.");

        UnitsInStock -= amount;
        Touch();
    }
}
