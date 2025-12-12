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
            try
            {
                return _context.ThemeDefaults.ToListAsync(cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<List<ThemeOverride>> GetOverridesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return _context.ThemeOverrides.ToListAsync(cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<ThemeOverride?> GetOverrideAsync(int defaultId, CancellationToken cancellationToken = default)
        {
            try
            {
                return _context.ThemeOverrides.FirstOrDefaultAsync(t => t.ThemeDefaultId == defaultId, cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void AddThemeOverride(ThemeOverride model)
        {
            try
            {
                _context.ThemeOverrides.Add(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
