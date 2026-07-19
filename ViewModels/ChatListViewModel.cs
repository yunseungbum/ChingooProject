namespace Chingoo.ViewModels
{
    public class ChatListViewModel
    {
        public int CurrentUserId { get; set; }

        public List<ChatListItemViewModel> Rooms { get; set; } = new();
    }

    public class ChatListItemViewModel
    {
        public int RoomId { get; set; }

        public string RecipientTeamName { get; set; } = string.Empty;

        public string PostTitle { get; set; } = string.Empty;

        public string LastMessage { get; set; } = string.Empty;

        public DateTime? LastMessageAt { get; set; }

        public int UnreadCount { get; set; }
    }
}
