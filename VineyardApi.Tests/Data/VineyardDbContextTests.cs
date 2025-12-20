using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using VineyardApi.Data;
using VineyardApi.Models;

namespace VineyardApi.Tests.Data
{
    public class VineyardDbContextTests
    {
        private class TestVineyardDbContext : VineyardDbContext
        {
            public TestVineyardDbContext(DbContextOptions<VineyardDbContext> options)
                : base(options) { }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Ignore content types not supported by the in-memory provider
                modelBuilder.Entity<Page>().Ignore(p => p.DefaultContent);
                modelBuilder.Entity<PageOverride>().Ignore(p => p.OverrideContent);
                modelBuilder.Entity<PageVersion>().Ignore(pv => pv.ContentJson);
            }
        }

        [Test]
        public void Model_Contains_ContentOverrides_Entity()
        {
            var options = new DbContextOptionsBuilder<VineyardDbContext>()
                .UseInMemoryDatabase(databaseName: "test_db")
                .Options;

            using var context = new TestVineyardDbContext(options);

            // Property should be accessible as DbSet
            context.ContentOverrides.Should().NotBeNull();

            // Model should include ContentOverride entity
            context.Model.FindEntityType(typeof(ContentOverride)).Should().NotBeNull();
        }
    }
}
