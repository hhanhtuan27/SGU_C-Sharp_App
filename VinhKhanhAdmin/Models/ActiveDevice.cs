using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhKhanhAdmin.Models;

[Table("ActiveDevices")]
public class ActiveDevice
{
    [Key]
    [StringLength(100)]
    public string DeviceId { get; set; } = "";

    public int? UserId { get; set; }

    [StringLength(20)]
    public string? Platform { get; set; }

    [StringLength(20)]
    public string? AppVersion { get; set; }

    public DateTime LastPingUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
