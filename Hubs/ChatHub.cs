using System.Security.Claims;
using Chingoo.Data;
using Chingoo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext _db;

    public ChatHub(AppDbContext db)
    {
        _db = db;
    }

    public async Task JoinRoom(int roomId)
    {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(x => x.Id == roomId);

        if (room == null)
        {
            await NotifyCallerRoomClosedAsync();
            return;
        }

        if (!CanAccessRoom(room))
        {
            throw new HubException("채팅방에 입장할 권한이 없습니다.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(roomId));

        if (TryGetCurrentUserId(out var userId))
        {
            var readMessageIds = await MarkRoomMessagesAsReadAsync(roomId, userId);
            await NotifyMessagesReadAsync(roomId, userId, readMessageIds);
        }
    }

    public async Task JoinUserInbox()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            throw new HubException("로그인이 필요합니다.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetUserInboxGroupName(userId));
    }

    public async Task SendMessageToRoom(int roomId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (!TryGetCurrentUserId(out var senderId))
        {
            throw new HubException("로그인이 필요합니다.");
        }

        var room = await _db.ChatRooms
            .Include(x => x.Post)
            .Include(x => x.PostOwner)
            .Include(x => x.Participant)
            .FirstOrDefaultAsync(x => x.Id == roomId);

        if (room == null)
        {
            await NotifyCallerRoomClosedAsync();
            return;
        }

        if (!CanAccessRoom(room, senderId))
        {
            throw new HubException("채팅방에 메시지를 보낼 권한이 없습니다.");
        }

        var content = message.Trim();
        var chatMessage = new ChatMessage
        {
            ChatRoomId = roomId,
            SenderId = senderId,
            Content = content
        };

        _db.ChatMessages.Add(chatMessage);
        await _db.SaveChangesAsync();

        var senderName = GetSenderName();
        var sentAt = chatMessage.CreatedAt.ToString("HH:mm");

        await Clients.Group(GetGroupName(roomId))
            .SendAsync("ReceiveMessage", senderName, content, sentAt, chatMessage.Id, senderId);

        await NotifyChatRoomUpdatedAsync(room, content, chatMessage.CreatedAt);
    }

    public async Task MarkRoomAsRead(int roomId)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            throw new HubException("로그인이 필요합니다.");
        }

        var room = await _db.ChatRooms.FirstOrDefaultAsync(x => x.Id == roomId);

        if (room == null)
        {
            await NotifyCallerRoomClosedAsync();
            return;
        }

        if (!CanAccessRoom(room, userId))
        {
            throw new HubException("채팅방에 접근할 권한이 없습니다.");
        }

        var readMessageIds = await MarkRoomMessagesAsReadAsync(roomId, userId);
        await NotifyMessagesReadAsync(roomId, userId, readMessageIds);
    }

    private bool CanAccessRoom(ChatRoom room)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return false;
        }

        return CanAccessRoom(room, userId);
    }

    private static bool CanAccessRoom(ChatRoom room, int userId)
    {
        return room.PostOwnerId == userId || room.ParticipantId == userId;
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out userId);
    }

    private string GetSenderName()
    {
        var teamName = Context.User?.FindFirst("TeamName")?.Value;

        if (!string.IsNullOrWhiteSpace(teamName))
        {
            return teamName;
        }

        return Context.User?.Identity?.Name ?? "익명";
    }

    public static string GetGroupName(int roomId)
    {
        return $"chat-room-{roomId}";
    }

    public static string GetUserInboxGroupName(int userId)
    {
        return $"user-inbox-{userId}";
    }

    private async Task<List<int>> MarkRoomMessagesAsReadAsync(int roomId, int readerId)
    {
        var unreadMessages = await _db.ChatMessages
            .Where(x => x.ChatRoomId == roomId &&
                        x.SenderId != readerId &&
                        x.ReadAt == null)
            .ToListAsync();

        if (!unreadMessages.Any())
        {
            return new List<int>();
        }

        var readAt = DateTime.Now;

        foreach (var message in unreadMessages)
        {
            message.ReadAt = readAt;
        }

        await _db.SaveChangesAsync();
        return unreadMessages.Select(x => x.Id).ToList();
    }

    private async Task NotifyMessagesReadAsync(int roomId, int readerId, List<int> readMessageIds)
    {
        if (!readMessageIds.Any())
        {
            return;
        }

        await Clients.Group(GetGroupName(roomId))
            .SendAsync("MessagesRead", readerId, readMessageIds);
    }

    private async Task NotifyCallerRoomClosedAsync()
    {
        await Clients.Caller.SendAsync("RoomClosed", "채팅방이 종료되었습니다.");
    }

    private async Task NotifyChatRoomUpdatedAsync(ChatRoom room, string lastMessage, DateTime lastMessageAt)
    {
        await Clients.Group(GetUserInboxGroupName(room.PostOwnerId))
            .SendAsync("ChatRoomUpdated", BuildChatRoomListPayload(room, room.PostOwnerId, lastMessage, lastMessageAt));

        await Clients.Group(GetUserInboxGroupName(room.ParticipantId))
            .SendAsync("ChatRoomUpdated", BuildChatRoomListPayload(room, room.ParticipantId, lastMessage, lastMessageAt));
    }

    private static object BuildChatRoomListPayload(ChatRoom room, int userId, string lastMessage, DateTime lastMessageAt)
    {
        return new
        {
            roomId = room.Id,
            recipientTeamName = room.PostOwnerId == userId
                ? room.Participant.TeamName
                : room.PostOwner.TeamName,
            postTitle = room.Post.Title,
            lastMessage,
            lastMessageAt = lastMessageAt.ToString("MM.dd HH:mm")
        };
    }
}
