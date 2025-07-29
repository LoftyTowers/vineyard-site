using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using VineyardApi.Data;
using VineyardApi.Models;

namespace VineyardApi.Tests.Data
{
    public class VineyardDbContextTests
    {
        [Test]
        public void Model_Contains_ContentOverrides_Entity()
        {
            var options = new DbContextOptionsBuilder<VineyardDbContext>()
                .UseInMemoryDatabase(databaseName: "test_db")
                .Options;

            using var context = new VineyardDbContext(options);

            // Property should be accessible as DbSet
            context.ContentOverrides.Should().NotBeNull();

            // Model should include ContentOverride entity
            context.Model.FindEntityType(typeof(ContentOverride)).Should().NotBeNull();
        }
    }
}
