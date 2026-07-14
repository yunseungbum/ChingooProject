using Chingoo.Models;

namespace Chingoo.ViewModels
{
    public class ChatRoomViewModel
    {
        public int RoomId { get; set; }

        public string PostTitle { get; set; } = string.Empty;

        public string RecipientTeamName { get; set; } = string.Empty;

        public int CurrentUserId { get; set; }

        public List<ChatMessageViewModel> Messages { get; set; } = new();
    }

    public class ChatMessageViewModel
    {
        public int Id { get; set; }

        public int SenderId { get; set; }

        public string SenderTeamName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool IsMine { get; set; }

        public bool IsRead { get; set; }

        public static ChatMessageViewModel FromMessage(ChatMessage message, int currentUserId)
        {
            return new ChatMessageViewModel
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderTeamName = message.Sender.TeamName,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                IsMine = message.SenderId == currentUserId,
                IsRead = message.ReadAt.HasValue
            };
        }
    }
}
