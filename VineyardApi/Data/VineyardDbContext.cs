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
        public DbSet<ContentOverride> ContentOverrides => Set<ContentOverride>();
        public DbSet<ThemeDefault> ThemeDefaults => Set<ThemeDefault>();
        public DbSet<ThemeOverride> ThemeOverrides => Set<ThemeOverride>();
        public DbSet<Image> Images => Set<Image>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
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

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // JSONB conversions
            modelBuilder.Entity<Page>()
                .Property(p => p.DefaultContent)
                .HasColumnType("jsonb");
            modelBuilder.Entity<PageOverride>()
                .Property(p => p.OverrideContent)
                .HasColumnType("jsonb");
            modelBuilder.Entity<ContentOverride>()
                .Property(c => c.Timestamp)
                .HasDefaultValueSql("now() at time zone 'utc'");
            modelBuilder.Entity<ContentOverride>()
                .Property(c => c.Status)
                .HasDefaultValue("draft");
            modelBuilder.Entity<AuditHistory>()
                .Property(a => a.PreviousValue)
                .HasColumnType("jsonb");
            modelBuilder.Entity<AuditHistory>()
                .Property(a => a.NewValue)
                .HasColumnType("jsonb");

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
