namespace D.A.sneaker.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string ImageUrl { get; set; }

        public Product Product { get; set; }
    }
}