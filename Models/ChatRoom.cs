namespace Chingoo.Models
{
    public class ChatRoom
    {
        public int Id { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public int PostOwnerId { get; set; }
        public User PostOwner { get; set; } = null!;

        public int ParticipantId { get; set; }
        public User Participant { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
