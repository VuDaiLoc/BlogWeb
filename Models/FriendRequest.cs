using BlogShare.Web.Models;


namespace BlogShare.Web.Models
{
    public class FriendRequest
    {
        public int Id { get; set; }

        public int SenderId { get; set; }
        public int ReceiverId { get; set; }

        public DateTime SentAt { get; set; }
        public string Status { get; set; } = "Pending";

        // Thêm navigation property
        public User Sender { get; set; } = default!;
        public User Receiver { get; set; } = default!;
    }   

}
