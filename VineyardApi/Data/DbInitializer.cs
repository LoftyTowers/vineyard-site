using System;
using System.Threading;
using BCrypt.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VineyardApi.Models;

namespace VineyardApi.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
        {
            try
            {
                using var scope = services.CreateScope();
                var provider = scope.ServiceProvider;
                var context = provider.GetRequiredService<VineyardDbContext>();
                var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
                var env = provider.GetRequiredService<IWebHostEnvironment>();

                await RunSeedScriptsAsync(context, env, logger, cancellationToken);

                var roles = new[] { "Admin", "Editor" };
                foreach (var r in roles)
                {
                    if (!await context.Roles.AnyAsync(x => x.Name == r, cancellationToken))
                    {
                        context.Roles.Add(new Role { Name = r });
                    }
                }
                await context.SaveChangesAsync(cancellationToken);

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
                    if (!await context.Permissions.AnyAsync(x => x.Name == p, cancellationToken))
                    {
                        context.Permissions.Add(new Permission { Name = p });
                    }
                }
                await context.SaveChangesAsync(cancellationToken);

                var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin", cancellationToken);
                var editorRole = await context.Roles.FirstAsync(r => r.Name == "Editor", cancellationToken);
                var allPerms = await context.Permissions.ToListAsync(cancellationToken);

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
                await context.SaveChangesAsync(cancellationToken);

                var email = Environment.GetEnvironmentVariable("SUPERADMIN_EMAIL");
                var password = Environment.GetEnvironmentVariable("SUPERADMIN_PASSWORD");
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    logger.LogWarning("SUPERADMIN_EMAIL or SUPERADMIN_PASSWORD not set");
                }
                else
                {
                    var user = await context.Users
                        .Include(u => u.Roles)
                        .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
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
                        await context.SaveChangesAsync(cancellationToken);
                        logger.LogInformation("Seeded roles and superuser successfully");
                    }
                    else
                    {
                        logger.LogInformation("Superuser already exists");
                    }

                    if (!user.Roles.Any(r => r.RoleId == adminRole.Id))
                    {
                        context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = adminRole.Id });
                        await context.SaveChangesAsync(cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
                logger.LogError(ex, "Failed to seed database");
                throw;
            }
        }

        private static async Task RunSeedScriptsAsync(
            VineyardDbContext context,
            IWebHostEnvironment env,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var seedRoot = Path.GetFullPath(Path.Combine(env.ContentRootPath, "SeedScripts"));
            if (!Directory.Exists(seedRoot))
            {
                logger.LogWarning("SeedScripts directory not found at {SeedRoot}", seedRoot);
                return;
            }

            var scripts = new (string Table, Func<Task<bool>> HasAny, string FileName)[]
            {
                ("ThemeDefaults", () => context.ThemeDefaults.AnyAsync(cancellationToken), "01_ThemeDefaults.sql"),
                ("Images", () => context.Images.AnyAsync(cancellationToken), "02_Images.sql"),
                ("Pages", () => context.Pages.AnyAsync(cancellationToken), "03_Pages.sql"),
                ("Roles", () => context.Roles.AnyAsync(cancellationToken), "04_Roles.sql"),
                ("Permissions", () => context.Permissions.AnyAsync(cancellationToken), "05_Permissions.sql"),
                ("RolePermissions", () => context.RolePermissions.AnyAsync(cancellationToken), "06_RolePermissions.sql"),
                ("Users", () => context.Users.AnyAsync(cancellationToken), "07_Users.sql"),
                ("UserRoles", () => context.UserRoles.AnyAsync(cancellationToken), "08_UserRoles.sql")
            };

            foreach (var script in scripts)
            {
                var path = Path.Combine(seedRoot, script.FileName);
                if (await script.HasAny())
                {
                    logger.LogInformation("Skipping seed script for {Table} because table is not empty.", script.Table);
                    continue;
                }

                if (!File.Exists(path))
                {
                    logger.LogWarning("Seed script missing for {Table}: {Path}", script.Table, path);
                    continue;
                }

                var sql = await File.ReadAllTextAsync(path, cancellationToken);
                if (string.IsNullOrWhiteSpace(sql))
                {
                    logger.LogWarning("Seed script empty for {Table}: {Path}", script.Table, path);
                    continue;
                }

                var escapedSql = sql.Replace("{", "{{").Replace("}", "}}");
                await context.Database.ExecuteSqlRawAsync(escapedSql, cancellationToken);
                logger.LogInformation("Applied seed script for {Table}.", script.Table);
            }
        }
    }
}
