using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using BlogShare.Web.Data;
using BlogShare.Web.Models;
using System.Linq;

namespace BlogShare.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly AppDbContext _context;

        public BaseController(AppDbContext context)
        {
            _context = context;
        }

        protected int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        protected User? CurrentUser
        {
            get
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null) return null;
                return _context.Users.FirstOrDefault(u => u.Id == userId);
            }
        }

        protected string CurrentUserName => CurrentUser?.FullName ?? "";
        protected string CurrentUserRole => HttpContext.Session.GetString("UserRole") ?? "";
    }
}
