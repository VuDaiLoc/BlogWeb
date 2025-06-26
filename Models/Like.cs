namespace BlogShare.Web.Models
{
    public class Like
    {
        public int Id { get; set; }

        // Khóa ngoại đến bài viết
        public int PostId { get; set; }
        public Post Post { get; set; } = default!;

        // Khóa ngoại đến người dùng
        public int UserId { get; set; }
        public User User { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
