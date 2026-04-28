using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhAdmin.Models;
using VinhKhanhAdmin.Models.ViewModels;
using VinhKhanhAdmin.Services;

namespace VinhKhanhAdmin.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class ApiAuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;
    private readonly ILogger<ApiAuthController> _logger;

    public ApiAuthController(AppDbContext db, JwtService jwt, ILogger<ApiAuthController> logger)
    {
        _db = db;
        _jwt = jwt;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] ApiLoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "Username and password required" });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username && u.IsActive);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { error = "Sai tên đăng nhập hoặc mật khẩu" });

        user.LastLoginAt = DateTime.UtcNow;

        // Auto-register device if provided
        if (!string.IsNullOrWhiteSpace(req.DeviceId))
        {
            var dev = await _db.ActiveDevices.FirstOrDefaultAsync(d => d.DeviceId == req.DeviceId);
            if (dev == null)
            {
                _db.ActiveDevices.Add(new ActiveDevice
                {
                    DeviceId    = req.DeviceId,
                    UserId      = user.Id,
                    Platform    = req.Platform,
                    LastPingUtc = DateTime.UtcNow
                });
            }
            else
            {
                dev.UserId      = user.Id;
                dev.Platform    = req.Platform ?? dev.Platform;
                dev.LastPingUtc = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();

        var token = _jwt.CreateToken(user);
        return Ok(new
        {
            token,
            user = new { user.Id, user.Username, user.DisplayName, user.Email, user.Role }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] ApiRegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "Username and password required" });

        if (req.Password.Length < 6)
            return BadRequest(new { error = "Password must be at least 6 characters" });

        if (await _db.Users.AnyAsync(u => u.Username == req.Username))
            return Conflict(new { error = "Username already exists" });

        var user = new User
        {
            Username     = req.Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 11),
            DisplayName  = string.IsNullOrWhiteSpace(req.DisplayName) ? null : req.DisplayName.Trim(),
            Email        = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim(),
            Role         = "user",
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwt.CreateToken(user);
        return Ok(new
        {
            token,
            user = new { user.Id, user.Username, user.DisplayName, user.Email, user.Role }
        });
    }
}
