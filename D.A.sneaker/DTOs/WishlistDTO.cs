namespace D.A.sneaker.DTOs
{
    public class AddWishlistDTO
    {
        public int ProductId { get; set; }
    }

    public class WishlistItemDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public WishlistProductDTO Product { get; set; }
    }

    public class WishlistProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public string MainImage { get; set; }
    }
}
