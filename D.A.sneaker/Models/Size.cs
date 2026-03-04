namespace D.A.sneaker.Models
{
    public class Size
    {
        public int Id { get; set; }
        public int Number { get; set; }

        public ICollection<ProductVariant> Variants { get; set; }
    }
}
