using System.Security.Claims;
using BlogShare.Web.Data;
using BlogShare.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace BlogShare.Web.Controllers
{
    public class ChatController : BaseController
    {
        public ChatController(AppDbContext context) : base(context) { }

        [HttpGet]
        public IActionResult StartThread(int receiverId)
        {
            if (CurrentUserId == null || receiverId == CurrentUserId)
                return Json(new { success = false });

            var thread = _context.ChatThreads
                .FirstOrDefault(t => t.User1Id == CurrentUserId && t.User2Id == receiverId ||
                                     t.User1Id == receiverId && t.User2Id == CurrentUserId);

            if (thread == null)
            {
                thread = new ChatThread
                {
                    User1Id = CurrentUserId.Value,
                    User2Id = receiverId,
                    Title = "Đoạn chat"
                };
                _context.ChatThreads.Add(thread);
                _context.SaveChanges();
            }

            return Json(new { success = true, threadId = thread.Id });
        }

        [HttpPost]
        public async Task<IActionResult> UploadChatImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Json(new { success = false, message = "Không có ảnh." });

                if (file.Length > 2 * 1024 * 1024)
                    return Json(new { success = false, message = "Ảnh quá lớn (tối đa 2MB)." });

                var ext = Path.GetExtension(file.FileName).ToLower();
                var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                if (!allowedExts.Contains(ext))
                    return Json(new { success = false, message = "Định dạng không hỗ trợ." });

                var fileName = Guid.NewGuid() + ext;
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/chat", fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Json(new { success = true, fileName });
            }
            catch (Exception ex)
            {
                Console.WriteLine("UploadChatImage Error: " + ex.Message);
                return Json(new { success = false, message = "Lỗi server khi tải ảnh." });
            }
        }

        [HttpGet]
        public IActionResult GetMessages(int threadId)
        {
            var messages = _context.ChatMessages
                .Where(m => m.ThreadId == threadId)
                .Include(m => m.Sender)
                .OrderBy(m => m.SentAt)
                .Select(m => new {
                    m.SenderId,
                    m.Content,
                    Time = m.SentAt.ToString("HH:mm"),
                    SenderName = m.Sender.FullName,
                    Avatar = m.Sender.AvatarFileName ?? "default.png"
                })
                .ToList();

            return Json(messages);
        }

        [HttpGet]
        public IActionResult GetThreads()
        {
            if (CurrentUserId == null) return Unauthorized();

            var threads = _context.ChatThreads
                .Where(t => t.User1Id == CurrentUserId || t.User2Id == CurrentUserId)
                .Include(t => t.User1)
                .Include(t => t.User2)
                .Include(t => t.Messages)
                .ToList() // 👉 chuyển ToList trước để tránh lỗi EF khi kiểm tra null navigation property
                .Where(t => t.User1 != null && t.User2 != null) // 👉 tránh dòng lỗi
                .Select(t => new
                {
                    id = t.Id,
                    title = (t.User1Id == CurrentUserId ? t.User2.FullName : t.User1.FullName) ?? "Đoạn chat",
                    unreadCount = t.Messages.Count(m => m.SenderId != CurrentUserId && !m.IsRead)
                })
                .ToList();

            return Json(threads);
        }


        [HttpPost]
        public IActionResult MarkAsRead(int threadId)
        {
            if (CurrentUserId == null) return Unauthorized();

            var unreadMessages = _context.ChatMessages
                .Where(m => m.ThreadId == threadId && m.SenderId != CurrentUserId && !m.IsRead)
                .ToList();

            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
            }

            _context.SaveChanges();

            return Ok();
        }
    }
}
