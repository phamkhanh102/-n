using D.A.sneaker.Models;

public class CartItem
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int VariantId { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public User User { get; set; }

    public ProductVariant Variant { get; set; }
}