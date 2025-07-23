using Microsoft.EntityFrameworkCore;
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

        public Task<List<ThemeDefault>> GetDefaultsAsync()
        {
            return _context.ThemeDefaults.ToListAsync();
        }

        public Task<List<ThemeOverride>> GetOverridesAsync()
        {
            return _context.ThemeOverrides.ToListAsync();
        }

        public Task<ThemeOverride?> GetOverrideAsync(int defaultId)
        {
            return _context.ThemeOverrides.FirstOrDefaultAsync(t => t.ThemeDefaultId == defaultId);
        }

        public void AddThemeOverride(ThemeOverride model)
        {
            _context.ThemeOverrides.Add(model);
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
