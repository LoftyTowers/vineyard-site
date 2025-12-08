using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public interface IAuditRepository
    {
        Task<List<AuditLog>> GetRecentAsync(int count, CancellationToken cancellationToken);
    }
}
