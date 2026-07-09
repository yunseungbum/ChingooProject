using System.Security.Claims;
using Chingoo.Models;
using Chingoo.ViewModels;

namespace Chingoo.Services.Accounts
{
    public interface IAccountService
    {
        Task RegisterAsync(RegisterViewModel model);
        (bool Available, string Message) CheckLoginId(string loginId);
        Task<(User? User, string? ErrorMessage)> ValidateLoginAsync(LoginViewModel model);
        IEnumerable<Claim> CreateUserClaims(User user);
        Task<MyPageViewModel?> GetMyPageViewModelAsync(int userId);
        Task<(bool Success, User? User, string? ErrorMessage)> UpdateMyPageAsync(int userId, MyPageViewModel model);
        Task FillMyPageFixedFieldsAsync(int userId, MyPageViewModel model);
    }
}
