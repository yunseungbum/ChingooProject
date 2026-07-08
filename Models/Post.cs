using System.ComponentModel.DataAnnotations;

namespace Chingoo.Models
{
    public class Post
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string BoardType { get; set; } = string.Empty;

        public string DayType { get; set; } = string.Empty;

        public string Region { get; set; } = string.Empty;

        public DateTime MatchDate { get; set; }

        public string TimeSlot { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
