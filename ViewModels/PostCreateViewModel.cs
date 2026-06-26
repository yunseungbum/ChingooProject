using System.ComponentModel.DataAnnotations;

namespace Chingoo.ViewModels
{
    public class PostCreateViewModel
    {
        [Required(ErrorMessage = "게시글 위치를 선택해주세요.")]
        public string BoardType { get; set; } = string.Empty;

        [Required(ErrorMessage = "평일/주말을 선택해주세요.")]
        public string DayType { get; set; } = string.Empty;

        [Required(ErrorMessage = "지역을 선택해주세요.")]
        public string Region { get; set; } = string.Empty;

        [Required(ErrorMessage = "경기 날짜를 선택해주세요.")]
        public DateTime MatchDate { get; set; }

        [Required(ErrorMessage = "시간대를 선택해주세요.")]
        public string TimeSlot { get; set; } = string.Empty;

        [Required(ErrorMessage = "제목을 입력해주세요.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "내용을 입력해주세요.")]
        public string Content { get; set; } = string.Empty;

        public IEnumerable<string> Days { get; set; } = new List<string>();

        public IEnumerable<string> Regions { get; set; } = new List<string>();

        public IEnumerable<string> Times { get; set; } = new List<string>();
    }
}