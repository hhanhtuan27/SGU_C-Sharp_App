using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhAdmin.Models;
using VinhKhanhAdmin.Models.ViewModels;

namespace VinhKhanhAdmin.Controllers.Api;

[ApiController]
[Route("api/ping")]
public class ApiPingController : ControllerBase
{
    private readonly AppDbContext _db;
    public ApiPingController(AppDbContext db) => _db = db;

    /// <summary>POST /api/ping — upsert device heartbeat (no auth — devices may be anonymous)</summary>
    [HttpPost]
    public async Task<IActionResult> Ping([FromBody] ApiPingRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.DeviceId))
            return BadRequest(new { error = "DeviceId required" });

        var dev = await _db.ActiveDevices.FirstOrDefaultAsync(d => d.DeviceId == req.DeviceId);
        if (dev == null)
        {
            _db.ActiveDevices.Add(new ActiveDevice
            {
                DeviceId    = req.DeviceId,
                UserId      = req.UserId,
                Platform    = req.Platform,
                AppVersion  = req.AppVersion,
                LastPingUtc = DateTime.UtcNow
            });
        }
        else
        {
            dev.UserId      = req.UserId ?? dev.UserId;
            dev.Platform    = req.Platform ?? dev.Platform;
            dev.AppVersion  = req.AppVersion ?? dev.AppVersion;
            dev.LastPingUtc = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return Ok(new { ok = true, serverTime = DateTime.UtcNow });
    }
}

[ApiController]
[Route("api/narration")]
public class ApiNarrationController : ControllerBase
{
    private readonly AppDbContext _db;
    public ApiNarrationController(AppDbContext db) => _db = db;

    /// <summary>POST /api/narration/log — called by mobile app after TTS plays</summary>
    [HttpPost("log")]
    public async Task<IActionResult> Log([FromBody] ApiNarrationLogRequest req)
    {
        var allowed = new[] { "Vietnamese", "English", "Japanese", "Korean", "Chinese" };
        if (!allowed.Contains(req.Language))
            return BadRequest(new { error = "Invalid language. Use: Vietnamese|English|Japanese|Korean|Chinese" });

        if (!await _db.Pois.AnyAsync(p => p.Id == req.PoiId))
            return NotFound(new { error = "POI not found" });

        _db.NarrationLogs.Add(new NarrationLog
        {
            PoiId    = req.PoiId,
            DeviceId = req.DeviceId,
            Language = req.Language,
            PlayedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return Ok(new { ok = true });
    }
}
