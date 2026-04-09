namespace D.A.sneaker.DTOs
{
    public class CreateOrderDTO
    {
        public int CustomerId { get; set; }

        public List<CreateOrderItemDTO> Items { get; set; }

        public string Address { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string Province { get; set; }
        public string? Phone { get; set; }
        public string? CustomerName { get; set; }

        /// <summary>Số tiền giảm giá từ coupon (VNĐ)</summary>
        public decimal DiscountAmount { get; set; } = 0;
        /// <summary>Mã coupon đã áp dụng</summary>
        public string? CouponCode { get; set; }
    }


    public class CreateOrderItemDTO
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
    }
}
