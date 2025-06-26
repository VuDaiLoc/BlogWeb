using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogShare.Web.Models
{
    public class ChatThread
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";

        public int User1Id { get; set; }
        public int User2Id { get; set; }

        [ForeignKey("User1Id")]
        public User? User1 { get; set; }

        [ForeignKey("User2Id")]
        public User? User2 { get; set; }

        public List<ChatMessage> Messages { get; set; } = new();
    }
}
