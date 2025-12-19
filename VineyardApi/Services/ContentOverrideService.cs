using Microsoft.Extensions.Logging;
using VineyardApi.Models;
using VineyardApi.Repositories;

namespace VineyardApi.Services
{
    public class ContentOverrideService : IContentOverrideService
    {
        private readonly IContentOverrideRepository _repository;
        private readonly ILogger<ContentOverrideService> _logger;

        public ContentOverrideService(IContentOverrideRepository repository, ILogger<ContentOverrideService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<Dictionary<string, string>>> GetPublishedOverridesAsync(string route, CancellationToken cancellationToken = default)
        {
            try
            {
                var items = await _repository.GetLatestPublishedAsync(route, cancellationToken);
                return Result<Dictionary<string, string>>.Ok(items.ToDictionary(i => i.BlockKey, i => i.HtmlValue));
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Request cancelled while getting published overrides for route {Route}", route);
                return Result<Dictionary<string, string>>.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting published overrides for route {Route}", route);
                return Result<Dictionary<string, string>>.Failure(ErrorCode.Unexpected);
            }
        }

        public async Task<Result> SaveDraftAsync(ContentOverride model, CancellationToken cancellationToken = default)
        {
            try
            {
                model.Status = "draft";
                model.Timestamp = DateTime.UtcNow;

                var existing = await _repository.GetDraftAsync(model.PageId, model.BlockKey, cancellationToken);
                if (existing == null)
                {
                    _repository.Add(model);
                }
                else
                {
                    existing.HtmlValue = model.HtmlValue;
                    existing.Note = model.Note;
                    existing.ChangedById = model.ChangedById;
                    existing.Timestamp = model.Timestamp;
                }

                await _repository.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Request cancelled while saving draft override for page {PageId} and block {BlockKey}", model.PageId, model.BlockKey);
                return Result.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving draft for page {PageId} and block {BlockKey}", model.PageId, model.BlockKey);
                return Result.Failure(ErrorCode.Unexpected);
            }
        }

        public async Task<Result> PublishAsync(ContentOverride model, CancellationToken cancellationToken = default)
        {
            try
            {
                model.Status = "published";
                model.Timestamp = DateTime.UtcNow;
                _repository.Add(model);
                await _repository.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Request cancelled while publishing override for page {PageId} and block {BlockKey}", model.PageId, model.BlockKey);
                return Result.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing override for page {PageId} and block {BlockKey}", model.PageId, model.BlockKey);
                return Result.Failure(ErrorCode.Unexpected);
            }
        }

        public async Task<Result> PublishDraftAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var draft = await _repository.GetByIdAsync(id, cancellationToken);
                if (draft == null)
                {
                    return Result.Failure(ErrorCode.NotFound, $"Draft with id {id} not found.");
                }

                draft.Status = "published";
                draft.Timestamp = DateTime.UtcNow;
                await _repository.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Request cancelled while publishing draft override {DraftId}", id);
                return Result.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing draft override {DraftId}", id);
                return Result.Failure(ErrorCode.Unexpected);
            }
        }

        public async Task<Result<List<ContentOverride>>> GetHistoryAsync(string route, string blockKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var history = await _repository.GetHistoryAsync(route, blockKey, cancellationToken);
                return Result<List<ContentOverride>>.Ok(history);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Request cancelled while getting history for route {Route} block {BlockKey}", route, blockKey);
                return Result<List<ContentOverride>>.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history for route {Route} block {BlockKey}", route, blockKey);
                return Result<List<ContentOverride>>.Failure(ErrorCode.Unexpected);
            }
        }

        public async Task<Result> RevertAsync(Guid id, Guid changedById, CancellationToken cancellationToken = default)
        {
            try
            {
                var src = await _repository.GetByIdAsync(id, cancellationToken);
                if (src == null)
                {
                    return Result.Failure(ErrorCode.NotFound, $"Override with id {id} not found.");
                }

                var draft = new ContentOverride
                {
                    Id = Guid.NewGuid(),
                    PageId = src.PageId,
                    BlockKey = src.BlockKey,
                    HtmlValue = src.HtmlValue,
                    Status = "draft",
                    Note = src.Note,
                    ChangedById = changedById,
                    Timestamp = DateTime.UtcNow
                };
                _repository.Add(draft);
                await _repository.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Request cancelled while reverting override {OverrideId}", id);
                return Result.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reverting override {OverrideId}", id);
                return Result.Failure(ErrorCode.Unexpected);
            }
        }
    }
}
