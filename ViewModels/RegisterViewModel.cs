using System.ComponentModel.DataAnnotations;

namespace Chingoo.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "아이디를 입력해 주세요.")]
    [StringLength(30, MinimumLength = 4, ErrorMessage = "아이디는 4~30자로 입력해 주세요.")]
    [Display(Name = "아이디")]
    public string LoginId { get; set; } = string.Empty;

    [Required(ErrorMessage = "팀명을 입력해 주세요.")]
    [StringLength(50)]
    [Display(Name = "팀명")]
    public string TeamName { get; set; } = string.Empty;

    [Required(ErrorMessage = "이메일을 입력해 주세요.")]
    [EmailAddress(ErrorMessage = "올바른 이메일 주소를 입력해 주세요.")]
    [Display(Name = "이메일")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "비밀번호를 입력해 주세요.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "비밀번호는 5자 이상 입력해 주세요.")]
    [DataType(DataType.Password)]
    [Display(Name = "비밀번호")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "비밀번호 확인을 입력해 주세요.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "비밀번호가 일치하지 않습니다.")]
    [Display(Name = "비밀번호 확인")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "지역")]
    public string Region { get; set; } = string.Empty;

    [Required(ErrorMessage = "활동 가능 요일을 선택해 주세요.")]
    [StringLength(20)]
    [Display(Name = "활동 가능 요일")]
    public string PreferredDayType { get; set; } = "상관없음";

    [Required(ErrorMessage = "선호 시간대를 선택해 주세요.")]
    [StringLength(20)]
    [Display(Name = "선호 시간대")]
    public string PreferredTimeSlot { get; set; } = "상관없음";
}
