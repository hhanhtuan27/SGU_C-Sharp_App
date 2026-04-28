using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhAdmin.Models;
using VinhKhanhAdmin.Models.ViewModels;

namespace VinhKhanhAdmin.Controllers;

[Authorize(Roles = "admin")]
public class UsersController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<UsersController> _logger;

    public UsersController(AppDbContext db, ILogger<UsersController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.ToLower();
            query = query.Where(u => u.Username.ToLower().Contains(kw)
                                  || (u.DisplayName != null && u.DisplayName.ToLower().Contains(kw)));
        }
        ViewData["Query"] = q;
        var users = await query.OrderByDescending(u => u.LastLoginAt).ToListAsync();
        return View(users);
    }

    [HttpGet]
    public IActionResult Create() => View("Form", new UserFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.Password))
            ModelState.AddModelError(nameof(vm.Password), "Mật khẩu là bắt buộc khi tạo mới");

        if (await _db.Users.AnyAsync(u => u.Username == vm.Username))
            ModelState.AddModelError(nameof(vm.Username), "Tên đăng nhập đã tồn tại");

        if (!ModelState.IsValid) return View("Form", vm);

        var user = new User
        {
            Username     = vm.Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password!, workFactor: 11),
            DisplayName  = string.IsNullOrWhiteSpace(vm.DisplayName) ? null : vm.DisplayName.Trim(),
            Email        = string.IsNullOrWhiteSpace(vm.Email) ? null : vm.Email.Trim(),
            Role         = vm.Role,
            IsActive     = vm.IsActive,
            CreatedAt    = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã thêm user \"{user.Username}\"";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        return View("Form", new UserFormViewModel
        {
            Id          = user.Id,
            Username    = user.Username,
            DisplayName = user.DisplayName,
            Email       = user.Email,
            Role        = user.Role,
            IsActive    = user.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (await _db.Users.AnyAsync(u => u.Id != id && u.Username == vm.Username))
            ModelState.AddModelError(nameof(vm.Username), "Tên đăng nhập đã tồn tại");

        if (!ModelState.IsValid) return View("Form", vm);

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.Username    = vm.Username.Trim();
        user.DisplayName = string.IsNullOrWhiteSpace(vm.DisplayName) ? null : vm.DisplayName.Trim();
        user.Email       = string.IsNullOrWhiteSpace(vm.Email) ? null : vm.Email.Trim();
        user.Role        = vm.Role;
        user.IsActive    = vm.IsActive;

        if (!string.IsNullOrWhiteSpace(vm.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password, workFactor: 11);

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã cập nhật user \"{user.Username}\"";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        if (user.Username == "admin")
        {
            TempData["Error"] = "Không thể vô hiệu hóa tài khoản admin gốc";
            return RedirectToAction("Index");
        }

        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();

        TempData["Success"] = user.IsActive
            ? $"Đã kích hoạt \"{user.Username}\""
            : $"Đã vô hiệu hóa \"{user.Username}\"";
        return RedirectToAction("Index");
    }
}
