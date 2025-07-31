using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VineyardApi.Models;

namespace VineyardApi.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;
            var context = provider.GetRequiredService<VineyardDbContext>();
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

            // roles
            var roles = new[] { "Admin", "Editor" };
            foreach (var r in roles)
            {
                if (!await context.Roles.AnyAsync(x => x.Name == r))
                {
                    context.Roles.Add(new Role { Name = r });
                }
            }
            await context.SaveChangesAsync();

            // permissions
            var permissionNames = new[]
            {
                "CanEditContent",
                "CanEditTheme",
                "CanManageUsers",
                "CanViewAdminPanel",
                "CanPublishContent"
            };
            foreach (var p in permissionNames)
            {
                if (!await context.Permissions.AnyAsync(x => x.Name == p))
                {
                    context.Permissions.Add(new Permission { Name = p });
                }
            }
            await context.SaveChangesAsync();

            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
            var editorRole = await context.Roles.FirstAsync(r => r.Name == "Editor");
            var allPerms = await context.Permissions.ToListAsync();

            foreach (var perm in allPerms)
            {
                if (!context.RolePermissions.Any(rp => rp.RoleId == adminRole.Id && rp.PermissionId == perm.Id))
                {
                    context.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = perm.Id });
                }
            }
            var editorPermNames = new[] { "CanEditContent", "CanEditTheme", "CanPublishContent", "CanViewAdminPanel" };
            foreach (var name in editorPermNames)
            {
                var perm = allPerms.First(p => p.Name == name);
                if (!context.RolePermissions.Any(rp => rp.RoleId == editorRole.Id && rp.PermissionId == perm.Id))
                {
                    context.RolePermissions.Add(new RolePermission { RoleId = editorRole.Id, PermissionId = perm.Id });
                }
            }
            await context.SaveChangesAsync();

            // superuser
            var email = Environment.GetEnvironmentVariable("SUPERADMIN_EMAIL");
            var password = Environment.GetEnvironmentVariable("SUPERADMIN_PASSWORD");
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("SUPERADMIN_EMAIL or SUPERADMIN_PASSWORD not set");
                return;
            }

            var user = await context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = email,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded roles and superuser successfully");
            }
            else
            {
                logger.LogInformation("Superuser already exists");
            }

            if (!user.Roles.Any(r => r.RoleId == adminRole.Id))
            {
                context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = adminRole.Id });
                await context.SaveChangesAsync();
            }
        }
    }
}
