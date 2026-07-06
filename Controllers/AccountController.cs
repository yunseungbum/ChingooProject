using Chingoo.Data;
using Chingoo.Models;
using Chingoo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Chingoo.Common;

namespace Chingoo.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    public AccountController(AppDbContext db)
    {
        _db = db;
    }

    public static IEnumerable<Claim> CreateUserClaims(User user)
    {
        return new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.LoginId),
            new Claim("TeamName", user.TeamName ?? ""),
            new Claim("Region", user.Region ?? "")
        };
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ShowErrorModal = true;
            return View(model);
        }

        var user = new User
        {
            LoginId = model.LoginId,
            TeamName = model.TeamName,
            Email = model.Email,


            Password = model.Password,
            Region = model.Region,
            SoccerTemperature = 36,
            CreatedAt = DateTime.Now
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult CheckLoginId(string loginId)
    {
        bool exists = _db.Users.Any(u => u.LoginId == loginId);

        if (exists)
        {
            return Json(new
            {
                available = false,
                message = "이미 사용 중인 아이디입니다."
            });
        }

        return Json(new
        {
            available = true,
            message = "사용 가능한 아이디입니다."
        });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = _db.Users.FirstOrDefault(u => u.LoginId == model.LoginId);

        if (user == null)
        {
            TempData["Message"] = "아이디를 다시 확인 해주세요.";
            return View(model);
        }
        if (user.Password != model.Password)
        {
            TempData["Message"] = "비밀번호가 올바르지 않습니다.";
            return View(model);
        }

        // 1. Claims 생성
        var claims = CreateUserClaims(user);

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        // 2. 쿠키 발급
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddHours(3)
            });

        TempData["Message"] = "로그인 되었습니다.";

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        TempData["Message"] = "로그아웃 되었습니다.";

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> MyPage()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var id))
        {
            return RedirectToAction("Login");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var model = new MyPageViewModel
        {
            LoginId = user.LoginId,
            TeamName = user.TeamName,
            Email = user.Email,
            Region = user.Region,
            SoccerTemperature = user.SoccerTemperature,
            CreatedAt = user.CreatedAt,
            Regions = BoardOptions.Regions
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MyPage(MyPageViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var id))
        {
            return RedirectToAction("Login");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        model.LoginId = user.LoginId;
        model.SoccerTemperature = user.SoccerTemperature;
        model.CreatedAt = user.CreatedAt;
        model.Regions = BoardOptions.Regions;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        user.TeamName = model.TeamName;
        user.Email = model.Email;
        user.Region = model.Region;

        if (!string.IsNullOrWhiteSpace(model.NewPassword) || !string.IsNullOrWhiteSpace(model.CurrentPassword) || !string.IsNullOrWhiteSpace(model.ConfirmNewPassword))
        {
            if (model.CurrentPassword != user.Password)
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "현재 비밀번호가 올바르지 않습니다.");
                model.Regions = BoardOptions.Regions;
                return View(model);
            }

            user.Password = model.NewPassword;
        }

        await _db.SaveChangesAsync();

        var claims = CreateUserClaims(user);
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddHours(3)
            });

        TempData["Message"] = "내정보가 수정되었습니다.";

        return RedirectToAction("MyPage");
    }
}
