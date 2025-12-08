using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public interface IThemeRepository
    {
        Task<List<ThemeDefault>> GetDefaultsAsync(CancellationToken cancellationToken);
        Task<List<ThemeOverride>> GetOverridesAsync(CancellationToken cancellationToken);
        Task<ThemeOverride?> GetOverrideAsync(int defaultId, CancellationToken cancellationToken);
        void AddThemeOverride(ThemeOverride model);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
