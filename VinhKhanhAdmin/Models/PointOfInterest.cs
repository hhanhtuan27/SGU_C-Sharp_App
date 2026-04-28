using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhKhanhAdmin.Models;

[Table("PointsOfInterest")]
public class PointOfInterest
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = "";

    /// <summary>Oc | Nuong | Lau | CaPhe | Khac</summary>
    [Required, StringLength(20)]
    public string Category { get; set; } = "Khac";

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Range(1, 500)]
    public double RadiusMeters { get; set; } = 30;

    [Range(1, 10)]
    public int Priority { get; set; } = 1;

    // Descriptions (Vi + En required, others optional)
    [Required, StringLength(1000)]
    public string DescriptionVi { get; set; } = "";

    [Required, StringLength(1000)]
    public string DescriptionEn { get; set; } = "";

    [StringLength(1000)]
    public string? DescriptionJa { get; set; }

    [StringLength(1000)]
    public string? DescriptionKo { get; set; }

    [StringLength(1000)]
    public string? DescriptionZh { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    public string? OpeningHours { get; set; }

    [StringLength(50)]
    public string? PriceRange { get; set; }

    [StringLength(1000)]
    public string? GoogleMapsLink { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
