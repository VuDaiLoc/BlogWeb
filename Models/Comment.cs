    using BlogShare.Web.Models;

    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int AuthorId { get; set; }                 // 🆕 thêm AuthorId
        public User Author { get; set; } = default!;
        public string Content { get; set; } = string.Empty!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Post Post { get; set; } = default!;
    }
