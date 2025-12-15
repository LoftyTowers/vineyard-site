using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VineyardApi.Domain.Content;
using VineyardApi.Models;
using VineyardApi.Repositories;

namespace VineyardApi.Services
{
    public class PageService : IPageService
    {
        private readonly IPageRepository _repository;
        private readonly ILogger<PageService> _logger;

        public PageService(IPageRepository repository, ILogger<PageService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<PageContent>> GetPageContentAsync(string route, CancellationToken cancellationToken = default)
        {
            const string operation = "GetPageWithOverrides";

            try
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    {"PageRoute", route},
                    {"DbOperation", operation}
                });
                var page = await _repository.GetPageWithOverridesAsync(route, cancellationToken);
                if (page == null)
                {
                    _logger.LogWarning("Page read failed: not found (Route: {Route})", route);
                    return Result<PageContent>.Failure(ErrorCode.NotFound, $"Page '{route}' not found.");
                }

                var overrideContent = page.Overrides
                    .OrderByDescending(o => o.UpdatedAt)
                    .FirstOrDefault();

                _logger.LogInformation("Page read succeeded (Route: {Route})", route);
                return Result<PageContent>.Ok(overrideContent?.OverrideContent ?? page.DefaultContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Page read failed (Route: {Route}, Operation: {Operation})", route, operation);
                return Result<PageContent>.Failure(ErrorCode.Unexpected, "PageReadFailed");
            }
        }

        public async Task<Result> SaveOverrideAsync(PageOverride model, CancellationToken cancellationToken = default)
        {
            const string operation = "SavePageOverride";
            try
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    {"PageId", model.PageId},
                    {"DbOperation", operation}
                });
                model.UpdatedAt = DateTime.UtcNow;
                var existing = await _repository.GetPageOverrideByPageIdAsync(model.PageId, cancellationToken);
                if (existing == null)
                {
                    _logger.LogInformation("Creating override for page {PageId}", model.PageId);
                    _repository.AddPageOverride(model);
                }
                else
                {
                    _logger.LogInformation("Updating override for page {PageId}", model.PageId);
                    existing.OverrideContent = model.OverrideContent;
                    existing.UpdatedAt = model.UpdatedAt;
                    existing.UpdatedById = model.UpdatedById;
                }

                await _repository.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving page override for page {PageId} (Operation: {Operation})", model.PageId, operation);
                return Result.Failure(ErrorCode.Unexpected, "PageOverrideSaveFailed");
            }
        }
    }
}
