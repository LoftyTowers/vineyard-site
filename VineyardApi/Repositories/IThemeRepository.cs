using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public interface IThemeRepository
    {
        Task<List<ThemeDefault>> GetDefaultsAsync();
        Task<List<ThemeOverride>> GetOverridesAsync();
        Task<ThemeOverride?> GetOverrideAsync(int defaultId);
        void AddThemeOverride(ThemeOverride model);
        Task<int> SaveChangesAsync();
    }
}
