using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using VineyardApi.Models;
using VineyardApi.Repositories;

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

        public async Task<Result<Dictionary<string, string>>> GetThemeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Loading theme defaults and overrides");
                var defaults = await _repository.GetDefaultsAsync(cancellationToken);
                var overrides = await _repository.GetOverridesAsync(cancellationToken);
                var result = defaults.ToDictionary(d => d.Key, d => d.Value);
                foreach (var ovr in overrides.OrderByDescending(o => o.UpdatedAt))
                {
                    var key = defaults.FirstOrDefault(d => d.Id == ovr.ThemeDefaultId)?.Key;
                    if (key != null) result[key] = ovr.Value;
                }

                return Result<Dictionary<string, string>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting theme overrides");
                return Result<Dictionary<string, string>>.Failure(ErrorCode.Unexpected);
            }
        }

        public async Task<Result> SaveOverrideAsync(ThemeOverride model, CancellationToken cancellationToken = default)
        {
            try
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>{{"ThemeDefaultId", model.ThemeDefaultId}});
                model.UpdatedAt = DateTime.UtcNow;
                var existing = await _repository.GetOverrideAsync(model.ThemeDefaultId, cancellationToken);
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

                await _repository.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving theme override for default {ThemeDefaultId}", model.ThemeDefaultId);
                return Result.Failure(ErrorCode.Unexpected);
            }
        }
    }
}
