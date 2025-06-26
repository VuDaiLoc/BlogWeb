using BlogShare.Web.Data;
using BlogShare.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogShare.Web.Controllers
{
    public class NotificationController : BaseController
    {
        public NotificationController(AppDbContext context) : base(context) { }

        public IActionResult Index()
        {
            if (CurrentUserId == null)
                return RedirectToAction("Login", "Account");

            var notifications = _context.Notifications
                .Where(n => n.UserId == CurrentUserId.Value)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(notifications);
        }

        public IActionResult Details(int id)
        {
            if (CurrentUserId == null)
                return RedirectToAction("Login", "Account");

            var notification = _context.Notifications
                .FirstOrDefault(n => n.Id == id && n.UserId == CurrentUserId.Value);

            if (notification == null)
                return NotFound();

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                _context.SaveChanges();
            }

            return View(notification);
        }
    }
}
