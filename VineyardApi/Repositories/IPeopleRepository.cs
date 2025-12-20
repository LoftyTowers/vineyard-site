using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public interface IPeopleRepository
    {
        Task<List<Person>> GetActiveByPageIdAsync(Guid pageId, CancellationToken cancellationToken = default);
        Task<List<Person>> GetByPageIdAsync(Guid pageId, CancellationToken cancellationToken = default);
        void Add(Person person);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
