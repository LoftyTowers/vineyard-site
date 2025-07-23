using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;
using VineyardApi.Models;

namespace VineyardApi.Data
{
    public class VineyardDbContext : DbContext
    {
        public VineyardDbContext(DbContextOptions<VineyardDbContext> options) : base(options) { }

        public DbSet<Page> Pages => Set<Page>();
        public DbSet<PageOverride> PageOverrides => Set<PageOverride>();
        public DbSet<ThemeDefault> ThemeDefaults => Set<ThemeDefault>();
        public DbSet<ThemeOverride> ThemeOverrides => Set<ThemeOverride>();
        public DbSet<Image> Images => Set<Image>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<AuditHistory> AuditHistories => Set<AuditHistory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Page>()
                .HasIndex(p => p.Route)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // JSONB conversions
            modelBuilder.Entity<Page>()
                .Property(p => p.DefaultContent)
                .HasColumnType("jsonb");
            modelBuilder.Entity<PageOverride>()
                .Property(p => p.OverrideContent)
                .HasColumnType("jsonb");
            modelBuilder.Entity<AuditHistory>()
                .Property(a => a.PreviousValue)
                .HasColumnType("jsonb");
            modelBuilder.Entity<AuditHistory>()
                .Property(a => a.NewValue)
                .HasColumnType("jsonb");

            SeedThemeDefaults(modelBuilder);
            SeedPages(modelBuilder);
        }

        private static void SeedThemeDefaults(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ThemeDefault>().HasData(
                new ThemeDefault { Id = 1, Key = "primary", Value = "#3B5F3B" },
                new ThemeDefault { Id = 2, Key = "secondary", Value = "#A97449" },
                new ThemeDefault { Id = 3, Key = "accent", Value = "#D5B57A" },
                new ThemeDefault { Id = 4, Key = "background", Value = "#F9F6F1" },
                new ThemeDefault { Id = 5, Key = "navbar", Value = "#EFE9DC" },
                new ThemeDefault { Id = 6, Key = "navbar-border", Value = "#DDD3C2" },
                new ThemeDefault { Id = 7, Key = "contrast", Value = "#2E2E2E" },
                new ThemeDefault { Id = 8, Key = "heading font", Value = "\"Playfair Display\", serif" },
                new ThemeDefault { Id = 9, Key = "body font", Value = "Lora, serif" }
            );
        }

