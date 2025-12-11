using Microsoft.EntityFrameworkCore;
using VineyardApi.Data;
using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly VineyardDbContext _context;
        public AuditRepository(VineyardDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
        {
            return await _context.AuditLogs
                .Include(l => l.User)
                .Include(l => l.History)
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}
