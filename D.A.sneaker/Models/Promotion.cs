namespace D.A.sneaker.Models
{
    public class Promotion
    {
        public int Id { get; set; }

        /// <summary>Tên chương trình khuyến mãi</summary>
        public string Name { get; set; } = "";

        /// <summary>Mô tả ngắn</summary>
        public string Description { get; set; } = "";

        /// <summary>Giảm theo phần trăm (0-100). Nếu > 0 thì ưu tiên dùng %</summary>
        public int DiscountPercent { get; set; } = 0;

        /// <summary>Giảm cố định (VNĐ). Dùng khi DiscountPercent = 0</summary>
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>Ngày bắt đầu</summary>
        public DateTime StartDate { get; set; }

        /// <summary>Ngày kết thúc</summary>
        public DateTime EndDate { get; set; }

        /// <summary>Kích hoạt hay không</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Ảnh banner (URL)</summary>
        public string BannerImage { get; set; } = "";

        /// <summary>Danh sách ProductId áp dụng (JSON array string, null = áp dụng tất cả)</summary>
        public string? ProductIds { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
