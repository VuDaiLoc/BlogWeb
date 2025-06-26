using BlogShare.Web.Models;
using System.ComponentModel.DataAnnotations.Schema;


namespace BlogShare.Web.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Message { get; set; } = "";

        public string? Link { get; set; } // Đường dẫn khi click vào

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}

