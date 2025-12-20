using Microsoft.EntityFrameworkCore;
using VineyardApi.Data;
using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public class PeopleRepository : IPeopleRepository
    {
        private readonly VineyardDbContext _context;

        public PeopleRepository(VineyardDbContext context)
        {
            _context = context;
        }

        public Task<List<Person>> GetActiveByPageIdAsync(Guid pageId, CancellationToken cancellationToken = default)
        {
            return _context.People
                .AsNoTracking()
                .Where(p => p.PageId == pageId && p.IsActive)
                .OrderBy(p => p.SortOrder)
                .ToListAsync(cancellationToken);
        }

        public Task<List<Person>> GetByPageIdAsync(Guid pageId, CancellationToken cancellationToken = default)
        {
            return _context.People
                .Where(p => p.PageId == pageId)
                .ToListAsync(cancellationToken);
        }

        public void Add(Person person)
        {
            _context.People.Add(person);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
