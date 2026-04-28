using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhAdmin.Models;
using VinhKhanhAdmin.Models.ViewModels;

namespace VinhKhanhAdmin.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext db, ILogger<AuthController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["HideChrome"] = true;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        ViewData["HideChrome"] = true;
        if (!ModelState.IsValid) return View(vm);

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == vm.Username && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(vm.Password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
            return View(vm);
        }

        if (user.Role != "admin")
        {
            ModelState.AddModelError("", "Tài khoản không có quyền truy cập admin panel");
            return View(vm);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.Username),
            new(ClaimTypes.Role,           user.Role),
            new("DisplayName",             user.DisplayName ?? user.Username)
        };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12) });

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Admin {Username} logged in", user.Username);

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
