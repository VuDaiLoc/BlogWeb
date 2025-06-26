using BlogShare.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogShare.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }

        public DbSet<Friendship> Friendships { get; set; }

        public DbSet<FriendRequest> FriendRequests { get; set; }

        public DbSet<ChatThread> ChatThreads { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<QrLoginRequest> qrLoginRequests { get; set; }

        // Tạm thời chưa khai báo DbSet, sẽ thêm sau


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Like - User
            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.NoAction); // tránh cascade vòng lặp

            // Like - Post
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Friendship - User1
            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.User1)
                .WithMany()
                .HasForeignKey(f => f.User1Id)
                .OnDelete(DeleteBehavior.Restrict); // ⚠️ Không cascade

            // Friendship - User2
            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.User2)
                .WithMany()
                .HasForeignKey(f => f.User2Id)
                .OnDelete(DeleteBehavior.Restrict); // ⚠️ Không cascade

            modelBuilder.Entity<FriendRequest>()
        .HasOne(fr => fr.Sender)
        .WithMany()
        .HasForeignKey(fr => fr.SenderId)
        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Receiver)
                .WithMany()
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatThread>()
               .HasOne(t => t.User1)
               .WithMany()
               .HasForeignKey(t => t.User1Id)
               .OnDelete(DeleteBehavior.Restrict); // ✅ Không cascade

            modelBuilder.Entity<ChatThread>()
                .HasOne(t => t.User2)
                .WithMany()
                .HasForeignKey(t => t.User2Id)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Không cascade

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
