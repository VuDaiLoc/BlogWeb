using System.Diagnostics;
using BlogShare.Web.Data;
using BlogShare.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogShare.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, AppDbContext context) : base(context)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                var posts = _context.Posts
                    .Include(p => p.Comments)
                    .Include(p => p.Likes)
                    .Where(p => p.IsApproved == true)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                return View("~/Views/Post/All.cshtml", posts);
            }

            return RedirectToAction("Welcome");
        }

        public IActionResult Welcome()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Details(int id)
        {
            var post = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefault(p => p.Id == id);

            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        public IActionResult Comment(int postId, string content)
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Account");

            var comment = new Comment
            {
                PostId = postId,
                Content = content,
                AuthorId = CurrentUserId.Value,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = postId });
        }

        [HttpPost]
        public IActionResult Like(int postId)
        {
            if (CurrentUserId == null)
                return RedirectToAction("Login", "Account");

            var existingLike = _context.Likes.FirstOrDefault(l => l.UserId == CurrentUserId && l.PostId == postId);
            if (existingLike == null)
            {
                var like = new Like
                {
                    UserId = CurrentUserId.Value,
                    PostId = postId,
                    CreatedAt = DateTime.Now
                };
                _context.Likes.Add(like);
                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = postId });
        }

        [HttpGet]
        public IActionResult SearchUsers(string term)
        {
            var results = _context.Users
                .Where(u => u.FullName.Contains(term) || u.UserName.Contains(term))
                .Select(u => new { u.Id, u.FullName, u.UserName })
                .Take(10)
                .ToList();

            return Json(results);
        }
    }
}