using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogShare.Web.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Content { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool? IsApproved { get; set; }

        // Foreign key
        public int AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public User? Author { get; set; }

        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public string? ImagePath { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public string? RejectReason { get; set; } // Lý do từ chối nếu có
        public List<Comment> Comments { get; set; } = new();

        public List<Like> Likes { get; set; } = new();

    }
}
