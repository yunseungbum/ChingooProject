using System.Security.Claims;
using Chingoo.Services.Accounts;
using Chingoo.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chingoo.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
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

        await _accountService.RegisterAsync(model);

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult CheckLoginId(string loginId)
    {
        var result = _accountService.CheckLoginId(loginId);

        return Json(new
        {
            available = result.Available,
            message = result.Message
        });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.ValidateLoginAsync(model);

        if (result.User == null)
        {
            TempData["Message"] = result.ErrorMessage;
            return View(model);
        }

        var claims = _accountService.CreateUserClaims(result.User);
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
        if (!TryGetCurrentUserId(out var userId))
        {
            return RedirectToAction("Login");
        }

        var model = await _accountService.GetMyPageViewModelAsync(userId);

        if (model == null)
        {
            return RedirectToAction("Login");
        }

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MyPage(MyPageViewModel model)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return RedirectToAction("Login");
        }

        await _accountService.FillMyPageFixedFieldsAsync(userId, model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.UpdateMyPageAsync(userId, model);

        if (result.User == null)
        {
            return RedirectToAction("Login");
        }

        if (!result.Success)
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), result.ErrorMessage ?? "내정보 수정에 실패했습니다.");
            await _accountService.FillMyPageFixedFieldsAsync(userId, model);
            return View(model);
        }

        var claims = _accountService.CreateUserClaims(result.User);
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

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out userId);
    }
}
