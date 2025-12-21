using Microsoft.EntityFrameworkCore;
using System;
using VineyardApi.Data;
using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public class ThemeRepository : IThemeRepository
    {
        private readonly VineyardDbContext _context;
        public ThemeRepository(VineyardDbContext context)
        {
            _context = context;
        }

        public Task<List<ThemeDefault>> GetDefaultsAsync(CancellationToken cancellationToken = default)
        {
            return _context.ThemeDefaults.ToListAsync(cancellationToken);
        }

        public Task<List<ThemeOverride>> GetOverridesAsync(CancellationToken cancellationToken = default)
        {
            return _context.ThemeOverrides.ToListAsync(cancellationToken);
        }

        public Task<ThemeOverride?> GetOverrideAsync(int defaultId, CancellationToken cancellationToken = default)
        {
            return _context.ThemeOverrides.FirstOrDefaultAsync(t => t.ThemeDefaultId == defaultId, cancellationToken);
        }

        public void AddThemeOverride(ThemeOverride model)
        {
            _context.ThemeOverrides.Add(model);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