        private static void SeedPages(ModelBuilder modelBuilder)
        {
            var now = new DateTime(2025, 7, 23, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<Page>().HasData(
                new Page
                {
                    Id = Guid.Parse("75f1dc70-6120-42c0-9c5e-8138fb755bbe"),
                    Route = string.Empty,
                    DefaultContent = new JsonObject
                    {
                        ["blocks"] = JsonNode.Parse("[{\"type\":\"p\",\"content\":\"Tucked away in the quiet countryside of North Essex, Hollywood Farm Vineyard is a small family project rooted in passion, tradition, and legacy.\"},{\"type\":\"p\",\"content\":\"Our family has farmed this land for over a century. Through five generations, it has been passed down, worked on, and cared for each adding something new to the story. When Charles retired, he decided it was time for a different kind of planting: a vineyard.\"},{\"type\":\"p\",\"content\":\"That one decision brought the whole family together. Aunts, uncles, cousins, and kids all got involved. Today, three generations pitch in with planting, pruning, and picking.\"},{\"type\":\"p\",\"content\":\"We have had one hand-picked harvest so far and while we are just getting started, the roots run deep.\"}]")
                    },
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Page
                {
                    Id = Guid.Parse("d1c84029-8121-47cc-9d84-a94c30b163b3"),
                    Route = "about",
                    DefaultContent = new JsonObject
                    {
                        ["blocks"] = JsonNode.Parse("[{\"type\":\"h1\",\"content\":\"Our Story\"},{\"type\":\"image\",\"content\":{\"src\":\"assets/temp-images/ReadyForHarvest.jpg\",\"alt\":\"Ready for Harvest\",\"caption\":\"Our first harvest, picked by hand in 2024\"}},{\"type\":\"h2\",\"content\":\"The People Behind the Vines\"},{\"type\":\"people\",\"content\":[{\"imageUrl\":\"assets/temp-images/ReadyForHarvest.jpg\",\"name\":\"Charles\",\"text\":\"After decades running the farm, Charles planted the first vines the year he retired. He\u2019s the reason any of this exists \u2013 and he still walks the rows most mornings, making sure everything\u2019s looking right.\"}]},{\"type\":\"p\",\"content\":\"Together, we are building something small, meaningful, and completely our own.\"}]")
                    },
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Page
                {
                    Id = Guid.Parse("a0412df9-703d-420a-a1af-2bec3380769f"),
                    Route = "gallery",
                    DefaultContent = new JsonObject
                    {
                        ["blocks"] = JsonNode.Parse("[{\"type\":\"h1\",\"content\":\"Gallery\"},{\"type\":\"h2\",\"content\":\"A few moments from our journey so far.\"},{\"type\":\"p\",\"content\":\"Here are a few snapshots of the family, the vineyard, and the land we\u2019re growing into.\"},{\"type\":\"image\",\"content\":{\"src\":\"assets/temp-images/ASunriseAtTheVineyard.jpg\",\"alt\":\"Sunrise at the vineyard\",\"caption\":\"Morning light over the first row\"}},{\"type\":\"image\",\"content\":{\"src\":\"assets/temp-images/HarvestComplete.jpg\",\"alt\":\"Harvest complete\",\"caption\":\"The last crate picked before sunset\"}},{\"type\":\"image\",\"content\":{\"src\":\"assets/temp-images/HelloWoof.jpg\",\"alt\":\"The family dog\",\"caption\":\"One of the friendliest members of the crew\"}},{\"type\":\"image\",\"content\":{\"src\":\"assets/temp-images/JustPlanted.jpg\",\"alt\":\"Just planted\",\"caption\":\"The first baby vines going into the soil\"}},{\"type\":\"image\",\"content\":{\"src\":\"assets/temp-images/PickedGrapesInSunset.jpg\",\"alt\":\"Picked grapes in sunset\",\"caption\":\"Fresh grapes and golden light \u2014 a perfect pairing\"}},{\"type\":\"image\",\"content\":{\"src\":\"assets/temp-images/ReadyForHarvest.jpg\",\"alt\":\"Ready for harvest\",\"caption\":\"The week before we picked our first crop\"}},{\"type\":\"image\",\"content\":{\"src\":\"assets/temp-images/TheMistyVineyard.jpg\",\"alt\":\"The misty vineyard\",\"caption\":\"Early autumn fog across the rows\"}},{\"type\":\"image\",\"content\":{\"src\":\"assets/temp-images/TheMoonlitMist.jpg\",\"alt\":\"The moonlit mist\",\"caption\":\"Taken on a quiet evening just before harvest\"}}]")
                    },
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );
        }

        public override int SaveChanges()
        {
            AddAuditEntries();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddAuditEntries();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void AddAuditEntries()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => (e.Entity is PageOverride || e.Entity is ThemeOverride) &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entityName = entry.Entity.GetType().Name;
                var entityId = (Guid)entry.Property("Id").CurrentValue!;
                var userId = entry.Property("UpdatedById").CurrentValue as Guid? ?? Guid.Empty;

                var log = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Action = entry.State == EntityState.Added ? "Created" : "Updated",
                    EntityType = entityName,
                    EntityId = entityId,
                    Timestamp = DateTime.UtcNow
                };
                AuditLogs.Add(log);

                var history = new AuditHistory
                {
                    Id = Guid.NewGuid(),
                    AuditLog = log,
                    PreviousValue = entry.State == EntityState.Modified ? (JsonNode.Parse(System.Text.Json.JsonSerializer.Serialize(entry.OriginalValues.ToObject())) as JsonObject) : null,
                    NewValue = JsonNode.Parse(System.Text.Json.JsonSerializer.Serialize(entry.CurrentValues.ToObject())) as JsonObject,
                    ChangedAt = DateTime.UtcNow
                };
                AuditHistories.Add(history);
            }
        }
    }
}
