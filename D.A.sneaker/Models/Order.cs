using D.A.sneaker.Models;

public class Order
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
    public string CustomerName { get; set; }
    public string Address { get; set; }
    public string Ward { get; set; }
    public string District { get; set; }
    public string Province { get; set; }
    public string Phone { get; set; }
    public decimal TotalAmount { get; set; }

    public string Status { get; set; }

    public Payment? Payment { get; set; }

    public ICollection<OrderItem> Items { get; set; }
}