namespace D.A.sneaker.Models
{
    public class UserChatState
    {
        public int Id { get; set; }
        public int? UserId { get; set; }

        public int? CurrentProductId { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}