using Microsoft.AspNetCore.Mvc;
using BlogShare.Web.Data;
using BlogShare.Web.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.Data;

namespace BlogShare.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AccountController(AppDbContext context, IWebHostEnvironment webHostEnvironment) : base(context)
        {
            _webHostEnvironment = webHostEnvironment; // ✅ Inject đúng cách
        }

        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
                return View(user);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.Role = "User";
            user.AvatarFileName = "male.png";

            _context.Users.Add(user);
            _context.SaveChanges();
            TempData["StatusMessage"] = "Đăng ký tài khoản thành công!";
            return RedirectToAction("Login");
        }

        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Message = "Sai email hoặc mật khẩu";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role);

            TempData["StatusMessage"] = "Đăng nhập thành công!";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public IActionResult PublicProfile(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            var posts = _context.Posts
                .Where(p => p.AuthorId == id && p.IsApproved == true)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            bool isFriend = false;
            bool alreadySentRequest = false;
            if (CurrentUserId != null)
            {
                isFriend = _context.Friendships.Any(f =>
                    (f.User1Id == CurrentUserId && f.User2Id == id) ||
                    (f.User2Id == CurrentUserId && f.User1Id == id));

                alreadySentRequest = _context.FriendRequests.Any(r =>
                    r.SenderId == CurrentUserId && r.ReceiverId == id && r.Status == "Pending");
            }

            ViewBag.User = user;
            ViewBag.UserPosts = posts;
            ViewBag.IsFriend = isFriend;
            ViewBag.AlreadySentRequest = alreadySentRequest;

            return View("Profile", user);
        }

        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
        {
            if (CurrentUserId == null || avatarFile == null || avatarFile.Length == 0)
                return RedirectToAction("PublicProfile", new { id = CurrentUserId });

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatar");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatarFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            var user = CurrentUser;
            if (user != null)
            {
                user.AvatarFileName = "/uploads/avatar/" + fileName; // ✅ CHUẨN
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("PublicProfile", new { id = CurrentUserId });
        }

        [HttpGet]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (CurrentUser == null) return RedirectToAction("Login");

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, CurrentUser.PasswordHash))
            {
                TempData["StatusMessage"] = "❌ Mật khẩu hiện tại không đúng.";
                return View();
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["StatusMessage"] = "❌ Mật khẩu mới không khớp.";
                return View();
            }

            CurrentUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.SaveChanges();

            TempData["StatusMessage"] = "✅ Đổi mật khẩu thành công.";
            return RedirectToAction("ChangePassword");
        }

        public IActionResult Settings() => View();

        [HttpGet]
        public IActionResult SearchUsers(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Content("");

            var users = _context.Users
                .Where(u => u.FullName.Contains(keyword))
                .OrderBy(u => u.FullName)
                .Take(8)
                .ToList();

            return PartialView("_UserSearchResults", users);
        }


        public IActionResult QRCode() => View();

        [HttpGet]
        public IActionResult CheckStatus()
        {
            return Json(new { loggedIn = HttpContext.Session.GetInt32("UserId") != null });
        }


        [HttpPost]
        public IActionResult QrCallback([FromBody] QrLoginRequest request)
        {
            var expectedToken = HttpContext.Session.GetString("QR_SESSION");
            if (request.Token != expectedToken)
                return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Id == request.UserId);
            if (user == null) return NotFound();

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("FullName", user.FullName);
            return Ok(new { message = "Đăng nhập thành công" });
        }
    }
}