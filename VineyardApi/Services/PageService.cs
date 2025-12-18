using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using VineyardApi.Domain.Content;
using VineyardApi.Models;
using VineyardApi.Repositories;

namespace VineyardApi.Services
{
    public class PageService : IPageService
    {
        private readonly IPageRepository _repository;
        private readonly IImageRepository _imageRepository;
        private readonly IImageUsageRepository _imageUsageRepository;
        private readonly ILogger<PageService> _logger;

        public PageService(IPageRepository repository, IImageRepository imageRepository, IImageUsageRepository imageUsageRepository, ILogger<PageService> logger)
        {
            _repository = repository;
            _imageRepository = imageRepository;
            _imageUsageRepository = imageUsageRepository;
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
                var content = overrideContent?.OverrideContent ?? page.DefaultContent;
                var hydrated = await HydrateImageBlocksAsync(content, cancellationToken);
                return Result<PageContent>.Ok(hydrated);
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
                await UpdateOverrideImageUsagesAsync(model, cancellationToken);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving page override for page {PageId} (Operation: {Operation})", model.PageId, operation);
                return Result.Failure(ErrorCode.Unexpected, "PageOverrideSaveFailed");
            }
        }

        private async Task UpdateOverrideImageUsagesAsync(PageOverride model, CancellationToken cancellationToken)
        {
            var page = await _repository.GetPageByIdAsync(model.PageId, cancellationToken);
            if (page == null)
            {
                _logger.LogWarning("Skipping image usage update because page {PageId} was not found.", model.PageId);
                return;
            }

            var content = model.OverrideContent ?? page.DefaultContent;
            var usages = BuildImageUsages(content, page.Route, "Override");
            await _imageUsageRepository.DeletePageUsagesAsync(page.Route, "Override", cancellationToken);
            if (usages.Count > 0)
            {
                _imageUsageRepository.AddRange(usages);
            }
            await _imageUsageRepository.SaveChangesAsync(cancellationToken);
        }

        private async Task<PageContent> HydrateImageBlocksAsync(PageContent content, CancellationToken cancellationToken)
        {
            if (content.Blocks.Count == 0)
            {
                return content;
            }

            var imageIds = ExtractImageIds(content);
            if (imageIds.Count == 0)
            {
                return content;
            }

            var images = await _imageRepository.GetActiveByIdsAsync(imageIds, cancellationToken);
            var hydratedBlocks = new List<PageBlock>(content.Blocks.Count);

            foreach (var block in content.Blocks)
            {
                if (!IsImageBlock(block))
                {
                    hydratedBlocks.Add(block);
                    continue;
                }

                if (!TryParseObject(block.Content, out var obj))
                {
                    hydratedBlocks.Add(block);
                    continue;
                }

                if (!TryGetImageId(obj, out var imageId))
                {
                    hydratedBlocks.Add(block);
                    continue;
                }

                if (images.TryGetValue(imageId, out var image))
                {
                    obj["url"] = image.PublicUrl;
                    if (!obj.ContainsKey("src"))
                    {
                        obj["src"] = image.PublicUrl;
                    }
                    obj["storageKey"] = image.StorageKey;
                    if (image.Width.HasValue)
                    {
                        obj["width"] = image.Width.Value;
                    }
                    if (image.Height.HasValue)
                    {
                        obj["height"] = image.Height.Value;
                    }
                    if (!obj.ContainsKey("alt") && !string.IsNullOrWhiteSpace(image.AltText))
                    {
                        obj["alt"] = image.AltText;
                    }
                    if (!obj.ContainsKey("caption") && !string.IsNullOrWhiteSpace(image.Caption))
                    {
                        obj["caption"] = image.Caption;
                    }
                }
                else
                {
                    obj["missing"] = true;
                }

                hydratedBlocks.Add(new PageBlock
                {
                    Type = block.Type,
                    Content = JsonSerializer.SerializeToElement(obj)
                });
            }

            return new PageContent
            {
                Blocks = hydratedBlocks
            };
        }

        private static HashSet<Guid> ExtractImageIds(PageContent content)
        {
            var ids = new HashSet<Guid>();
            foreach (var block in content.Blocks)
            {
                if (!IsImageBlock(block))
                {
                    continue;
                }

                if (!TryParseObject(block.Content, out var obj))
                {
                    continue;
                }

                if (TryGetImageId(obj, out var imageId))
                {
                    ids.Add(imageId);
                }
            }

            return ids;
        }

        private static List<ImageUsage> BuildImageUsages(PageContent content, string route, string source)
        {
            var usages = new List<ImageUsage>();
            var imageBlocks = new List<(Guid Id, int Index, string? Variant)>();

            for (var i = 0; i < content.Blocks.Count; i++)
            {
                var block = content.Blocks[i];
                if (!IsImageBlock(block))
                {
                    continue;
                }

                if (!TryParseObject(block.Content, out var obj))
                {
                    continue;
                }

                if (!TryGetImageId(obj, out var imageId))
                {
                    continue;
                }

                var variant = obj["variant"]?.GetValue<string>();
                imageBlocks.Add((imageId, i, variant));
            }

            if (imageBlocks.Count == 0)
            {
                return usages;
            }

            var heroIndexes = new HashSet<int>(imageBlocks
                .Where(b => string.Equals(b.Variant, "hero", StringComparison.OrdinalIgnoreCase))
                .Select(b => b.Index));

            if (heroIndexes.Count == 0)
            {
                heroIndexes.Add(imageBlocks[0].Index);
            }

            foreach (var block in imageBlocks)
            {
                var usageType = heroIndexes.Contains(block.Index) ? "Hero" : "Block";
                usages.Add(new ImageUsage
                {
                    Id = Guid.NewGuid(),
                    ImageId = block.Id,
                    EntityType = "Page",
                    EntityKey = route,
                    UsageType = usageType,
                    JsonPath = $"blocks[{block.Index}]",
                    Source = source,
                    UpdatedUtc = DateTime.UtcNow
                });
            }

            return usages;
        }

        private static bool IsImageBlock(PageBlock block) =>
            string.Equals(block.Type, "image", StringComparison.OrdinalIgnoreCase);

        private static bool TryParseObject(JsonElement element, out JsonObject obj)
        {
            obj = null!;
            if (element.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            obj = JsonNode.Parse(element.GetRawText()) as JsonObject ?? null!;
            return obj != null;
        }

        private static bool TryGetImageId(JsonObject obj, out Guid imageId)
        {
            imageId = Guid.Empty;
            if (!obj.TryGetPropertyValue("imageId", out var imageIdNode) || imageIdNode == null)
            {
                return false;
            }

            if (imageIdNode is JsonValue value && value.TryGetValue(out Guid parsed))
            {
                imageId = parsed;
                return true;
            }

            return Guid.TryParse(imageIdNode.ToString(), out imageId);
        }
    }
}
