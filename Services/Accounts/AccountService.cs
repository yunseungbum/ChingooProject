using System.Security.Claims;
using Chingoo.Common;
using Chingoo.Data;
using Chingoo.Models;
using Chingoo.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Services.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _db;

        public AccountService(AppDbContext db)
        {
            _db = db;
        }

        public async Task RegisterAsync(RegisterViewModel model)
        {
            var user = new User
            {
                LoginId = model.LoginId,
                TeamName = model.TeamName,
                Email = model.Email,
                Password = model.Password,
                Region = model.Region,
                PreferredDayType = model.PreferredDayType,
                PreferredTimeSlot = model.PreferredTimeSlot,
                SoccerTemperature = 36,
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        public (bool Available, string Message) CheckLoginId(string loginId)
        {
            if (string.IsNullOrWhiteSpace(loginId) || loginId.Length < 4 || loginId.Length > 30)
            {
                return (false, "아이디는 4~30자로 입력해 주세요.");
            }

            var exists = _db.Users.Any(u => u.LoginId == loginId);

            if (exists)
            {
                return (false, "이미 사용 중인 아이디입니다.");
            }

            return (true, "사용 가능한 아이디입니다.");
        }

        public async Task<(User? User, string? ErrorMessage)> ValidateLoginAsync(LoginViewModel model)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.LoginId == model.LoginId);

            if (user == null)
            {
                return (null, "아이디를 다시 확인 해주세요.");
            }

            if (user.Password != model.Password)
            {
                return (null, "비밀번호가 올바르지 않습니다.");
            }

            return (user, null);
        }

        public IEnumerable<Claim> CreateUserClaims(User user)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.LoginId),
                new Claim("TeamName", user.TeamName ?? ""),
                new Claim("Region", user.Region ?? "")
            };
        }

        public async Task<MyPageViewModel?> GetMyPageViewModelAsync(int userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }

            return new MyPageViewModel
            {
                LoginId = user.LoginId,
                TeamName = user.TeamName,
                Email = user.Email,
                Region = user.Region,
                PreferredDayType = user.PreferredDayType,
                PreferredTimeSlot = user.PreferredTimeSlot,
                SoccerTemperature = user.SoccerTemperature,
                CreatedAt = user.CreatedAt,
                Regions = BoardOptions.Regions,
                Days = BoardOptions.Days,
                Times = BoardOptions.Times
            };
        }

        public async Task FillMyPageFixedFieldsAsync(int userId, MyPageViewModel model)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return;
            }

            model.LoginId = user.LoginId;
            model.TeamName = user.TeamName;
            model.SoccerTemperature = user.SoccerTemperature;
            model.CreatedAt = user.CreatedAt;
            model.Regions = BoardOptions.Regions;
            model.Days = BoardOptions.Days;
            model.Times = BoardOptions.Times;
        }

        public async Task<(bool Success, User? User, string? ErrorMessage)> UpdateMyPageAsync(int userId, MyPageViewModel model)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return (false, null, null);
            }

            user.TeamName = model.TeamName;
            user.Email = model.Email;
            user.Region = model.Region;
            user.PreferredDayType = model.PreferredDayType;
            user.PreferredTimeSlot = model.PreferredTimeSlot;

            var wantsPasswordChange =
                !string.IsNullOrWhiteSpace(model.NewPassword) ||
                !string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                !string.IsNullOrWhiteSpace(model.ConfirmNewPassword);

            if (wantsPasswordChange)
            {
                if (model.CurrentPassword != user.Password)
                {
                    return (false, user, "현재 비밀번호가 올바르지 않습니다.");
                }

                if (string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    return (false, user, "새 비밀번호를 입력해 주세요.");
                }

                user.Password = model.NewPassword;
            }

            await _db.SaveChangesAsync();

            return (true, user, null);
        }
    }
}
