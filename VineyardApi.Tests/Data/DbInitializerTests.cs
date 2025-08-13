using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using VineyardApi.Data;
using VineyardApi.Models;

namespace VineyardApi.Tests.Data
{
    public class DbInitializerTests
    {
        private class TestVineyardDbContext : VineyardDbContext
        {
            public TestVineyardDbContext(DbContextOptions<VineyardDbContext> options)
                : base(options) { }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                modelBuilder.Entity<Page>().Ignore(p => p.DefaultContent);
                modelBuilder.Entity<PageOverride>().Ignore(p => p.OverrideContent);
            }
        }

        [Test]
        public async Task SeedAsync_CreatesRoles_And_Superuser()
        {
            var options = new DbContextOptionsBuilder<VineyardDbContext>()
                .UseInMemoryDatabase("seed_db")
                .Options;
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<VineyardDbContext>(new TestVineyardDbContext(options));
            var provider = services.BuildServiceProvider();

            Environment.SetEnvironmentVariable("SUPERADMIN_EMAIL", "seed@test.com");
            Environment.SetEnvironmentVariable("SUPERADMIN_PASSWORD", "pass123");

            await DbInitializer.SeedAsync(provider);

            using var scope = provider.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<VineyardDbContext>();

            ctx.Roles.Should().Contain(r => r.Name == "Admin");
            ctx.Roles.Should().Contain(r => r.Name == "Editor");

            var user = await ctx.Users.Include(u => u.Roles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == "seed@test.com");
            user.Should().NotBeNull();
            user!.Roles.Should().Contain(r => r.Role!.Name == "Admin");
        }
    }
}
