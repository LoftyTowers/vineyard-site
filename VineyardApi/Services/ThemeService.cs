using VineyardApi.Models;
using VineyardApi.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace VineyardApi.Services
{
    public class ThemeService : IThemeService
    {
        private readonly IThemeRepository _repository;
        private readonly ILogger<ThemeService> _logger;

        public ThemeService(IThemeRepository repository, ILogger<ThemeService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Dictionary<string, string>> GetThemeAsync()
        {
            _logger.LogInformation("Loading theme defaults and overrides");
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
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"ThemeDefaultId", model.ThemeDefaultId}});
            model.UpdatedAt = DateTime.UtcNow;
            var existing = await _repository.GetOverrideAsync(model.ThemeDefaultId);
            if (existing == null)
            {
                _logger.LogInformation("Creating theme override {ThemeDefaultId}", model.ThemeDefaultId);
                _repository.AddThemeOverride(model);
            }
            else
            {
                _logger.LogInformation("Updating theme override {ThemeDefaultId}", model.ThemeDefaultId);
                existing.Value = model.Value;
                existing.UpdatedAt = model.UpdatedAt;
                existing.UpdatedById = model.UpdatedById;
            }
            await _repository.SaveChangesAsync();
        }
    }
}
