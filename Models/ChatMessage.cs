namespace Chingoo.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public int ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; } = null!;

        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReadAt { get; set; }
    }
}
