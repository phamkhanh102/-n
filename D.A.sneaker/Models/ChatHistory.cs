namespace D.A.sneaker.Models
{
    public class ChatHistory
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public string Message { get; set; }

        public string Response { get; set; }
        public int? ContextProductId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
