using D.A.sneaker.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Brand { get; set; }

    public decimal Price { get; set; }

    public decimal CostPrice { get; set; } = 0; // Giá nhập

    public int CategoryId { get; set; }

    public Category Category { get; set; }

    public string Description { get; set; }

    public string MainImage { get; set; }

    public bool IsActive { get; set; } = true;

    public int SoldCount { get; set; } = 0;

    public double Rating { get; set; } = 0;

    public ICollection<ProductImage> Images { get; set; }

    public ICollection<ProductVariant> Variants { get; set; }

    public ICollection<Review> Reviews { get; set; }
}