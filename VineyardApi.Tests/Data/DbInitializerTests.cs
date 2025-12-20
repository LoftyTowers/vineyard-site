using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using VineyardApi.Data;
using VineyardApi.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace VineyardApi.Tests.Data
{
    public class DbInitializerTests
    {
        private sealed class TestHostEnvironment : IWebHostEnvironment
        {
            public string ApplicationName { get; set; } = "VineyardApi.Tests";
            public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
            public string WebRootPath { get; set; } = string.Empty;
            public string EnvironmentName { get; set; } = Environments.Development;
            public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        }

        private class TestVineyardDbContext : VineyardDbContext
        {
            public TestVineyardDbContext(DbContextOptions<VineyardDbContext> options)
                : base(options) { }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                modelBuilder.Entity<Page>().Ignore(p => p.DefaultContent);
                modelBuilder.Entity<PageOverride>().Ignore(p => p.OverrideContent);
                modelBuilder.Entity<PageVersion>().Ignore(pv => pv.ContentJson);
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
            services.AddSingleton<IWebHostEnvironment>(new TestHostEnvironment());
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
