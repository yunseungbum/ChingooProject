using System.Security.Claims;
using Chingoo.Models;
using Chingoo.Services.Comments;
using Chingoo.Services.Notices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chingoo.Controllers
{
    public class NoticeController : Controller
    {
        private readonly INoticeService _noticeService;
        private readonly ICommentService _commentService;

        public NoticeController(INoticeService noticeService, ICommentService commentService)
        {
            _noticeService = noticeService;
            _commentService = commentService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _noticeService.GetNoticesAsync());
        }

        public async Task<IActionResult> Details(int id, string commentSort = "oldest")
        {
            var notice = await _noticeService.GetNoticeAsync(id);

            if (notice == null)
            {
                return NotFound();
            }

            ViewBag.CommentSort = commentSort == "latest" ? "latest" : "oldest";
            ViewBag.Comments = await _commentService.GetCommentsAsync("Notice", id);

            return View(notice);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment(int noticeId, string content, int? parentCommentId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction(nameof(Details), new { id = noticeId });
            }

            if (!await _noticeService.NoticeExistsAsync(noticeId))
            {
                return NotFound();
            }

            if (parentCommentId.HasValue)
            {
                if (User.Identity?.Name != "admin")
                {
                    return Forbid();
                }

                var parentExists = await _commentService.ParentCommentExistsAsync("Notice", noticeId, parentCommentId.Value);

                if (!parentExists)
                {
                    return NotFound();
                }
            }

            await _commentService.CreateCommentAsync("Notice", noticeId, userId, content, parentCommentId);

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

            await _noticeService.CreateNoticeAsync(notice);

            return RedirectToAction(nameof(Index));
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdValue, out userId);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (User.Identity?.Name != "admin")
            {
                return Forbid();
            }

            var notice = await _noticeService.GetNoticeAsync(id);

            if (notice == null)
            {
                return NotFound();
            }

            return View(notice);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Notice notice)
        {
            if (User.Identity?.Name != "admin")
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                notice.Id = id;
                return View(notice);
            }

            var updated = await _noticeService.UpdateNoticeAsync(id, notice);

            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (User.Identity?.Name != "admin")
            {
                return Forbid();
            }

            var deleted = await _noticeService.DeleteNoticeAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
