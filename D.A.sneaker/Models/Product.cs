namespace D.A.sneaker.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string MainImage { get; set; }

        // Navigation
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}