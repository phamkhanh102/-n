using D.A.sneaker.Models;

public class ProductVariant
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int SizeId { get; set; }

    public int ColorId { get; set; }

    public int Stock { get; set; }

    public Product Product { get; set; }

    public Size Size { get; set; }

    public Color Color { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; }

    public ICollection<CartItem> CartItems { get; set; }
}