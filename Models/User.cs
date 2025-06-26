namespace BlogShare.Web.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string Role { get; set; } = "User"; // hoặc "Admin"

        public List<Like> Likes { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserName { get; set; } = ""; // 🔹 BẮT BUỘC
        public string? AvatarFileName { get; set; } = "default.png"; // ảnh mặc định

    }
}
