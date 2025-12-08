using VineyardApi.Domain.Content;
using VineyardApi.Models;
using VineyardApi.Repositories;

namespace VineyardApi.Services
{
    public class PageService : IPageService
    {
        private readonly IPageRepository _repository;

        public PageService(IPageRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<PageContent>> GetPageContentAsync(string route, CancellationToken cancellationToken)
        {
            var page = await _repository.GetPageWithOverridesAsync(route, cancellationToken);
            if (page == null)
            {
                return Result<PageContent>.Failure(ErrorCode.NotFound, $"Page '{route}' not found");
            }

            var overrideContent = page.Overrides
                .OrderByDescending(o => o.UpdatedAt)
                .FirstOrDefault();

            return Result<PageContent>.Ok(overrideContent?.OverrideContent ?? page.DefaultContent);
        }

        public async Task<Result> SaveOverrideAsync(PageOverride model, CancellationToken cancellationToken)
        {
            model.UpdatedAt = DateTime.UtcNow;
            var existing = await _repository.GetPageOverrideByPageIdAsync(model.PageId, cancellationToken);
            if (existing == null)
            {
                _repository.AddPageOverride(model);
            }
            else
            {
                existing.OverrideContent = model.OverrideContent;
                existing.UpdatedAt = model.UpdatedAt;
                existing.UpdatedById = model.UpdatedById;
            }
            await _repository.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        
    }
}
