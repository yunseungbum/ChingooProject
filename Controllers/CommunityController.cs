using System.Security.Claims;
using Chingoo.Data;
using Chingoo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Controllers
{
    public class CommunityController : Controller
    {
        private readonly AppDbContext _db;

        public CommunityController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _db.CommunityPosts
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        public async Task<IActionResult> Details(int id, string commentSort = "oldest")
        {
            var post = await _db.CommunityPosts
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            ViewBag.CommentSort = commentSort == "latest" ? "latest" : "oldest";
            ViewBag.Comments = await _db.Comments
                .Include(x => x.User)
                .Include(x => x.Replies)
                    .ThenInclude(x => x.User)
                .Where(x => x.BoardType == "Community" && x.BoardId == id)
                .ToListAsync();

            return View(post);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment(int communityPostId, string content, int? parentCommentId)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction(nameof(Details), new { id = communityPostId });
            }

            var post = await _db.CommunityPosts.FirstOrDefaultAsync(x => x.Id == communityPostId);

            if (post == null)
            {
                return NotFound();
            }

            if (parentCommentId.HasValue)
            {
                if (post.UserId != userId)
                {
                    return Forbid();
                }

                var parentExists = await _db.Comments
                    .AnyAsync(x => x.Id == parentCommentId.Value && x.BoardType == "Community" && x.BoardId == communityPostId);

                if (!parentExists)
                {
                    return NotFound();
                }
            }

            var comment = new Comment
            {
                BoardType = "Community",
                BoardId = communityPostId,
                UserId = userId,
                Content = content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.Now
            };

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

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
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            ModelState.Remove(nameof(communityPost.User));

            if (!ModelState.IsValid)
            {
                return View(communityPost);
            }

            communityPost.UserId = userId;
            communityPost.CreatedAt = DateTime.Now;

            _db.CommunityPosts.Add(communityPost);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
