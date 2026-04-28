using Microsoft.EntityFrameworkCore;
using VinhKhanhAdmin.Models;

namespace VinhKhanhAdmin.Services;

/// <summary>
/// Ensures admin/demo users exist with BCrypt-hashed passwords.
/// Runs once on app startup; idempotent.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedUsersAsync(AppDbContext db, ILogger logger)
    {
        try
        {
            if (!await db.Database.CanConnectAsync())
            {
                logger.LogWarning("⚠ Cannot connect to database. Skip user seeding.");
                return;
            }

            await EnsureUserAsync(db, "admin", "admin123", "Quản trị viên",
                "admin@vinhkhanh.local", "admin", logger);

            await EnsureUserAsync(db, "demo", "demo123", "Demo User",
                "demo@vinhkhanh.local", "user", logger);

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DataSeeder failed. Check database connection + schema.");
        }
    }

    private static async Task EnsureUserAsync(AppDbContext db,
        string username, string plaintextPassword, string displayName,
        string email, string role, ILogger logger)
    {
        var existing = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (existing != null) return;

        db.Users.Add(new User
        {
            Username     = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(plaintextPassword, workFactor: 11),
            DisplayName  = displayName,
            Email        = email,
            Role         = role,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        });

        logger.LogInformation("✓ Seeded user: {Username} ({Role})", username, role);
    }
}
