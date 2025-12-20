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

                if (page.CurrentVersion == null)
                {
                    _logger.LogWarning("Page read failed: missing current version (Route: {Route})", route);
                    return Result<PageContent>.Failure(ErrorCode.NotFound, $"Page '{route}' has no published version.");
                }

                var overrideContent = page.Overrides
                    .OrderByDescending(o => o.UpdatedAt)
                    .FirstOrDefault();

                _logger.LogInformation("Page read succeeded (Route: {Route})", route);
                var content = overrideContent?.OverrideContent ?? page.CurrentVersion.ContentJson;
                var sanitized = SanitizeRichTextBlocks(content);
                var hydrated = await HydrateImageBlocksAsync(sanitized, cancellationToken);
                return Result<PageContent>.Ok(hydrated);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Page read cancelled (Route: {Route}, Operation: {Operation})", route, operation);
                return Result<PageContent>.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Page read failed (Route: {Route}, Operation: {Operation})", route, operation);
                return Result<PageContent>.Failure(ErrorCode.Unexpected, "PageReadFailed");
            }
        }

        public async Task<Result<PageContent>> GetDraftContentAsync(string route, CancellationToken cancellationToken = default)
        {
            const string operation = "GetDraft";
            try
            {
                var page = await _repository.GetPageWithVersionsAsync(route, cancellationToken);
                if (page == null)
                {
                    return Result<PageContent>.Failure(ErrorCode.NotFound, $"Page '{route}' not found.");
                }

                var draft = page.DraftVersionId.HasValue
                    ? page.Versions.FirstOrDefault(v => v.Id == page.DraftVersionId && v.Status == PageVersionStatus.Draft)
                    : null;

                if (draft == null)
                {
                    if (page.CurrentVersion == null)
                    {
                        return Result<PageContent>.Failure(ErrorCode.NotFound, "Draft not found.");
                    }

                    draft = page.CurrentVersion;
                }

                var sanitized = SanitizeRichTextBlocks(draft.ContentJson);
                var hydrated = await HydrateImageBlocksAsync(sanitized, cancellationToken);
                return Result<PageContent>.Ok(hydrated);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Draft read cancelled (Route: {Route}, Operation: {Operation})", route, operation);
                return Result<PageContent>.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Draft read failed (Route: {Route}, Operation: {Operation})", route, operation);
                return Result<PageContent>.Failure(ErrorCode.Unexpected, "DraftReadFailed");
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
                if (model.OverrideContent != null)
                {
                    model.OverrideContent = SanitizeRichTextBlocks(model.OverrideContent);
                }
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
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Saving page override cancelled for page {PageId} (Operation: {Operation})", model.PageId, operation);
                return Result.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving page override for page {PageId} (Operation: {Operation})", model.PageId, operation);
                return Result.Failure(ErrorCode.Unexpected, "PageOverrideSaveFailed");
            }
        }

        public async Task<Result<PageContent>> UpdateHeroImageAsync(string route, Guid imageId, CancellationToken cancellationToken = default)
        {
            const string operation = "UpdateHeroImage";
            try
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    {"PageRoute", route},
                    {"ImageId", imageId},
                    {"DbOperation", operation}
                });

                var page = await _repository.GetPageWithOverridesAsync(route, cancellationToken);
                if (page == null)
                {
                    return Result<PageContent>.Failure(ErrorCode.NotFound, $"Page '{route}' not found.");
                }

                var images = await _imageRepository.GetActiveByIdsAsync(new[] { imageId }, cancellationToken);
                if (!images.ContainsKey(imageId))
                {
                    return Result<PageContent>.Failure(ErrorCode.NotFound, "Image not found.");
                }

                var updatedContent = SetHeroImage(page.DefaultContent, imageId);
                page.DefaultContent = updatedContent;
                page.UpdatedAt = DateTime.UtcNow;
                await _repository.SaveChangesAsync(cancellationToken);

                var sanitized = SanitizeRichTextBlocks(updatedContent);
                var hydrated = await HydrateImageBlocksAsync(sanitized, cancellationToken);
                return Result<PageContent>.Ok(hydrated);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Hero image update cancelled (Route: {Route}, Operation: {Operation})", route, operation);
                return Result<PageContent>.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hero image update failed (Route: {Route}, Operation: {Operation})", route, operation);
                return Result<PageContent>.Failure(ErrorCode.Unexpected, "HeroImageUpdateFailed");
            }
        }

        public async Task<Result> AutosaveDraftAsync(string route, PageContent content, CancellationToken cancellationToken = default)
        {
            const string operation = "AutosaveDraft";
            try
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    {"PageRoute", route},
                    {"DbOperation", operation}
                });

                var page = await _repository.GetPageWithVersionsAsync(route, cancellationToken);
                if (page == null)
                {
                    _logger.LogWarning("Autosave failed: page not found (Route: {Route})", route);
                    return Result.Failure(ErrorCode.NotFound, $"Page '{route}' not found.");
                }

                var sanitized = SanitizeRichTextBlocks(content);
                var now = DateTime.UtcNow;

                if (page.DraftVersionId.HasValue)
                {
                    var draftCandidate = page.Versions.FirstOrDefault(v => v.Id == page.DraftVersionId);
                    if (draftCandidate != null && draftCandidate.Status != PageVersionStatus.Draft)
                    {
                        return Result.Failure(ErrorCode.Conflict, "Draft version id points to a non-draft version.");
                    }

                    if (draftCandidate != null)
                    {
                        draftCandidate.ContentJson = sanitized;
                        draftCandidate.UpdatedUtc = now;
                        await _repository.SaveChangesAsync(cancellationToken);
                        return Result.Ok();
                    }

                    _logger.LogWarning("DraftVersionId set but draft not found; creating new draft (Route: {Route})", route);
                }

                var nextVersionNo = page.Versions.Count == 0
                    ? 1
                    : page.Versions.Max(v => v.VersionNo) + 1;

                var newDraft = new PageVersion
                {
                    Id = Guid.NewGuid(),
                    PageId = page.Id,
                    VersionNo = nextVersionNo,
                    ContentJson = sanitized,
                    Status = PageVersionStatus.Draft,
                    CreatedUtc = now,
                    UpdatedUtc = now
                };

                _repository.AddPageVersion(newDraft);
                page.Versions.Add(newDraft);
                page.DraftVersionId = newDraft.Id;

                await _repository.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Autosave cancelled (Route: {Route}, Operation: {Operation})", route, operation);
                return Result.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Autosave failed (Route: {Route}, Operation: {Operation})", route, operation);
                return Result.Failure(ErrorCode.Unexpected, "AutosaveFailed");
            }
        }

        public async Task<Result<PageContent>> PublishDraftAsync(string route, CancellationToken cancellationToken = default)
        {
            const string operation = "PublishDraft";
            try
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    {"PageRoute", route},
                    {"DbOperation", operation}
                });

                var page = await _repository.GetPageWithVersionsAsync(route, cancellationToken);
                if (page == null)
                {
                    return Result<PageContent>.Failure(ErrorCode.NotFound, $"Page '{route}' not found.");
                }

                if (!page.DraftVersionId.HasValue)
                {
                    return Result<PageContent>.Failure(ErrorCode.Validation, "No draft exists to publish.");
                }

                var draft = page.Versions.FirstOrDefault(v => v.Id == page.DraftVersionId && v.Status == PageVersionStatus.Draft);
                if (draft == null)
                {
                    return Result<PageContent>.Failure(ErrorCode.Validation, "Draft version is missing or invalid.");
                }

                var now = DateTime.UtcNow;

                if (page.CurrentVersion != null && page.CurrentVersion.Status == PageVersionStatus.Published)
                {
                    page.CurrentVersion.Status = PageVersionStatus.Archived;
                    page.CurrentVersion.UpdatedUtc = now;
                }

                draft.ContentJson = SanitizeRichTextBlocks(draft.ContentJson);
                draft.Status = PageVersionStatus.Published;
                draft.PublishedUtc = now;
                draft.UpdatedUtc = now;

                page.CurrentVersionId = draft.Id;
                page.DraftVersionId = null;

                await _repository.SaveChangesAsync(cancellationToken);

                var hydrated = await HydrateImageBlocksAsync(draft.ContentJson, cancellationToken);
                return Result<PageContent>.Ok(hydrated);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Publish draft cancelled (Route: {Route}, Operation: {Operation})", route, operation);
                return Result<PageContent>.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Publish draft failed (Route: {Route}, Operation: {Operation})", route, operation);
                return Result<PageContent>.Failure(ErrorCode.Unexpected, "PublishDraftFailed");
            }
        }

        public async Task<Result> DiscardDraftAsync(string route, CancellationToken cancellationToken = default)
        {
            const string operation = "DiscardDraft";
            try
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    {"PageRoute", route},
                    {"DbOperation", operation}
                });

                var page = await _repository.GetPageWithVersionsAsync(route, cancellationToken);
                if (page == null)
                {
                    return Result.Failure(ErrorCode.NotFound, $"Page '{route}' not found.");
                }

                if (!page.DraftVersionId.HasValue)
                {
                    return Result.Failure(ErrorCode.NotFound, "No draft to discard.");
                }

                var draft = page.Versions.FirstOrDefault(v => v.Id == page.DraftVersionId && v.Status == PageVersionStatus.Draft);
                if (draft != null)
                {
                    _repository.RemovePageVersion(draft);
                }

                page.DraftVersionId = null;
                await _repository.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Discard draft cancelled (Route: {Route}, Operation: {Operation})", route, operation);
                return Result.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Discard draft failed (Route: {Route}, Operation: {Operation})", route, operation);
                return Result.Failure(ErrorCode.Unexpected, "DiscardDraftFailed");
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
                    ContentHtml = block.ContentHtml,
                    Content = JsonSerializer.SerializeToElement(obj)
                });
            }

            return new PageContent
            {
                Blocks = hydratedBlocks
            };
        }

        private static PageContent SanitizeRichTextBlocks(PageContent content)
        {
            if (content.Blocks.Count == 0)
            {
                return content;
            }

            var updatedBlocks = new List<PageBlock>(content.Blocks.Count);
            foreach (var block in content.Blocks)
            {
                if (!string.Equals(block.Type, "richText", StringComparison.OrdinalIgnoreCase))
                {
                    updatedBlocks.Add(block);
                    continue;
                }

                var sanitized = RichTextSanitizer.Sanitize(block.ContentHtml ?? string.Empty);
                updatedBlocks.Add(new PageBlock
                {
                    Type = block.Type,
                    ContentHtml = sanitized,
                    Content = block.Content
                });
            }

            return new PageContent
            {
                Blocks = updatedBlocks
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

        private static PageContent SetHeroImage(PageContent content, Guid imageId)
        {
            var blocks = new List<PageBlock>(content.Blocks);
            var heroIndex = -1;
            for (var i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                if (!IsImageBlock(block))
                {
                    continue;
                }

                if (!TryParseObject(block.Content, out var obj))
                {
                    continue;
                }

                var variant = obj["variant"]?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(variant) &&
                    string.Equals(variant, "hero", StringComparison.OrdinalIgnoreCase))
                {
                    heroIndex = i;
                    break;
                }
            }

            if (heroIndex < 0)
            {
                heroIndex = blocks.FindIndex(IsImageBlock);
            }

            if (heroIndex >= 0)
            {
                var existing = blocks[heroIndex];
                if (!TryParseObject(existing.Content, out var obj))
                {
                    obj = new JsonObject();
                }
                obj["imageId"] = imageId;
                obj["variant"] ??= "hero";
                blocks[heroIndex] = new PageBlock
                {
                    Type = existing.Type,
                    ContentHtml = existing.ContentHtml,
                    Content = JsonSerializer.SerializeToElement(obj)
                };
            }
            else
            {
                var obj = new JsonObject
                {
                    ["imageId"] = imageId,
                    ["variant"] = "hero"
                };
                blocks.Insert(0, new PageBlock
                {
                    Type = "image",
                    Content = JsonSerializer.SerializeToElement(obj)
                });
            }

            return new PageContent
            {
                Blocks = blocks
            };
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

        public async Task<Result<List<PageVersionSummary>>> GetPublishedVersionsAsync(string route, CancellationToken cancellationToken = default)
        {
            const string operation = "ListPublishedVersions";
            try
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    {"PageRoute", route},
                    {"DbOperation", operation}
                });

                var page = await _repository.GetPageWithVersionsAsync(route, cancellationToken);
                if (page == null)
                {
                    return Result<List<PageVersionSummary>>.Failure(ErrorCode.NotFound, $"Page '{route}' not found.");
                }

                var versions = await _repository.GetPublishedVersionsAsync(page.Id, cancellationToken);
                var summaries = versions
                    .Select(v => new PageVersionSummary(v.Id, v.VersionNo, v.PublishedUtc, v.ChangeNote))
                    .ToList();

                return Result<List<PageVersionSummary>>.Ok(summaries);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "List published versions cancelled (Route: {Route}, Operation: {Operation})", route, operation);
                return Result<List<PageVersionSummary>>.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "List published versions failed (Route: {Route}, Operation: {Operation})", route, operation);
                return Result<List<PageVersionSummary>>.Failure(ErrorCode.Unexpected, "PublishedVersionsReadFailed");
            }
        }

        public async Task<Result<PageVersionContentResponse>> GetPublishedVersionContentAsync(string route, Guid versionId, CancellationToken cancellationToken = default)
        {
            const string operation = "GetPublishedVersionContent";
            try
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    {"PageRoute", route},
                    {"VersionId", versionId},
                    {"DbOperation", operation}
                });

                var page = await _repository.GetPageWithVersionsAsync(route, cancellationToken);
                if (page == null)
                {
                    return Result<PageVersionContentResponse>.Failure(ErrorCode.NotFound, $"Page '{route}' not found.");
                }

                var version = await _repository.GetVersionByIdAsync(versionId, cancellationToken);
                if (version == null || version.PageId != page.Id)
                {
                    return Result<PageVersionContentResponse>.Failure(ErrorCode.NotFound, "Version not found.");
                }

                if (version.Status != PageVersionStatus.Published && version.Status != PageVersionStatus.Archived)
                {
                    return Result<PageVersionContentResponse>.Failure(ErrorCode.Validation, "Version is not published.");
                }

                var sanitized = SanitizeRichTextBlocks(version.ContentJson);
                var response = new PageVersionContentResponse(sanitized, version.VersionNo);
                return Result<PageVersionContentResponse>.Ok(response);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Get published version content cancelled (Route: {Route}, VersionId: {VersionId}, Operation: {Operation})", route, versionId, operation);
                return Result<PageVersionContentResponse>.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get published version content failed (Route: {Route}, VersionId: {VersionId}, Operation: {Operation})", route, versionId, operation);
                return Result<PageVersionContentResponse>.Failure(ErrorCode.Unexpected, "PublishedVersionReadFailed");
            }
        }

        public async Task<Result<PageContent>> RollbackToVersionAsync(string route, Guid versionId, CancellationToken cancellationToken = default)
        {
            const string operation = "RollbackPublishedVersion";
            try
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    {"PageRoute", route},
                    {"VersionId", versionId},
                    {"DbOperation", operation}
                });

                var page = await _repository.GetPageWithVersionsAsync(route, cancellationToken);
                if (page == null)
                {
                    return Result<PageContent>.Failure(ErrorCode.NotFound, $"Page '{route}' not found.");
                }

                var version = await _repository.GetVersionByIdAsync(versionId, cancellationToken);
                if (version == null || version.PageId != page.Id)
                {
                    return Result<PageContent>.Failure(ErrorCode.NotFound, "Version not found.");
                }

                if (version.Status != PageVersionStatus.Published && version.Status != PageVersionStatus.Archived)
                {
                    return Result<PageContent>.Failure(ErrorCode.BadRequest, "Version must be published to rollback.");
                }

                var now = DateTime.UtcNow;
                if (page.CurrentVersion != null && page.CurrentVersion.Id != version.Id && page.CurrentVersion.Status == PageVersionStatus.Published)
                {
                    page.CurrentVersion.Status = PageVersionStatus.Archived;
                    page.CurrentVersion.UpdatedUtc = now;
                }

                version.Status = PageVersionStatus.Published;
                version.UpdatedUtc = now;
                version.PublishedUtc ??= now;

                page.CurrentVersionId = version.Id;
                await _repository.SaveChangesAsync(cancellationToken);

                var sanitized = SanitizeRichTextBlocks(version.ContentJson);
                var hydrated = await HydrateImageBlocksAsync(sanitized, cancellationToken);
                return Result<PageContent>.Ok(hydrated);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Rollback cancelled (Route: {Route}, VersionId: {VersionId}, Operation: {Operation})", route, versionId, operation);
                return Result<PageContent>.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rollback failed (Route: {Route}, VersionId: {VersionId}, Operation: {Operation})", route, versionId, operation);
                return Result<PageContent>.Failure(ErrorCode.Unexpected, "RollbackFailed");
            }
        }
    }
}
