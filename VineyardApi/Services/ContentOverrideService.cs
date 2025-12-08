using VineyardApi.Models;
using VineyardApi.Repositories;

namespace VineyardApi.Services
{
    public class ContentOverrideService : IContentOverrideService
    {
        private readonly IContentOverrideRepository _repository;
        public ContentOverrideService(IContentOverrideRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Dictionary<string, string>>> GetPublishedOverridesAsync(string route, CancellationToken cancellationToken)
        {
            var items = await _repository.GetLatestPublishedAsync(route, cancellationToken);
            return Result<Dictionary<string, string>>.Ok(items.ToDictionary(i => i.BlockKey, i => i.HtmlValue));
        }

        public async Task<Result> SaveDraftAsync(ContentOverride model, CancellationToken cancellationToken)
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

        public async Task<Result> PublishAsync(ContentOverride model, CancellationToken cancellationToken)
        {
            model.Status = "published";
            model.Timestamp = DateTime.UtcNow;
            _repository.Add(model);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        public async Task<Result> PublishDraftAsync(Guid id, CancellationToken cancellationToken)
        {
            var draft = await _repository.GetByIdAsync(id, cancellationToken);
            if (draft == null) return Result.Failure(ErrorCode.NotFound, "Draft not found");
            draft.Status = "published";
            draft.Timestamp = DateTime.UtcNow;
            await _repository.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        public async Task<Result<List<ContentOverride>>> GetHistoryAsync(string route, string blockKey, CancellationToken cancellationToken)
        {
            var history = await _repository.GetHistoryAsync(route, blockKey, cancellationToken);
            return Result<List<ContentOverride>>.Ok(history);
        }

        public async Task<Result> RevertAsync(Guid id, Guid changedById, CancellationToken cancellationToken)
        {
            var src = await _repository.GetByIdAsync(id, cancellationToken);
            if (src == null) return Result.Failure(ErrorCode.NotFound, "Override not found");
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
    }
}
