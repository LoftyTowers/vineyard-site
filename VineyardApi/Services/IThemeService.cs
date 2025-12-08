using VineyardApi.Models;

namespace VineyardApi.Services
{
    public interface IThemeService
    {
        Task<Result<Dictionary<string, string>>> GetThemeAsync(CancellationToken cancellationToken);
        Task<Result> SaveOverrideAsync(Models.ThemeOverride model, CancellationToken cancellationToken);
    }
}
