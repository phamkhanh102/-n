namespace D.A.sneaker.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }

        public string CustomerType { get; set; } = "New"; // New | Loyal | VIP
        public decimal TotalSpent { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User User { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
