using BlogShare.Web.Data;
using BlogShare.Web.Models;
using Microsoft.AspNetCore.SignalR;

namespace BlogShare.Web.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context) // ✅ Bổ sung constructor
        {
            _context = context;
        }

        public async Task SendMessage(string threadId, string userId, string content)
        {
            try
            {
                if (!int.TryParse(threadId, out int threadInt) || !int.TryParse(userId, out int senderInt))
                {
                    Console.WriteLine("❌ Lỗi chuyển đổi threadId hoặc userId sang int.");
                    return;
                }

                var user = await _context.Users.FindAsync(senderInt);
                if (user == null)
                {
                    Console.WriteLine("❌ Không tìm thấy người dùng.");
                    return;
                }

                var avatar = user.AvatarFileName ?? "default.png";
                var fullName = user.FullName ?? "Ẩn danh";

                // Nếu content null hoặc quá dài
                if (string.IsNullOrWhiteSpace(content) || content.Length > 5000)
                {
                    Console.WriteLine("❌ Nội dung tin nhắn không hợp lệ.");
                    return;
                }

                // Gửi tới group
                await Clients.Group(threadId).SendAsync("ReceiveMessage", userId, content, threadId, fullName, avatar);

                // Lưu DB
                var msg = new ChatMessage
                {
                    ThreadId = threadInt,
                    SenderId = senderInt,
                    Content = content,
                    SentAt = DateTime.Now
                };

                _context.ChatMessages.Add(msg);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 Lỗi SendMessage SignalR: " + ex.Message);
                // Không throw để tránh mất kết nối
            }
        }

        public async Task JoinThread(string threadId)
        {
            try
            {
                var connectionId = Context.ConnectionId;
                Console.WriteLine($"[JOIN] thread {threadId} - {connectionId}");

                if (string.IsNullOrEmpty(connectionId))
                {
                    Console.WriteLine("❌ ConnectionId is null!");
                    throw new Exception("ConnectionId is null");
                }

                await Groups.AddToGroupAsync(connectionId, threadId);
                Console.WriteLine("✅ Join success");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ JoinThread Error: " + ex.Message);
                throw; // QUAN TRỌNG: Đừng nuốt lỗi
            }
        }

        public async Task SendTyping(string threadId, string userId, string userName)
        {
            await Clients.OthersInGroup(threadId).SendAsync("ReceiveTyping", threadId, userName);
        }

    }
}
