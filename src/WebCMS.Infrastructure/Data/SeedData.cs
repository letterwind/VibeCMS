using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WebCMS.Core.Entities;

namespace WebCMS.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            var roles = new[]
            {
                new Role { Name = "root", Description = "超級管理員", HierarchyLevel = 1 },
                new Role { Name = "admin", Description = "管理員", HierarchyLevel = 2 }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        if (!await context.AdminUsers.AnyAsync())
        {
            var rootRole = await context.Roles.FirstAsync(r => r.Name == "root");
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "admin");

            var users = new[]
            {
                new AdminUser 
                { 
                    Account = "root", 
                    DisplayName = "Root",
                    PasswordHash = HashPassword("root123"),
                    PasswordLastChanged = DateTime.UtcNow
                },
                new AdminUser 
                { 
                    Account = "admin", 
                    DisplayName = "Admin",
                    PasswordHash = HashPassword("admin123"),
                    PasswordLastChanged = DateTime.UtcNow
                }
            };
            await context.AdminUsers.AddRangeAsync(users);
            await context.SaveChangesAsync();

            var userRoles = new[]
            {
                new UserRole { UserId = users[0].Id, RoleId = rootRole.Id },
                new UserRole { UserId = users[1].Id, RoleId = adminRole.Id }
            };
            await context.UserRoles.AddRangeAsync(userRoles);
            await context.SaveChangesAsync();
        }
    }

    private static string HashPassword(string password)
    {
        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            100000,
            HashAlgorithmName.SHA256,
            32);

        var hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 32);

        return Convert.ToBase64String(hashBytes);
    }
}
