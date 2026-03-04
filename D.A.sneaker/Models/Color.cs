namespace D.A.sneaker.Models
{
    public class Color
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<ProductVariant> Variants { get; set; }
    }
}
