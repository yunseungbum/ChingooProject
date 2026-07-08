using System.ComponentModel.DataAnnotations;

namespace Chingoo.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string BoardType { get; set; } = string.Empty;

        public int BoardId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }

        public ICollection<Comment> Replies { get; set; } = new List<Comment>();

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
