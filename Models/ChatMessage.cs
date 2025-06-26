namespace BlogShare.Web.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public int ThreadId { get; set; }
        public ChatThread Thread { get; set; } = default!;

        public int SenderId { get; set; }
        public User Sender { get; set; } = default!;

        public string Content { get; set; } = "";
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }


}
