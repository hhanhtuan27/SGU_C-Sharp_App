using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhAdmin.Models;

namespace VinhKhanhAdmin.Controllers.Api;

[ApiController]
[Route("api/pois")]
public class ApiPoisController : ControllerBase
{
    private readonly AppDbContext _db;

    public ApiPoisController(AppDbContext db) => _db = db;

    /// <summary>GET /api/pois?category=Oc — active POIs for mobile app</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null)
    {
        var q = _db.Pois.AsNoTracking().Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(p => p.Category == category);

        var items = await q
            .OrderByDescending(p => p.Priority)
            .Select(p => new
            {
                p.Id, p.Name, p.Category, p.Latitude, p.Longitude,
                p.RadiusMeters, p.Priority,
                p.DescriptionVi, p.DescriptionEn,
                p.DescriptionJa, p.DescriptionKo, p.DescriptionZh,
                p.ImageUrl, p.Address, p.PhoneNumber,
                p.OpeningHours, p.PriceRange
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>GET /api/pois/{id}</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.Pois.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (p == null) return NotFound(new { error = "POI not found" });
        return Ok(p);
    }
}
