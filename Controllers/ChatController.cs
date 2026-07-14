using System.Security.Claims;
using Chingoo.Data;
using Chingoo.Hubs;
using Chingoo.Models;
using Chingoo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly AppDbContext _db;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(AppDbContext db, IHubContext<ChatHub> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
    }

    public async Task<IActionResult> Index()
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return RedirectToAction("Login", "Account");
        }

        var rooms = await _db.ChatRooms
            .Include(x => x.Post)
            .Include(x => x.PostOwner)
            .Include(x => x.Participant)
            .Where(x => x.PostOwnerId == currentUserId || x.ParticipantId == currentUserId)
            .ToListAsync();

        var roomIds = rooms.Select(x => x.Id).ToList();
        var latestMessages = await _db.ChatMessages
            .Where(x => roomIds.Contains(x.ChatRoomId))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var latestMessageByRoomId = latestMessages
            .GroupBy(x => x.ChatRoomId)
            .ToDictionary(x => x.Key, x => x.First());

        var model = new ChatListViewModel
        {
            CurrentUserId = currentUserId,
            Rooms = rooms
                .Select(room =>
                {
                    if (!latestMessageByRoomId.TryGetValue(room.Id, out var latestMessage))
                    {
                        return null;
                    }

                    return new ChatListItemViewModel
                    {
                        RoomId = room.Id,
                        RecipientTeamName = room.PostOwnerId == currentUserId
                            ? room.Participant.TeamName
                            : room.PostOwner.TeamName,
                        PostTitle = room.Post.Title,
                        LastMessage = latestMessage?.Content ?? "아직 메시지가 없습니다.",
                        LastMessageAt = latestMessage?.CreatedAt ?? room.CreatedAt
                    };
                })
                .Where(x => x != null)
                .Select(x => x!)
                .OrderByDescending(x => x.LastMessageAt)
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartFromPost(int postId)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return RedirectToAction("Login", "Account");
        }

        var post = await _db.Posts
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == postId);

        if (post == null)
        {
            return NotFound();
        }

        if (post.UserId == currentUserId)
        {
            TempData["Message"] = "본인 게시글에는 채팅을 시작할 수 없습니다.";
            return RedirectToAction("Details", "Post", new { id = postId });
        }

        var room = await _db.ChatRooms
            .Include(x => x.Post)
            .Include(x => x.PostOwner)
            .Include(x => x.Participant)
            .FirstOrDefaultAsync(x =>
                x.PostId == post.Id &&
                x.PostOwnerId == post.UserId &&
                x.ParticipantId == currentUserId);

        var isNewRoom = room == null;

        if (isNewRoom)
        {
            room = new ChatRoom
            {
                PostId = post.Id,
                PostOwnerId = post.UserId,
                ParticipantId = currentUserId
            };

            _db.ChatRooms.Add(room);
            await _db.SaveChangesAsync();

            room = await _db.ChatRooms
                .Include(x => x.Post)
                .Include(x => x.PostOwner)
                .Include(x => x.Participant)
                .FirstAsync(x => x.Id == room.Id);
        }

        if (room == null)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Room), new { id = room.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Room(int id)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return RedirectToAction("Login", "Account");
        }

        var room = await _db.ChatRooms
            .Include(x => x.Post)
            .Include(x => x.PostOwner)
            .Include(x => x.Participant)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (room == null)
        {
            return NotFound();
        }

        if (room.PostOwnerId != currentUserId && room.ParticipantId != currentUserId)
        {
            return Forbid();
        }

        await MarkRoomMessagesAsReadAsync(room.Id, currentUserId);

        var recipientTeamName = room.PostOwnerId == currentUserId
            ? room.Participant.TeamName
            : room.PostOwner.TeamName;

        var chatMessages = await _db.ChatMessages
            .Include(x => x.Sender)
            .Where(x => x.ChatRoomId == room.Id)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        var model = new ChatRoomViewModel
        {
            RoomId = room.Id,
            PostTitle = room.Post.Title,
            RecipientTeamName = recipientTeamName,
            CurrentUserId = currentUserId,
            Messages = chatMessages
                .Select(message => ChatMessageViewModel.FromMessage(message, currentUserId))
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(int id)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return RedirectToAction("Login", "Account");
        }

        var room = await _db.ChatRooms.FirstOrDefaultAsync(x => x.Id == id);

        if (room == null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (room.PostOwnerId != currentUserId && room.ParticipantId != currentUserId)
        {
            return Forbid();
        }

        await _hubContext.Clients.Group(ChatHub.GetGroupName(room.Id))
            .SendAsync("RoomClosed", "상대방이 채팅방을 나갔습니다.");

        await _hubContext.Clients.Group(ChatHub.GetUserInboxGroupName(room.PostOwnerId))
            .SendAsync("ChatRoomRemoved", room.Id);
        await _hubContext.Clients.Group(ChatHub.GetUserInboxGroupName(room.ParticipantId))
            .SendAsync("ChatRoomRemoved", room.Id);

        _db.ChatRooms.Remove(room);
        await _db.SaveChangesAsync();

        TempData["Message"] = "채팅방을 나갔습니다.";
        return RedirectToAction(nameof(Index));
    }

    private async Task MarkRoomMessagesAsReadAsync(int roomId, int currentUserId)
    {
        var unreadMessages = await _db.ChatMessages
            .Where(x => x.ChatRoomId == roomId &&
                        x.SenderId != currentUserId &&
                        x.ReadAt == null)
            .ToListAsync();

        if (!unreadMessages.Any())
        {
            return;
        }

        var readAt = DateTime.Now;

        foreach (var message in unreadMessages)
        {
            message.ReadAt = readAt;
        }

        await _db.SaveChangesAsync();
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out userId);
    }
}
