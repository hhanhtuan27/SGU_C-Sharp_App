using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhAdmin.Models;
using VinhKhanhAdmin.Models.ViewModels;
using VinhKhanhAdmin.Services;

namespace VinhKhanhAdmin.Controllers;

[Authorize]
public class PoisController : Controller
{
    private readonly AppDbContext _db;
    private readonly ImageService _images;
    private readonly ILogger<PoisController> _logger;

    public PoisController(AppDbContext db, ImageService images, ILogger<PoisController> logger)
    {
        _db = db;
        _images = images;
        _logger = logger;
    }

    // ================= LIST =================
    [HttpGet]
    public async Task<IActionResult> Index(string? q, string? cat, bool deleted = false, int page = 1)
    {
        var query = _db.Pois.AsNoTracking().AsQueryable();
        query = deleted ? query.Where(p => !p.IsActive) : query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(cat) && cat != "all")
            query = query.Where(p => p.Category == cat);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(kw));
        }

        var total = await query.CountAsync();
        const int pageSize = 15;
        page = Math.Max(1, page);

        var items = await query
            .OrderByDescending(p => p.Priority)
            .ThenByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PoiListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                RadiusMeters = p.RadiusMeters,
                Priority = p.Priority,
                ImageUrl = p.ImageUrl,
                DescriptionVi = p.DescriptionVi,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        var vm = new PoiListViewModel
        {
            Items = items,
            SearchQuery = q,
            CategoryFilter = cat ?? "all",
            ShowDeleted = deleted,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };

        return View(vm);
    }

    // ================= CREATE =================
    [HttpGet]
    public IActionResult Create() => View("Form", new PoiFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PoiFormViewModel vm)
    {
        if (!ModelState.IsValid) return View("Form", vm);

        try
        {
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
                vm.ImageUrl = await _images.UploadPoiImageAsync(vm.ImageFile, Request);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(nameof(vm.ImageFile), ex.Message);
            return View("Form", vm);
        }

        var poi = new PointOfInterest
        {
            Name = vm.Name?.Trim() ?? "",
            Category = vm.Category,
            Latitude = vm.Latitude,
            Longitude = vm.Longitude,
            RadiusMeters = vm.RadiusMeters,
            Priority = vm.Priority,
            DescriptionVi = vm.DescriptionVi?.Trim() ?? "",  // FIX: null-safe
            DescriptionEn = vm.DescriptionEn?.Trim() ?? "",  // FIX: null-safe
            DescriptionJa = NullIfBlank(vm.DescriptionJa),
            DescriptionKo = NullIfBlank(vm.DescriptionKo),
            DescriptionZh = NullIfBlank(vm.DescriptionZh),
            ImageUrl = NullIfBlank(vm.ImageUrl),
            Address = NullIfBlank(vm.Address),
            PhoneNumber = NullIfBlank(vm.PhoneNumber),
            OpeningHours = NullIfBlank(vm.OpeningHours),
            PriceRange = NullIfBlank(vm.PriceRange),
            GoogleMapsLink = NullIfBlank(vm.GoogleMapsLink),
            IsActive = vm.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            _db.Pois.Add(poi);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Created POI #{Id}: {Name}", poi.Id, poi.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo POI: {Name}", vm.Name);
            ModelState.AddModelError("", $"Lỗi lưu database: {ex.InnerException?.Message ?? ex.Message}");
            return View("Form", vm);
        }

        TempData["Success"] = $"Đã thêm POI \"{poi.Name}\" (ID: {poi.Id})";
        return RedirectToAction("Index");
    }

    // ================= EDIT =================
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var poi = await _db.Pois.FindAsync(id);
        if (poi == null) return NotFound();

        var vm = new PoiFormViewModel
        {
            Id = poi.Id,
            Name = poi.Name,
            Category = poi.Category,
            Latitude = poi.Latitude,
            Longitude = poi.Longitude,
            RadiusMeters = poi.RadiusMeters,
            Priority = poi.Priority,
            DescriptionVi = poi.DescriptionVi,
            DescriptionEn = poi.DescriptionEn,
            DescriptionJa = poi.DescriptionJa,
            DescriptionKo = poi.DescriptionKo,
            DescriptionZh = poi.DescriptionZh,
            ImageUrl = poi.ImageUrl,
            Address = poi.Address,
            PhoneNumber = poi.PhoneNumber,
            OpeningHours = poi.OpeningHours,
            PriceRange = poi.PriceRange,
            GoogleMapsLink = poi.GoogleMapsLink,
            IsActive = poi.IsActive
        };
        return View("Form", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PoiFormViewModel vm)
    {
        // FIX Bug 2: nếu Form.cshtml thiếu hidden field Id, vm.Id = 0 → dùng id từ route
        if (vm.Id == 0) vm.Id = id;
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Form", vm);

        var poi = await _db.Pois.FindAsync(id);
        if (poi == null) return NotFound();

        try
        {
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var newUrl = await _images.UploadPoiImageAsync(vm.ImageFile, Request);
                if (!string.IsNullOrWhiteSpace(newUrl))
                {
                    _images.DeletePoiImage(poi.ImageUrl);
                    vm.ImageUrl = newUrl;
                }
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(nameof(vm.ImageFile), ex.Message);
            return View("Form", vm);
        }

        poi.Name = vm.Name?.Trim() ?? "";
        poi.Category = vm.Category;
        poi.Latitude = vm.Latitude;
        poi.Longitude = vm.Longitude;
        poi.RadiusMeters = vm.RadiusMeters;
        poi.Priority = vm.Priority;
        poi.DescriptionVi = vm.DescriptionVi?.Trim() ?? "";  // FIX: null-safe
        poi.DescriptionEn = vm.DescriptionEn?.Trim() ?? "";  // FIX: null-safe
        poi.DescriptionJa = NullIfBlank(vm.DescriptionJa);
        poi.DescriptionKo = NullIfBlank(vm.DescriptionKo);
        poi.DescriptionZh = NullIfBlank(vm.DescriptionZh);
        poi.ImageUrl = NullIfBlank(vm.ImageUrl);
        poi.Address = NullIfBlank(vm.Address);
        poi.PhoneNumber = NullIfBlank(vm.PhoneNumber);
        poi.OpeningHours = NullIfBlank(vm.OpeningHours);
        poi.PriceRange = NullIfBlank(vm.PriceRange);
        poi.GoogleMapsLink = NullIfBlank(vm.GoogleMapsLink);
        poi.IsActive = vm.IsActive;
        poi.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật POI #{Id}", id);
            ModelState.AddModelError("", $"Lỗi lưu database: {ex.InnerException?.Message ?? ex.Message}");
            return View("Form", vm);
        }

        TempData["Success"] = $"Đã cập nhật POI \"{poi.Name}\"";
        return RedirectToAction("Index");
    }

    // ================= DELETE / RESTORE =================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var poi = await _db.Pois.FindAsync(id);
        if (poi == null) return NotFound();

        poi.IsActive = false;
        poi.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã vô hiệu hóa \"{poi.Name}\" (soft delete)";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var poi = await _db.Pois.FindAsync(id);
        if (poi == null) return NotFound();

        poi.IsActive = true;
        poi.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã khôi phục \"{poi.Name}\"";
        return RedirectToAction("Index", new { deleted = true });
    }

    // ================= AJAX PARSE GMAPS =================
    // Uses antiforgery header (X-CSRF-TOKEN or RequestVerificationToken)
    [HttpPost]
    [IgnoreAntiforgeryToken] // We validate manually since client sends JSON body
    public IActionResult ParseGmaps([FromBody] ParseGmapsRequest body)
    {
        if (body == null || string.IsNullOrWhiteSpace(body.Input))
            return Json(new { ok = false, error = "missing input" });

        var r = GmapsParser.Parse(body.Input);
        if (r == null)
            return Json(new { ok = false, error = "not_parseable" });

        return Json(new { ok = true, lat = r.Lat, lng = r.Lng, method = r.Method });
    }

    public class ParseGmapsRequest
    {
        public string Input { get; set; } = "";
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
