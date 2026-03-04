namespace D.A.sneaker.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string Method { get; set; } = "COD";
        // COD | VNPAY | MOMO

        public decimal Amount { get; set; }

        public string Status { get; set; } = "Pending";
        // Pending | Paid | Failed

        public string? TransactionCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? PaidAt { get; set; }
    }
}