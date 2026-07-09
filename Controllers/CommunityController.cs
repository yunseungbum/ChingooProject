using System.Security.Claims;
using Chingoo.Models;
using Chingoo.Services.Comments;
using Chingoo.Services.Communities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chingoo.Controllers
{
    public class CommunityController : Controller
    {
        private readonly ICommunityService _communityService;
        private readonly ICommentService _commentService;

        public CommunityController(ICommunityService communityService, ICommentService commentService)
        {
            _communityService = communityService;
            _commentService = commentService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _communityService.GetPostsAsync());
        }

        public async Task<IActionResult> Details(int id, string commentSort = "oldest")
        {
            var post = await _communityService.GetPostAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            ViewBag.CommentSort = commentSort == "latest" ? "latest" : "oldest";
            ViewBag.Comments = await _commentService.GetCommentsAsync("Community", id);

            return View(post);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment(int communityPostId, string content, int? parentCommentId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction(nameof(Details), new { id = communityPostId });
            }

            var post = await _communityService.GetPostAsync(communityPostId);

            if (post == null)
            {
                return NotFound();
            }

            if (parentCommentId.HasValue)
            {
                if (!await _communityService.CanWriteReplyAsync(communityPostId, userId))
                {
                    return Forbid();
                }

                var parentExists = await _commentService.ParentCommentExistsAsync("Community", communityPostId, parentCommentId.Value);

                if (!parentExists)
                {
                    return NotFound();
                }
            }

            await _commentService.CreateCommentAsync("Community", communityPostId, userId, content, parentCommentId);

            return RedirectToAction(nameof(Details), new { id = communityPostId });
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommunityPost communityPost)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            ModelState.Remove(nameof(communityPost.User));

            if (!ModelState.IsValid)
            {
                return View(communityPost);
            }

            await _communityService.CreatePostAsync(communityPost, userId);

            return RedirectToAction(nameof(Index));
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdValue, out userId);
        }
    }
}
