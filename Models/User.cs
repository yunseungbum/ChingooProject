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

        public decimal SoccerTemperature { get; set; } = 36.0m;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
//[Required] = 반드시 값이 있어야 함
 

