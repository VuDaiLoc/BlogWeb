using BlogShare.Web.Controllers;
using BlogShare.Web.Data;
using BlogShare.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class FriendController : BaseController
{
    public FriendController(AppDbContext context) : base(context) { }

    // Gửi lời mời kết bạn
    [HttpPost]
    public IActionResult AddFriend(int userId)
    {
        if (CurrentUserId == null) return RedirectToAction("Login", "Account");

        bool alreadySent = _context.FriendRequests.Any(r =>
            r.SenderId == CurrentUserId && r.ReceiverId == userId && r.Status == "Pending");

        bool alreadyFriend = _context.Friendships.Any(f =>
            (f.User1Id == CurrentUserId && f.User2Id == userId) ||
            (f.User2Id == CurrentUserId && f.User1Id == userId));

        if (!alreadySent && !alreadyFriend && CurrentUserId != userId)
        {
            _context.FriendRequests.Add(new FriendRequest
            {
                SenderId = CurrentUserId.Value,
                ReceiverId = userId,
                SentAt = DateTime.Now,
                Status = "Pending"
            });
            _context.SaveChanges();
        }

        return RedirectToAction("PublicProfile", "Account", new { id = userId });
    }

    // Hiển thị danh sách bạn bè
    public IActionResult MyFriends()
    {
        if (CurrentUserId == null) return RedirectToAction("Login", "Account");

        var friends = _context.Friendships
            .Where(f => f.User1Id == CurrentUserId || f.User2Id == CurrentUserId)
            .Include(f => f.User1)
            .Include(f => f.User2)
            .ToList();

        return View(friends);
    }

    // Huỷ kết bạn
    [HttpPost]
    public IActionResult RemoveFriend(int userId)
    {
        if (CurrentUserId == null) return RedirectToAction("Login", "Account");

        var friendship = _context.Friendships.FirstOrDefault(f =>
            (f.User1Id == CurrentUserId && f.User2Id == userId) ||
            (f.User2Id == CurrentUserId && f.User1Id == userId));

        if (friendship != null)
        {
            _context.Friendships.Remove(friendship);
            _context.SaveChanges();
        }

        return RedirectToAction("PublicProfile", "Account", new { id = userId });
    }

    // Hiển thị danh sách lời mời kết bạn đến
    public IActionResult IncomingRequests()
    {
        if (CurrentUserId == null) return RedirectToAction("Login", "Account");

        var requests = _context.FriendRequests
            .Where(r => r.ReceiverId == CurrentUserId && r.Status == "Pending")
            .Include(r => r.Sender)
            .ToList();

        return View(requests);
    }

    // Chấp nhận lời mời kết bạn
    [HttpPost]
    public IActionResult AcceptRequest(int requestId)
    {
        var request = _context.FriendRequests.FirstOrDefault(r => r.Id == requestId);

        if (request != null && request.Status == "Pending")
        {
            request.Status = "Accepted";
            _context.Friendships.Add(new Friendship
            {
                User1Id = request.SenderId,
                User2Id = request.ReceiverId,
                ConnectedAt = DateTime.Now
            });
            _context.SaveChanges();
        }

        return RedirectToAction("IncomingRequests");
    }

    // Từ chối lời mời kết bạn
    [HttpPost]
    public IActionResult RejectRequest(int requestId)
    {
        var request = _context.FriendRequests.FirstOrDefault(r => r.Id == requestId);

        if (request != null && request.Status == "Pending")
        {
            request.Status = "Rejected";
            _context.SaveChanges();
        }

        return RedirectToAction("IncomingRequests");
    }
}
