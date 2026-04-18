namespace D.A.sneaker.DTOs
{
    public class ProductCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }

        // Sale fields
        public decimal? SalePrice { get; set; }
        public int DiscountPercent { get; set; }
        public string? PromoName { get; set; }
    }
}
