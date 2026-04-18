namespace D.A.sneaker.DTOs
{
    public class ProductDetailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string MainImage { get; set; }

        public List<string> Images { get; set; }
        public List<VariantDTO> Variants { get; set; }

        // Sale fields
        public decimal? SalePrice { get; set; }
        public int DiscountPercent { get; set; }
        public string? PromoName { get; set; }
    }

    public class VariantDTO
    {
        public int VariantId { get; set; }
        public int Size { get; set; }
        public string Color { get; set; }
        public int Stock { get; set; }
    }
}