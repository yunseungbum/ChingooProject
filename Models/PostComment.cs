using System.ComponentModel.DataAnnotations;

namespace Chingoo.Models
{
    public class PostComment
    {
        public int Id { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int? ParentCommentId { get; set; }
        public PostComment? ParentComment { get; set; }

        public ICollection<PostComment> Replies { get; set; } = new List<PostComment>();

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}