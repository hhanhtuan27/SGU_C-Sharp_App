using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhAdmin.Models;
using VinhKhanhAdmin.Models.ViewModels;

namespace VinhKhanhAdmin.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var now      = DateTime.UtcNow;
        var today    = now.Date;
        var since30  = now.AddDays(-30);
        var since5m = now.AddSeconds(-90);

        var totalPois   = await _db.Pois.CountAsync();
        var activePois  = await _db.Pois.CountAsync(p => p.IsActive);
        var totalUsers  = await _db.Users.CountAsync(u => u.IsActive);
        var online      = await _db.ActiveDevices.CountAsync(d => d.LastPingUtc >= since5m);
        var playsToday  = await _db.NarrationLogs.CountAsync(n => n.PlayedAt >= today);
        var plays30     = await _db.NarrationLogs.CountAsync(n => n.PlayedAt >= since30);

        // Top 10 POIs by play count (all-time)
        var topPois = await _db.NarrationLogs
            .GroupBy(n => n.PoiId)
            .Select(g => new { PoiId = g.Key, PlayCount = g.Count(), UniqueDevices = g.Select(x => x.DeviceId).Distinct().Count() })
            .OrderByDescending(x => x.PlayCount)
            .Take(10)
            .ToListAsync();

        var topPoiIds = topPois.Select(x => x.PoiId).ToList();
        var topPoiDetails = await _db.Pois
            .Where(p => topPoiIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.Category, p.ImageUrl })
            .ToListAsync();

        var topPoiItems = topPois
            .Select(t =>
            {
                var d = topPoiDetails.FirstOrDefault(x => x.Id == t.PoiId);
                return new TopPoiItem
                {
                    Id = t.PoiId,
                    Name = d?.Name ?? "(đã xoá)",
                    Category = d?.Category ?? "Khac",
                    ImageUrl = d?.ImageUrl,
                    PlayCount = t.PlayCount,
                    UniqueDevices = t.UniqueDevices
                };
            }).ToList();

        // Category distribution
        var categoryCounts = await _db.Pois
            .Where(p => p.IsActive)
            .GroupBy(p => p.Category)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        // Language distribution (30d)
        var languageCounts = await _db.NarrationLogs
            .Where(n => n.PlayedAt >= since30)
            .GroupBy(n => n.Language)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        // Daily plays (last 30d)
        var daily = await _db.NarrationLogs
            .Where(n => n.PlayedAt >= since30)
            .GroupBy(n => n.PlayedAt.Date)
            .Select(g => new DailyPlayItem { PlayDate = g.Key, PlayCount = g.Count() })
            .OrderBy(x => x.PlayDate)
            .ToListAsync();

        // Recent POIs
        var recent = await _db.Pois
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Take(5)
            .Select(p => new RecentPoiItem
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                IsActive = p.IsActive,
                UpdatedAt = p.UpdatedAt ?? p.CreatedAt
            })
            .ToListAsync();

        var vm = new DashboardViewModel
        {
            TotalPois       = totalPois,
            ActivePois      = activePois,
            TotalUsers      = totalUsers,
            OnlineDevices   = online,
            PlaysToday      = playsToday,
            Plays30Days     = plays30,
            TopPois         = topPoiItems,
            CategoryCounts  = categoryCounts,
            LanguageCounts  = languageCounts,
            DailyPlays      = daily,
            RecentPois      = recent
        };

        return View(vm);
    }

    /// <summary>AJAX: online devices count (called every 30s from client)</summary>
    [HttpGet]
    public async Task<IActionResult> OnlineCount()
    {
        // 2 phút = nếu app tắt, sau 2 phút biến mất
        var since = DateTime.UtcNow.AddMinutes(-2);
        var devices = await _db.ActiveDevices
            .Where(d => d.LastPingUtc >= since)
            .Select(d => new
            {
                d.DeviceId,
                d.Platform,
                d.AppVersion,
                seconds = (int)(DateTime.UtcNow - d.LastPingUtc).TotalSeconds
            })
            .OrderByDescending(d => d.seconds)
            .ToListAsync();

        return Json(new { count = devices.Count, devices });
    }

    [AllowAnonymous]
    public IActionResult Error() => View();
}
