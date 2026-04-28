using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VinhKhanhAdmin.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

public class PoiFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Tên quán là bắt buộc")]
    [StringLength(150)]
    [Display(Name = "Tên quán")]
    public string Name { get; set; } = "";

    [Required]
    [StringLength(20)]
    public string Category { get; set; } = "Oc";

    [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng -90 đến 90")]
    public double Latitude { get; set; } = 10.760719;

    [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng -180 đến 180")]
    public double Longitude { get; set; } = 106.703297;

    [Range(1, 500)]
    public double RadiusMeters { get; set; } = 30;

    [Range(1, 10)]
    public int Priority { get; set; } = 1;

    [Required(ErrorMessage = "Mô tả tiếng Việt là bắt buộc")]
    [StringLength(1000)]
    public string DescriptionVi { get; set; } = "";

    [Required(ErrorMessage = "English description is required")]
    [StringLength(1000)]
    public string DescriptionEn { get; set; } = "";

    [StringLength(1000)] public string? DescriptionJa { get; set; }
    [StringLength(1000)] public string? DescriptionKo { get; set; }
    [StringLength(1000)] public string? DescriptionZh { get; set; }

    [StringLength(500)]  public string? ImageUrl { get; set; }
    [StringLength(300)]  public string? Address { get; set; }
    [StringLength(20)]   public string? PhoneNumber { get; set; }
    [StringLength(100)]  public string? OpeningHours { get; set; }
    [StringLength(50)]   public string? PriceRange { get; set; }
    [StringLength(1000)] public string? GoogleMapsLink { get; set; }

    public bool IsActive { get; set; } = true;

    public IFormFile? ImageFile { get; set; }
}

public class PoiListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusMeters { get; set; }
    public int Priority { get; set; }
    public string? ImageUrl { get; set; }
    public string? DescriptionVi { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PoiListViewModel
{
    public List<PoiListItemViewModel> Items { get; set; } = new();
    public string? SearchQuery { get; set; }
    public string? CategoryFilter { get; set; }
    public bool ShowDeleted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class DashboardViewModel
{
    public int TotalPois { get; set; }
    public int ActivePois { get; set; }
    public int TotalUsers { get; set; }
    public int OnlineDevices { get; set; }
    public int PlaysToday { get; set; }
    public int Plays30Days { get; set; }
    public List<TopPoiItem> TopPois { get; set; } = new();
    public Dictionary<string, int> CategoryCounts { get; set; } = new();
    public Dictionary<string, int> LanguageCounts { get; set; } = new();
    public List<DailyPlayItem> DailyPlays { get; set; } = new();
    public List<RecentPoiItem> RecentPois { get; set; } = new();
}

public class TopPoiItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public int PlayCount { get; set; }
    public int UniqueDevices { get; set; }
    public string? ImageUrl { get; set; }
}

public class DailyPlayItem
{
    public DateTime PlayDate { get; set; }
    public int PlayCount { get; set; }
}

public class RecentPoiItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UserFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(50), RegularExpression(@"^[a-zA-Z0-9_\-]+$", ErrorMessage = "Chỉ cho phép chữ, số, gạch dưới và gạch ngang")]
    public string Username { get; set; } = "";

    [StringLength(100)]
    public string? DisplayName { get; set; }

    [EmailAddress, StringLength(200)]
    public string? Email { get; set; }

    [Required, StringLength(20)]
    public string Role { get; set; } = "user";

    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    public string? Password { get; set; }

    public bool IsActive { get; set; } = true;
}

// --- API DTOs ---

public class ApiLoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string? DeviceId { get; set; }
    public string? Platform { get; set; }
}

public class ApiRegisterRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
}

public class ApiPingRequest
{
    public string DeviceId { get; set; } = "";
    public int? UserId { get; set; }
    public string? Platform { get; set; }
    public string? AppVersion { get; set; }
}

public class ApiNarrationLogRequest
{
    public int PoiId { get; set; }
    public string? DeviceId { get; set; }
    public string Language { get; set; } = "Vietnamese";
}
