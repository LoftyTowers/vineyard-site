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

        public async Task<Dictionary<string, string>> GetPublishedOverridesAsync(string route)
        {
            var items = await _repository.GetLatestPublishedAsync(route);
            return items.ToDictionary(i => i.BlockKey, i => i.HtmlValue);
        }

        public async Task SaveDraftAsync(ContentOverride model)
        {
            model.Status = "draft";
            model.Timestamp = DateTime.UtcNow;

            var existing = await _repository.GetDraftAsync(model.PageId, model.BlockKey);
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
            await _repository.SaveChangesAsync();
        }

        public async Task PublishAsync(ContentOverride model)
        {
            model.Status = "published";
            model.Timestamp = DateTime.UtcNow;
            _repository.Add(model);
            await _repository.SaveChangesAsync();
        }

        public async Task PublishDraftAsync(Guid id)
        {
            var draft = await _repository.GetByIdAsync(id);
            if (draft == null) return;
            draft.Status = "published";
            draft.Timestamp = DateTime.UtcNow;
            await _repository.SaveChangesAsync();
        }

        public Task<List<ContentOverride>> GetHistoryAsync(string route, string blockKey)
        {
            return _repository.GetHistoryAsync(route, blockKey);
        }

        public async Task RevertAsync(Guid id, Guid changedById)
        {
            var src = await _repository.GetByIdAsync(id);
            if (src == null) return;
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
            await _repository.SaveChangesAsync();
        }
    }
}
