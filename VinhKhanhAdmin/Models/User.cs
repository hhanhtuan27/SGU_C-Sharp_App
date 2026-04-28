using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhKhanhAdmin.Models;

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Username { get; set; } = "";

    [Required, StringLength(256)]
    public string PasswordHash { get; set; } = "";

    [StringLength(100)]
    public string? DisplayName { get; set; }

    [StringLength(200)]
    public string? Email { get; set; }

    /// <summary>admin | user</summary>
    [Required, StringLength(20)]
    public string Role { get; set; } = "user";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;
}
