using System.ComponentModel.DataAnnotations;

namespace Chingoo.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "아이디를 입력해주세요.")]
    [Display(Name = "아이디")]
    public string LoginId { get; set; } = string.Empty;

    [Required(ErrorMessage = "비밀번호를 입력해주세요.")]
    [DataType(DataType.Password)]
    [Display(Name = "비밀번호")]
    public string Password { get; set; } = string.Empty;
}