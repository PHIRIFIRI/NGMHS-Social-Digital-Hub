using NGMHS.Models;
using NGMHS.Services;

namespace NGMHS.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, PasswordService passwordService, IConfiguration configuration)
    {
        var adminEmail = configuration["SeedAdmin:Email"] ?? "admin@ngmhs.local";
        var adminPassword = configuration["SeedAdmin:Password"] ?? "Admin#2026";
        var adminFullName = configuration["SeedAdmin:FullName"] ?? "NGMHS Administrator";

        var existingAdmin = context.Users.FirstOrDefault(u =>
            u.Email.ToLower() == adminEmail.ToLower());

        if (existingAdmin is not null)
        {
            return;
        }

        var admin = new User
        {
            FullName = adminFullName,
            Email = adminEmail,
            Role = "Admin",
            CreatedAtUtc = DateTime.UtcNow
        };

        admin.PasswordHash = passwordService.HashPassword(admin, adminPassword);

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}
