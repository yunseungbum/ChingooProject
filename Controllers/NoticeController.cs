using System.Security.Claims;
using Chingoo.Data;
using Chingoo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Controllers
{
    public class NoticeController : Controller
    {
        private readonly AppDbContext _db;

        public NoticeController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var notices = await _db.Notices
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(notices);
        }

        public async Task<IActionResult> Details(int id, string commentSort = "oldest")
        {
            var notice = await _db.Notices.FirstOrDefaultAsync(x => x.Id == id);

            if (notice == null)
            {
                return NotFound();
            }

            ViewBag.CommentSort = commentSort == "latest" ? "latest" : "oldest";
            ViewBag.Comments = await _db.Comments
                .Include(x => x.User)
                .Include(x => x.Replies)
                    .ThenInclude(x => x.User)
                .Where(x => x.BoardType == "Notice" && x.BoardId == id)
                .ToListAsync();

            return View(notice);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment(int noticeId, string content, int? parentCommentId)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction(nameof(Details), new { id = noticeId });
            }

            var noticeExists = await _db.Notices.AnyAsync(x => x.Id == noticeId);

            if (!noticeExists)
            {
                return NotFound();
            }

            if (parentCommentId.HasValue)
            {
                if (User.Identity?.Name != "admin")
                {
                    return Forbid();
                }

                var parentExists = await _db.Comments
                    .AnyAsync(x => x.Id == parentCommentId.Value && x.BoardType == "Notice" && x.BoardId == noticeId);

                if (!parentExists)
                {
                    return NotFound();
                }
            }

            var comment = new Comment
            {
                BoardType = "Notice",
                BoardId = noticeId,
                UserId = userId,
                Content = content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.Now
            };

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = noticeId });
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            if (User.Identity?.Name != "admin")
            {
                return Forbid();
            }

            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notice notice)
        {
            if (User.Identity?.Name != "admin")
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(notice);
            }

            notice.CreatedAt = DateTime.Now;

            _db.Notices.Add(notice);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
