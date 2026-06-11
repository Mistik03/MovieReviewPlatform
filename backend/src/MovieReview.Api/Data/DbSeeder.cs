using Microsoft.EntityFrameworkCore;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Enums;

namespace MovieReview.Api.Data;

public static class DbSeeder
{
    /// <summary>Ensures an administrator account exists, using credentials from the Admin config section.</summary>
    public static async Task SeedAdminAsync(AppDbContext context, IConfiguration configuration, ILogger logger)
    {
        var username = configuration["Admin:Username"];
        var email = configuration["Admin:Email"];
        var password = configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Admin seeding skipped — Admin:Username / Admin:Email / Admin:Password not fully configured.");
            return;
        }

        if (await context.Users.AnyAsync(u => u.Role == Roles.Admin))
            return;

        context.Users.Add(new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = Roles.Admin
        });
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded administrator account '{Username}'.", username);
    }
}
