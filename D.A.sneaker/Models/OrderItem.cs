namespace D.A.sneaker.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int VariantId { get; set; }

        // snapshot dữ liệu tại thời điểm mua
        public string ProductName { get; set; }
        public string ColorName { get; set; }
        public int SizeNumber { get; set; }

        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
