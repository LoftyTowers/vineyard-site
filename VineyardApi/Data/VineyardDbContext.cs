using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;
using VineyardApi.Domain.Content;
using VineyardApi.Models;

namespace VineyardApi.Data
{
    public class VineyardDbContext : DbContext
    {
        public VineyardDbContext(DbContextOptions<VineyardDbContext> options) : base(options) { }

        public DbSet<Page> Pages => Set<Page>();
        public DbSet<PageVersion> PageVersions => Set<PageVersion>();
        public DbSet<PageOverride> PageOverrides => Set<PageOverride>();
        public DbSet<ContentOverride> ContentOverrides => Set<ContentOverride>();
        public DbSet<ThemeDefault> ThemeDefaults => Set<ThemeDefault>();
        public DbSet<ThemeOverride> ThemeOverrides => Set<ThemeOverride>();
        public DbSet<Image> Images => Set<Image>();
        public DbSet<ImageUsage> ImageUsages => Set<ImageUsage>();
        public DbSet<Person> People => Set<Person>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<AuditHistory> AuditHistories => Set<AuditHistory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Page>(e =>
            {
                e.Property(p => p.DefaultContent)
                 .HasColumnType("jsonb");
                e.HasIndex(p => p.Route).IsUnique();
                e.HasOne(p => p.CurrentVersion)
                    .WithMany()
                    .HasForeignKey(p => p.CurrentVersionId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(p => p.DraftVersion)
                    .WithMany()
                    .HasForeignKey(p => p.DraftVersionId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasMany(p => p.Versions)
                    .WithOne(v => v.Page)
                    .HasForeignKey(v => v.PageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PageVersion>(e =>
            {
                e.Property(pv => pv.ContentJson)
                    .HasColumnType("jsonb");
                e.Property(pv => pv.Status)
                    .HasConversion<string>()
                    .HasDefaultValue(PageVersionStatus.Published);
                e.HasIndex(pv => new { pv.PageId, pv.VersionNo })
                    .IsUnique()
                    .HasDatabaseName("IX_PageVersions_PageId_VersionNo");
                e.HasIndex(pv => pv.PageId)
                    .IsUnique()
                    .HasDatabaseName("IX_PageVersions_PageId_Draft")
                    .HasFilter("\"Status\" = 'Draft'");
                e.Property(pv => pv.CreatedUtc)
                    .HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<PageOverride>(e =>
            {
                e.Property(p => p.OverrideContent)
                 .HasColumnType("jsonb");
            });

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<ContentOverride>()
                .Property(c => c.Timestamp)
                .HasDefaultValueSql("now() at time zone 'utc'");
            modelBuilder.Entity<ContentOverride>()
                .Property(c => c.Status)
                .HasDefaultValue("draft");

            modelBuilder.Entity<Image>(e =>
            {
                e.HasIndex(i => i.StorageKey).IsUnique();
                e.Property(i => i.CreatedUtc)
                    .HasDefaultValueSql("now() at time zone 'utc'");
                e.Property(i => i.IsActive)
                    .HasDefaultValue(true);
            });

            modelBuilder.Entity<ImageUsage>(e =>
            {
                e.HasIndex(iu => iu.ImageId).HasDatabaseName("IX_ImageUsages_ImageId");
                e.HasIndex(iu => new { iu.EntityType, iu.EntityKey }).HasDatabaseName("IX_ImageUsages_EntityType_EntityKey");
                e.HasIndex(iu => new { iu.ImageId, iu.EntityType, iu.EntityKey, iu.UsageType, iu.Source, iu.JsonPath })
                    .IsUnique();
                e.Property(iu => iu.UpdatedUtc)
                    .HasDefaultValueSql("now() at time zone 'utc'");
            });

            modelBuilder.Entity<Person>(e =>
            {
                e.HasOne(p => p.Page)
                    .WithMany()
                    .HasForeignKey(p => p.PageId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(p => new { p.PageId, p.SortOrder });
                e.HasIndex(p => new { p.PageId, p.IsActive });
                e.Property(p => p.IsActive)
                    .HasDefaultValue(true);
                e.Property(p => p.CreatedUtc)
                    .HasDefaultValueSql("now() at time zone 'utc'");
                e.Property(p => p.UpdatedUtc)
                    .HasDefaultValueSql("now() at time zone 'utc'");
            });

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
                    PreviousValue = entry.State == EntityState.Modified
                        ? JsonSerializer.Serialize(entry.OriginalValues.ToObject())
                        : null,
                    NewValue = JsonSerializer.Serialize(entry.CurrentValues.ToObject()),
                    ChangedAt = DateTime.UtcNow
                };
                AuditHistories.Add(history);
            }
        }
    }
}
