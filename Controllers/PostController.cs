using BlogShare.Web.Data;
using BlogShare.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogShare.Web.Controllers
{
    public class PostController : BaseController
    {
        public PostController(AppDbContext context) : base(context) { }

        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Post post, IFormFile? imageFile)
        {
            if (CurrentUserId == null)
                return RedirectToAction("Login", "Account");

            post.AuthorId = CurrentUserId.Value;
            post.IsApproved = null;
            post.CreatedAt = DateTime.Now;

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                post.ImagePath = "/uploads/" + fileName;
            }

            _context.Posts.Add(post);
            _context.SaveChanges();

            TempData["StatusMessage"] = "✅ Đăng bài thành công!";
            return RedirectToAction("MyPosts");
        }

        public IActionResult MyPosts()
        {
            if (CurrentUserId == null)
                return RedirectToAction("Login", "Account");

            var posts = _context.Posts
                .Where(p => p.AuthorId == CurrentUserId)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return View(posts);
        }

        public IActionResult Details(int id)
        {
            var post = _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .FirstOrDefault(p => p.Id == id);

            if (post == null)
                return NotFound();

            return View(post);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
                return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            return View(post);
        }

        [HttpPost]
        public IActionResult Edit(int id, Post model, IFormFile image)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();

            post.Title = model.Title;
            post.Content = model.Content;
            post.CategoryId = model.CategoryId;
            post.UpdatedAt = DateTime.Now;

            if (image != null && image.Length > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }

                post.ImagePath = "/uploads/" + fileName;
            }

            _context.SaveChanges();
            TempData["StatusMessage"] = "✅ Cập nhật bài viết thành công!";
            return RedirectToAction("MyPosts");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
                return NotFound();

            return View(post);
        }

        [HttpPost]
        public IActionResult ConfirmDelete(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();

            if (!string.IsNullOrEmpty(post.ImagePath))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Posts.Remove(post);
            _context.SaveChanges();

            TempData["StatusMessage"] = "Xóa bài thành công!";
            return RedirectToAction("MyPosts");
        }

        [HttpGet]
        public IActionResult Pending()
        {
            var pendingPosts = _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Where(p => p.IsApproved == null)
                .ToList();

            return View(pendingPosts);
        }

        [HttpPost]
        public IActionResult Approve(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();

            post.IsApproved = true;
            post.UpdatedAt = DateTime.Now;

            var noti = new Notification
            {
                UserId = post.AuthorId,
                Message = $"✅ Bài viết \"{post.Title}\" của bạn đã được duyệt.",
                IsRead = false,
                CreatedAt = DateTime.Now,
                Link = Url.Action("Details", "Post", new { id = post.Id })
            };
            _context.Notifications.Add(noti);

            _context.SaveChanges();
            TempData["StatusMessage"] = "Đã duyệt bài viết.";
            return RedirectToAction("Pending");
        }

        [HttpPost]
        public IActionResult Reject(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();

            post.IsApproved = false;
            post.UpdatedAt = DateTime.Now;

            var noti = new Notification
            {
                UserId = post.AuthorId,
                Message = $"❌ Bài viết \"{post.Title}\" của bạn đã bị từ chối.",
                IsRead = false,
                CreatedAt = DateTime.Now,
                Link = Url.Action("Details", "Post", new { id = post.Id })
            };
            _context.Notifications.Add(noti);

            _context.SaveChanges();
            TempData["StatusMessage"] = "Đã từ chối bài viết.";
            return RedirectToAction("Pending");
        }

        public IActionResult All()
        {
            var posts = _context.Posts
                .Where(p => p.IsApproved == true)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return View(posts);
        }
    }
}