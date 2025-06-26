namespace BlogShare.Web.Models
{
    public class Friendship
    {
        public int Id { get; set; }
        public int User1Id { get; set; }
        public int User2Id { get; set; }
        public DateTime ConnectedAt { get; set; }

        // Optional: Navigation properties
        public User User1 { get; set; } = null!;
        public User User2 { get; set; } = null!;
    }
}
