namespace D.A.sneaker.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string Role { get; set; } = "Customer"; // Admin | Staff | Customer
        public bool Status { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Customer? Customer { get; set; }
        public ICollection<CartItem>? CartItems { get; set; }

        public ICollection<Wishlist>? Wishlists { get; set; }

        public ICollection<Review>? Reviews { get; set; }
    }
}
