using VineyardApi.Infrastructure;

using VineyardApi.Models;

namespace VineyardApi.Services
{
    public interface IThemeService
    {
        Task<Result<Dictionary<string, string>>> GetThemeAsync(CancellationToken cancellationToken = default);
        Task<Result> SaveOverrideAsync(Models.ThemeOverride model, CancellationToken cancellationToken = default);
    }
}
