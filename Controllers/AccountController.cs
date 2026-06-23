using Chingoo.Data;
using Chingoo.Models;
using Chingoo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Chingoo.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    public AccountController(AppDbContext db)
    {
        _db = db;
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
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.LoginId),
            new Claim("TeamName", user.TeamName ?? "")
        };

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
}
