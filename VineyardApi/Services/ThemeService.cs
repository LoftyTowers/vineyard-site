using VineyardApi.Models;
using VineyardApi.Repositories;

namespace VineyardApi.Services
{
    public class ThemeService : IThemeService
    {
        private readonly IThemeRepository _repository;

        public ThemeService(IThemeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Dictionary<string, string>> GetThemeAsync()
        {
            var defaults = await _repository.GetDefaultsAsync();
            var overrides = await _repository.GetOverridesAsync();
            var result = defaults.ToDictionary(d => d.Key, d => d.Value);
            foreach (var ovr in overrides.OrderByDescending(o => o.UpdatedAt))
            {
                var key = defaults.FirstOrDefault(d => d.Id == ovr.ThemeDefaultId)?.Key;
                if (key != null) result[key] = ovr.Value;
            }
            return result;
        }

        public async Task SaveOverrideAsync(ThemeOverride model)
        {
            model.UpdatedAt = DateTime.UtcNow;
            var existing = await _repository.GetOverrideAsync(model.ThemeDefaultId);
            if (existing == null)
            {
                _repository.AddThemeOverride(model);
            }
            else
            {
                existing.Value = model.Value;
                existing.UpdatedAt = model.UpdatedAt;
                existing.UpdatedById = model.UpdatedById;
            }
            await _repository.SaveChangesAsync();
        }
    }
}
