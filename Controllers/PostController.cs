using System.Security.Claims;
using Chingoo.Data;
using Chingoo.Hubs;
using Chingoo.Services.Comments;
using Chingoo.Services.Posts;
using Chingoo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly AppDbContext _db;
        private readonly IHubContext<ChatHub> _hubContext;

        public PostController(
            IPostService postService,
            ICommentService commentService,
            AppDbContext db,
            IHubContext<ChatHub> hubContext)
        {
            _postService = postService;
            _commentService = commentService;
            _db = db;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(_postService.GetCreateViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(PostCreateViewModel model)
        {
            NormalizeTeamRecruitMatchDate(model);

            if (!ModelState.IsValid)
            {
                var fixedModel = _postService.GetCreateViewModel();
                model.Days = fixedModel.Days;
                model.Regions = fixedModel.Regions;
                model.Times = fixedModel.Times;

                return View(model);
            }

            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            await _postService.CreatePostAsync(model, userId);

            return RedirectToAction(nameof(Index), new
            {
                boardType = model.BoardType
            });
        }

        public IActionResult Index(string boardType, string day, string region, string time)
        {
            ViewBag.BoardType = boardType;
            ViewBag.Day = day;
            ViewBag.Region = region;
            ViewBag.Time = time;

            var posts = _postService.GetPosts(boardType, day, region, time);

            return View(posts);
        }

        public async Task<IActionResult> Details(int id, string commentSort = "oldest")
        {
            var post = await _postService.GetPostDetailsAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            ViewBag.CommentSort = commentSort == "latest" ? "latest" : "oldest";
            ViewBag.Comments = await _commentService.GetCommentsAsync("Post", id);

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment(int postId, string content, int? parentCommentId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction(nameof(Details), new { id = postId });
            }

            var post = await _postService.GetPostDetailsAsync(postId);

            if (post == null)
            {
                return NotFound();
            }

            if (parentCommentId.HasValue)
            {
                if (!await _postService.CanWriteReplyAsync(postId, userId))
                {
                    return Forbid();
                }

                var parentExists = await _commentService.ParentCommentExistsAsync("Post", postId, parentCommentId.Value);

                if (!parentExists)
                {
                    return NotFound();
                }
            }

            await _commentService.CreateCommentAsync("Post", postId, userId, content, parentCommentId);

            return RedirectToAction(nameof(Details), new { id = postId });
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdValue, out userId);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var model = await _postService.GetEditViewModelAsync(id, userId);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PostCreateViewModel model)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            NormalizeTeamRecruitMatchDate(model);

            if (!ModelState.IsValid)
            {
                var fixedModel = _postService.GetCreateViewModel();
                model.Days = fixedModel.Days;
                model.Regions = fixedModel.Regions;
                model.Times = fixedModel.Times;

                return View(model);
            }

            var updated = await _postService.UpdatePostAsync(id, model, userId);

            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var chatRooms = await _db.ChatRooms
                .Where(x => x.PostId == id && x.PostOwnerId == userId)
                .Select(x => new ChatRoomCloseTarget(x.Id, x.PostOwnerId, x.ParticipantId))
                .ToListAsync();

            var deleted = await _postService.DeletePostAsync(id, userId);

            if (!deleted)
            {
                return NotFound();
            }

            await NotifyPostChatRoomsClosedAsync(chatRooms);

            return RedirectToAction(nameof(Index));
        }

        private async Task NotifyPostChatRoomsClosedAsync(IEnumerable<ChatRoomCloseTarget> chatRooms)
        {
            foreach (var room in chatRooms)
            {
                await _hubContext.Clients.Group(ChatHub.GetGroupName(room.RoomId))
                    .SendAsync("RoomClosed", "게시글이 삭제되었습니다.");

                await _hubContext.Clients.Group(ChatHub.GetUserInboxGroupName(room.PostOwnerId))
                    .SendAsync("ChatRoomRemoved", room.RoomId);

                await _hubContext.Clients.Group(ChatHub.GetUserInboxGroupName(room.ParticipantId))
                    .SendAsync("ChatRoomRemoved", room.RoomId);

                await NotifyUnreadChatCountAsync(room.PostOwnerId);
                await NotifyUnreadChatCountAsync(room.ParticipantId);
            }
        }

        private sealed record ChatRoomCloseTarget(int RoomId, int PostOwnerId, int ParticipantId);

        private async Task NotifyUnreadChatCountAsync(int userId)
        {
            var unreadCount = await _db.ChatMessages.CountAsync(x =>
                x.SenderId != userId &&
                x.ReadAt == null &&
                (x.ChatRoom.PostOwnerId == userId ||
                 x.ChatRoom.ParticipantId == userId));

            await _hubContext.Clients.Group(ChatHub.GetUserInboxGroupName(userId))
                .SendAsync("UnreadChatCountUpdated", unreadCount);
        }

        private void NormalizeTeamRecruitMatchDate(PostCreateViewModel model)
        {
            if (model.BoardType == "팀원 모집")
            {
                model.MatchDate = DateTime.Today;
                ModelState.Remove(nameof(model.MatchDate));
            }
        }
        public IActionResult Manage(string filter = "전체")
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var model = _postService.GetManageViewModel(userId, filter);

            return View(model);
        }
    }
}
