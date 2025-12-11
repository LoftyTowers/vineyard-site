using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public interface IThemeRepository
    {
        Task<List<ThemeDefault>> GetDefaultsAsync(CancellationToken cancellationToken = default);
        Task<List<ThemeOverride>> GetOverridesAsync(CancellationToken cancellationToken = default);
        Task<ThemeOverride?> GetOverrideAsync(int defaultId, CancellationToken cancellationToken = default);
        void AddThemeOverride(ThemeOverride model);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
