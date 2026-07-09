using System.ComponentModel.DataAnnotations;

namespace Chingoo.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string LoginId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TeamName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [StringLength(50)]
        public string Region { get; set; } = string.Empty;

        [StringLength(20)]
        public string PreferredDayType { get; set; } = "상관없음";

        [StringLength(20)]
        public string PreferredTimeSlot { get; set; } = "상관없음";

        public int SoccerTemperature { get; set; } = 36;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //게시글
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
//[Required] = 반드시 값이 있어야 함
 

