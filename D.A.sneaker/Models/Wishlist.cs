namespace D.A.sneaker.Models
{
    public class Wishlist
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int ProductId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User User { get; set; }

        public Product Product { get; set; }
    }
}
