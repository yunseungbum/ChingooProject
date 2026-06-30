using System.ComponentModel.DataAnnotations;

namespace Chingoo.ViewModels
{
    public class MyPageViewModel
    {
        [Display(Name = "아이디")]
        public string LoginId { get; set; } = string.Empty;

        [Display(Name = "팀명")]
        public string TeamName { get; set; } = string.Empty;

        [Required(ErrorMessage = "이메일을 입력해주세요.")]
        [EmailAddress(ErrorMessage = "올바른 이메일을 입력해주세요.")]
        [Display(Name = "이메일")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "지역을 선택해주세요.")]
        [Display(Name = "지역")]
        public string Region { get; set; } = string.Empty;

        [Display(Name = "축구 온도")]
        public int SoccerTemperature { get; set; }

        [Display(Name = "가입일")]
        public DateTime CreatedAt { get; set; }

        public IEnumerable<string> Regions { get; set; } = new List<string>();

        [DataType(DataType.Password)]
        [Display(Name = "현재 비밀번호")]
        public string CurrentPassword { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 5, ErrorMessage = "비밀번호는 5자 이상 입력해 주세요.")]
        [DataType(DataType.Password)]
        [Display(Name = "새 비밀번호")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "새 비밀번호가 일치하지 않습니다.")]
        [Display(Name = "새 비밀번호 확인")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}