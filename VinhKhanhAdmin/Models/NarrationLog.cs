using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhKhanhAdmin.Models;

[Table("NarrationLog")]
public class NarrationLog
{
    [Key]
    public long Id { get; set; }

    public int PoiId { get; set; }

    [StringLength(100)]
    public string? DeviceId { get; set; }

    /// <summary>Vietnamese | English | Japanese | Korean | Chinese</summary>
    [Required, StringLength(20)]
    public string Language { get; set; } = "Vietnamese";

    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(PoiId))]
    public PointOfInterest? Poi { get; set; }
}
