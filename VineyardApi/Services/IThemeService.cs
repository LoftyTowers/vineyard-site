namespace VineyardApi.Services
{
    public interface IThemeService
    {
        Task<Dictionary<string, string>> GetThemeAsync();
        Task SaveOverrideAsync(Models.ThemeOverride model);
    }
}
